using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Linq;
using UserCollections.ViewModels;
using UserCollections.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UserCollections.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> Signin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Signin(SigninViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User{
                    Email = model.Email,
                    UserName = model.Name,
                };
                var result = userManager.CreateAsync(user, model.Password).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    signInManager.SignInAsync(user, false).Wait();
                    ViewBag.Status = 2;
                    return Redirect($"/Home/Index/{model.Name}");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel model = new LoginViewModel {
                ReturnUrl = returnUrl,
                ExternalLogins = signInManager.GetExternalAuthenticationSchemesAsync().GetAwaiter().GetResult().ToList()
            };
            if (remoteError != null) {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", model);
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ModelState.AddModelError(string.Empty, $"Error loading external login iformation");
                return View("Login", model);
            }
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user is null)
                    {
                        user = new User
                        {
                            UserName = email,
                            Email = email
                        };
                        await userManager.CreateAsync(user);
                    }
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            return View("Login", model);
        }
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect($"/Home/Index/{User.Identity.Name}");
            }
            return View(new LoginViewModel { 
                ReturnUrl = returnUrl, 
                ExternalLogins = signInManager.GetExternalAuthenticationSchemesAsync().GetAwaiter().GetResult().ToList()});
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                signInManager.SignOutAsync().Wait();
            }
            return Redirect($"/Account/Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await userManager.FindByNameAsync(model.Name);
                if (user != null && userManager.GetRolesAsync(user).GetAwaiter().GetResult().Contains("Blocked"))
                {
                    ModelState.AddModelError(string.Empty, "This user is blocked");
                    return View(model);
                }
                var result =
                    await signInManager.PasswordSignInAsync(model.Name, model.Password,false, false);
                if (result.Succeeded)
                {
                    ViewBag.Status = userManager.GetRolesAsync(user).GetAwaiter().GetResult().Contains("Admin") ? 1 : 2; ;
                    return Redirect($"/Home/Index/{model.Name}");
                }
                else
                {
                    ModelState.AddModelError("", "Uncorrect login or password");
                }
            }
            return View(model);
        }
    }
}