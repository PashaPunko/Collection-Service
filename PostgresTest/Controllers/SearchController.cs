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
    public class SearchController : Controller
    {
        private ApplicationContext db;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public SearchController(ApplicationContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        private async Task<int> ValidateStatus()
        {
            if (this.User.Identity.Name != null)
            {
                User user = await userManager.FindByNameAsync(this.User.Identity.Name);
                if (user == null || (await userManager.GetRolesAsync(user)).Contains("Blocked"))
                {
                    signInManager.SignOutAsync().Wait();
                    return 3;
                }
                var roles = await userManager.GetRolesAsync(user);
                return roles.Contains("Admin") ? 1 : 2;
            }
            return 3;
        }
        [Route("Index/{name?}")]
        [HttpGet]
        public IActionResult Index(string? name, string searchString)
        {
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            List<Item> model = new List<Item>();
            model.AddRange(db.Items.Include(i => i.Tags).Include(i => i.Collection).ThenInclude(c => c.User)
    .Where(p => p.SearchVector.Matches(searchString))
    .ToList());
            var collections = db.Collections
    .Where(p => p.SearchVector.Matches(searchString)).Select(c => c.Items)
    .ToList();
            foreach (var items in collections)
            {
                model.AddRange(items);
            }
            model.AddRange(db.Comments
    .Where(p => p.SearchVector.Matches(searchString)).Select(c => c.Item)
    .ToList());
            model.AddRange(db.TextFields
    .Where(p => p.SearchVector.Matches(searchString)).Select(c => c.Item)
    .ToList());
            model.AddRange(db.WordFields
    .Where(p => p.SearchVector.Matches(searchString)).Select(c => c.Item)
    .ToList());
            ViewBag.Name = name;
            model = model.Distinct().ToList();
            return View(model);
        }

    }
}
