using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PostgresTest.Models;
using PostgresTest.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace PostgresTest.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private ApplicationContext db;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public HomeController(ApplicationContext context, UserManager<User> userManager, SignInManager<User> signInManager)
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
        [Route("Index/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Index(string? name)
        {
            if (User.Identity.IsAuthenticated && name is null)
            {
                name = User.Identity.Name;
            }
            ViewBag.Status = await ValidateStatus();
            ViewBag.Name = name;
            HomeViewModel model = new HomeViewModel();
            model.Collections = new List<Collection>(db.Collections.Include(col=>col.User).OrderBy(col => col.Items.Count).Take(20));
            model.Items = new List<Item>(db.Items.Include(i=>i.Collection).ThenInclude(col=>col.User).OrderBy(i => i.CreationTime).Take(20));
            var tags = from tag in db.Tags
                                       group tag by tag.Text into g
                                       select new KeyValuePair<string, int>(g.Key,g.Count());
            model.Tags = tags.ToList();
            return View(model);
        }

    }
}
