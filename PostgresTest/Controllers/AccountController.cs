using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using PostgresTest.ViewModels;
using PostgresTest.Models;
using Microsoft.AspNetCore.Identity;

namespace PostgresTest.Controllers
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
        public IActionResult Signin()
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
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    await signInManager.SignInAsync(user, false);
                    ViewBag.Status = 2;
                    return Redirect($"/Home/Index/{User.Identity.Name}");
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
        [HttpGet]
        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return Redirect($"/Home/Index/{User.Identity.Name}");
            }
            return View(new LoginViewModel());
        }
        [HttpGet]
        public IActionResult Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                signInManager.SignOutAsync().Wait();
            }
            return Redirect($"/Account/Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await userManager.FindByNameAsync(model.Name);
                if (user != null && (await userManager.GetRolesAsync(user)).Contains("Blocked"))
                {
                    ModelState.AddModelError(string.Empty, "This user is blocked");
                    return View(model);
                }
                var result =
                    await signInManager.PasswordSignInAsync(model.Name, model.Password,false, false);
                if (result.Succeeded)
                {
                    ViewBag.Status = (await userManager.GetRolesAsync(user)).Contains("Admin") ? 1 : 2; ;
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