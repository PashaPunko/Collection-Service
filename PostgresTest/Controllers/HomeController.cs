using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserCollections.Models;
using UserCollections.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CloudinaryDotNet;

namespace UserCollections.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext db;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public HomeController(ApplicationContext context, 
                              UserManager<User> userManager, 
                              SignInManager<User> signInManager)
        {
            db = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        private async Task<int> ValidateStatus()
        {
            if (!User.Identity.IsAuthenticated) return 4;
            else 
            {
                User user =  await userManager.FindByNameAsync(User.Identity.Name);
                if (user == null || userManager.GetRolesAsync(user).GetAwaiter().GetResult().Contains("Blocked"))
                {
                    signInManager.SignOutAsync().Wait();
                    return 3;
                }
                var roles = await userManager.GetRolesAsync(user);
                return roles.Contains
                    ("Admin") ? 1 : 2;
            }
        }
        [Route("Home/Index/{name?}")]
        [Route("/")]
        [HttpGet]
        public async Task<IActionResult> Index(string? name)
        {
            ViewBag.Status = await ValidateStatus();
            if (User.Identity.IsAuthenticated && name is null && ViewBag.Status!=3)
            {
                name = User.Identity.Name;
            }
            var cloudinary = new Cloudinary(ApplicationConstants.cloudinaryAccount);
            ViewBag.Name = name;
            ConstantValues model = new ConstantValues();
            model.Collections = new List<Collection>(db.Collections.Include(col=>col.User).OrderBy(col => col.Items.Count).Take(20));
            model.Items = new List<Item>(db.Items.Include(i=>i.Collection).ThenInclude(col=>col.User).OrderByDescending(i => i.CreationTime).Take(20));
            var tags = from tag in db.Tags
                                       select tag.Text;
            model.Tags = String.Join(" ",tags.ToList());
            return View(model);
        }

    }
}
