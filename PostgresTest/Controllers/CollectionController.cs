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
    public class CollectionController : Controller
    {
        private ApplicationContext db;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public CollectionController(ApplicationContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        private async Task<int> ValidateStatus()
        {
            if (!User.Identity.IsAuthenticated) return 3;
            else
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
        }


        [Route("Index/{id}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Index(string? name, CollectionViewModel model, int id)
        {
            ViewBag.Status = await ValidateStatus();
            Collection collection = db.Collections.
                Include(col => col.Items).ThenInclude(item => item.TextFields).
                Include(col => col.Items).ThenInclude(item => item.DigitFields).
                Include(col => col.Items).ThenInclude(item => item.WordFields).
                Include(col => col.Items).ThenInclude(item => item.DateFields).
                Include(col => col.Items).ThenInclude(item => item.CheckboxFields).
                Include(col => col.Items).ThenInclude(item => item.Tags).
                Include(col => col.User).
                FirstOrDefault(col => col.Id == id);
            if (collection is null)
            {
                return Redirect($"/Profile/Index/{name}");
            }
            for (int i = 0; i < 5; i++)
            {
                if (collection.OptFields[i] is null)
                {
                    model.optFields[i] = new List<string>();
                }
                else {
                    model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                }
                
            }
            model.Items = collection.Items;
            model.CollectionName = collection.CollectionName;
            model.Discription = collection.Description;
            model.Owner = collection.User.UserName;
            model.Image = collection.Image;
            model.Theme = collection.Theme;
            ViewBag.Name = name;
            ViewBag.CollectionId = id;
            return View(model);
        }
        [Route("Create/{id}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Create(string? name, int id)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection != null)
            {
                ItemViewModel model = new ItemViewModel();
                for (int i = 0; i < 5; i++)
                {
                    if (!(collection.OptFields[i] is null))
                        model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                    else model.optFields[i] = new List<string>();
                }
                model.Item.DigitFields = new List<DigitField>(new DigitField[model.optFields[0].Count]);
                model.Item.TextFields = new List<TextField>(new TextField[model.optFields[1].Count]);
                model.Item.WordFields = new List<WordField>(new WordField[model.optFields[2].Count]);
                model.Item.DateFields = new List<DateField>(new DateField[model.optFields[3].Count]);
                model.Item.CheckboxFields = new List<CheckboxField>(new CheckboxField[model.optFields[4].Count]);
                var tags = from tag in db.Tags
                           select tag.Text;
                model.Tags = String.Join(" ", tags.ToList().Distinct());
                ViewBag.Name = name;
                ViewBag.CollectionId = id;
                return View(model);
            }
            else return Redirect($"/Profile/Index/{name}");
        }
        [Route("Create/{id}/{name?}")]
        [HttpPost]
        public async Task<IActionResult> Create(string? name, int id, ItemViewModel model)
        {

            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection != null)
            {
                model.TagString = model.TagString is null ? "" : model.TagString;
                model.Item.Name = model.Item.Name is null ? "Default Name" : model.Item.Name;
                model.Item.Collection = collection;
                model.Item.CollectionId = collection.Id;
                model.Item.CreationTime = DateTime.Now;
                List<Tag> tags = new List<Tag>();
                foreach (string el in model.TagString.Split(' '))
                {
                    if (!(el == "" || el is null))
                    tags.Add(new Tag { Item = model.Item, ItemId = model.Item.Id, Text = el });
                }
                model.Item.DigitFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id;});
                model.Item.TextFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; 
                    f.Text = f.Text is null ? "" : f.Text;
                });
                model.Item.WordFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id;
                    f.Word = f.Word is null ? "" : f.Word;
                });
                model.Item.DateFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id;});
                model.Item.CheckboxFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id;});
                db.Items.Add(model.Item);
                db.Tags.AddRange(tags);
                db.DigitFields.AddRange(model.Item.DigitFields);
                db.TextFields.AddRange(model.Item.TextFields);
                db.WordFields.AddRange(model.Item.WordFields);
                db.DateFields.AddRange(model.Item.DateFields);
                db.CheckboxFields.AddRange(model.Item.CheckboxFields);
                await db.SaveChangesAsync();
                return Redirect($"/Collection/Index/{id}/{name}");
            }
            else return Redirect($"/Profile/Index/{name}");
        }
        [Route("Edit/{id}/{itemId}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Edit(string? name, int id, int itemId)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection != null)
            {
                ItemViewModel model = new ItemViewModel();
                for (int i = 0; i < 5; i++)
                {
                    if (!(collection.OptFields[i] is null))
                        model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                    else model.optFields[i] = new List<string>();
                }
                model.Item = db.Items.Include(i => i.DigitFields).
                    Include(i => i.TextFields).
                    Include(i => i.WordFields).
                    Include(i => i.DateFields).
                    Include(i => i.CheckboxFields).
                    Include(i => i.Tags).
                    FirstOrDefault(i => i.Id == itemId);
                if (model.Item is null)
                {
                    return Redirect($"/Collection/Index/{id}/{name}");
                }
                List<string> tags = model.Item.Tags.ConvertAll(t=>t.Text);
                model.TagString = String.Join(", ", tags);
                var alltags = from tag in db.Tags
                           select tag.Text;
                model.Tags = String.Join(" ", alltags.ToList().Distinct());
                ViewBag.Name = name;
                ViewBag.CollectionId = id;
                return View(model);
            }
            else return Redirect($"/Profile/Index/{name}");
        }
        [Route("Edit/{id}/{itemId}/{name?}")]
        [HttpPost]
        public async Task<IActionResult> Edit(string? name, int id, int itemId, ItemViewModel model)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection != null)
            {
                Item item = db.Items.Include(i => i.DigitFields).
                    Include(i => i.TextFields).
                    Include(i => i.WordFields).
                    Include(i => i.Tags).
                    Include(i => i.DateFields).
                    Include(i => i.CheckboxFields).
                    FirstOrDefault(i => i.Id == itemId);
                if (item is null)
                {
                    return Redirect($"/Collection/Index/{id}/{name}");
                }
                model.TagString = model.TagString is null ? "" : model.TagString;
                item.Name = model.Item.Name is null ? "Default Name" : model.Item.Name;
                List<Tag> tags = new List<Tag>();
                db.Tags.RemoveRange(item.Tags);
                foreach (string el in model.TagString.Split(' '))
                {
                    if (!(el == "" || el is null))
                        tags.Add(new Tag { Item = item, ItemId = item.Id, Text = el });
                }
                db.Tags.AddRange(tags);
                for (int i = 0; i < model.Item.TextFields.Count; i++)
                {
                    item.TextFields[i].Text = model.Item.TextFields[i].Text is null ? "" : model.Item.TextFields[i].Text;
                }
                for (int i = 0; i < model.Item.DigitFields.Count; i++)
                {
                    item.DigitFields[i].Digit = model.Item.DigitFields[i].Digit;
                }
                for (int i = 0; i < model.Item.WordFields.Count; i++)
                {
                    item.WordFields[i].Word = model.Item.WordFields[i].Word is null ? "" : model.Item.WordFields[i].Word;
                }
                for (int i = 0; i < model.Item.DateFields.Count; i++)
                {
                    item.DateFields[i].Date = model.Item.DateFields[i].Date;
                }
                for (int i = 0; i < model.Item.CheckboxFields.Count; i++)
                {
                    item.CheckboxFields[i].Checkbox = model.Item.CheckboxFields[i].Checkbox;
                }
                db.SaveChanges();
                return Redirect($"/Collection/Index/{id}/{name}");
            }
            else return Redirect($"/Profile/Index/{name}");
        }
        [Route("Delete/{id}/{itemId}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, string? name, int itemId)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Item item = db.Items.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
            {
                return Redirect($"/Collection/Index/{id}/{name}");
            }
            var collection = db.Collections.Include(c => c.Items).FirstOrDefault(c => c.Id == id);
            if (collection is null)
            {
                return Redirect($"/Profile/Index/{name}");
            }
            collection.Items.Remove(item);
            db.Items.Remove(item);
            db.SaveChanges();
            return Redirect($"/Collection/Index/{id}/{name}");
        }
    }
}
