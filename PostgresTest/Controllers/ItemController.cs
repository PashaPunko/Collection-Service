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
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;

namespace PostgresTest
{
    public class CommentsHub : Hub
    {
    }
}
namespace PostgresTest.Controllers
{
    [Route("[controller]")]
    public class ItemController : Controller
    {
        private ApplicationContext db;
        private IHubContext<CommentsHub> hubContext;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public ItemController(ApplicationContext context, IHubContext<CommentsHub> hubContext, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            this.hubContext = hubContext;
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
        [Route("Index/{id}/{itemId}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Index(string? name, int id, int itemId)
        {
            ViewBag.Status = await ValidateStatus();
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection is null)
            {
                Redirect($"/Profile/Index/{name}");
            }
            Item item = db.Items.
                Include(item => item.TextFields).
                Include(item => item.DigitFields).
                Include(item => item.WordFields).
                Include(item => item.DateFields).
                Include(item => item.CheckboxFields).
                Include(item => item.Comments).
                Include(item => item.Likes).
                FirstOrDefault(col => col.Id == itemId);
            if (item is null)
            {
                return Redirect($"/Collection/Index/{id}/{name}");
            }
            ItemViewModel model = new ItemViewModel();
            model.Item = item;
            for (int i = 0; i < 5; i++)
            {
                model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
            }
            ViewBag.Name = name;
            ViewBag.CollectionId = id;
            return View(model);
        }
        [Route("Like/{itemId}/{name}")]
        [HttpPost]
        public async Task Like(int id, int itemId, string name)
        {
            var like = db.Likes.FirstOrDefault(l => l.ItemId == itemId && l.UserName == name);
            var item = db.Items.FirstOrDefault(i => i.Id == itemId);
            if (like is null)
            {
                like = new Like { UserName = name, Item = item, ItemId = item.Id };
                item.Likes.Add(like);
                db.Likes.Add(like);
            }
            else db.Likes.Remove(like);
            await db.SaveChangesAsync();
        }
        [Route("Join")]
        [HttpPost]
        public async Task Join(string connectionId, string itemId)
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, itemId);
        }
        [Route("Send")]
        [HttpPost]
        public async Task Send( string itemId, string message)
        {
            await hubContext.Clients.Group(itemId).SendAsync("Notify",message, User.Identity.Name);
        }

    }
}
