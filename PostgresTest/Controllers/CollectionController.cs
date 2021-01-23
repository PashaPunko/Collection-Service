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
        private bool Filter<T,U,V>(List<T> fields, List<U> filters) where U:ProxyFilter<V> where T:IField<V>
        {
            for (int i = 0; i < filters.Count; i++)
            {
                if (!filters[i].Filter(fields[i].GetField()))
                {
                    return false;
                }
            }
            return true;
        }
        private List<Item> FilterAndSortItems(CollectionViewModel model, Collection collection) {
            List<Item> items = collection.Items;
            items = (from item in items
                    where model.FilterForName.Filter(item.Name)
                    where model.FilterForTags.Filter(item.Tags.ConvertAll(t => t.Text))
                    where Filter<TextField, ProxyFilterForText, string>(item.TextFields,
                                            model.FilterForText)
                    where Filter<DigitField, ProxyFilterForDigit, int>(item.DigitFields,
                                            model.FilterForDigit)
                    where Filter<WordField, ProxyFilterForText, string>(item.WordFields,
                                            model.FilterForWord)
                    where Filter<DateField, ProxyFilterForDate, DateTime>(item.DateFields,
                                            model.FilterForDate)
                    where Filter<CheckboxField, ProxyFilterForCheckbox, bool>(item.CheckboxFields,
                                            model.FilterForBoolean)
                    select item).ToList();
            items.Sort(delegate (Item x, Item y)
            {
                switch (model.SortASC.Key)
                {
                    case 1:
                        if (model.SortASC.Value) return x.Name.CompareTo(y.Name);
                        else return x.Name.CompareTo(y.Name);
                    case 2:
                        if (model.SortASC.Value) return x.Tags.Count.CompareTo(y.Tags.Count);
                        else return y.Tags.Count.CompareTo(x.Tags.Count);
                    default:
                        {
                            int counter = 3;
                            if (model.SortASC.Key >= counter && model.SortASC.Key < counter + x.DigitFields.Count)
                            {
                                if (model.SortASC.Value)
                                    return x.DigitFields[model.SortASC.Key - counter].Digit.
                                    CompareTo(y.DigitFields[model.SortASC.Key - counter].Digit);
                                else return y.DigitFields[model.SortASC.Key - counter].Digit.
                                    CompareTo(x.DigitFields[model.SortASC.Key - counter].Digit);
                            }
                            else return x.Name.CompareTo(y.Name);
                        }

                }
            });
            return items;
        }
        [Route("Index/{id}/{name?}")]
        [HttpGet]
        public async Task<IActionResult> Index(string? name, int id, CollectionViewModel? model, KeyValuePair<int, bool> sortId)
        {
            ViewBag.Status = await ValidateStatus();
            Collection collection = null;
            collection = db.Collections.
                Include(col => col.Items).ThenInclude(item => item.TextFields).
                Include(col => col.Items).ThenInclude(item => item.DigitFields).
                Include(col => col.Items).ThenInclude(item => item.WordFields).
                Include(col => col.Items).ThenInclude(item => item.DateFields).
                Include(col => col.Items).ThenInclude(item => item.CheckboxFields).
                Include(col => col.Items).ThenInclude(item => item.Tags).
                FirstOrDefault(col => col.Id == id);
            if (collection is null)
            {
                return Redirect($"/Profile/Index/{name}");
            }
            if (model.SortASC.Key == 0)
            {
                model = new CollectionViewModel();

                for (int i = 0; i < 5; i++)
                {
                    model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                }
                model.SortASC = new KeyValuePair<int, bool>(1, true);
                model.FilterForDigit = (new ProxyFilterForDigit[model.optFields[0].Count]).ToList();
                model.FilterForDigit = model.FilterForDigit.ConvertAll(f => new ProxyFilterForDigit());
                model.FilterForText = (new ProxyFilterForText[model.optFields[1].Count]).ToList();
                model.FilterForText = model.FilterForText.ConvertAll(f =>  new ProxyFilterForText());
                model.FilterForWord = (new ProxyFilterForText[model.optFields[2].Count]).ToList();
                model.FilterForWord = model.FilterForWord.ConvertAll(f => new ProxyFilterForText());
                model.FilterForDate = (new ProxyFilterForDate[model.optFields[3].Count]).ToList();
                model.FilterForDate = model.FilterForDate.ConvertAll(f =>  new ProxyFilterForDate());
                model.FilterForBoolean = (new ProxyFilterForCheckbox[model.optFields[4].Count]).ToList();
                model.FilterForBoolean = model.FilterForBoolean.ConvertAll(f => new ProxyFilterForCheckbox());
                model.FilterForName = new ProxyFilterForText();
                var tags = from tag in db.Tags
                           group tag by tag.Text into g
                           select (g.Key);
                model.FilterForTags = new ProxyFilterForTags { FilterTags = String.Join(' ', tags) };
            }
            model.Items = FilterAndSortItems(model, collection);
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
                    model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                   
                }
                model.Item.DigitFields = new List<DigitField>(new DigitField[model.optFields[0].Count]);
                model.Item.TextFields = new List<TextField>(new TextField[model.optFields[1].Count]);
                model.Item.WordFields = new List<WordField>(new WordField[model.optFields[2].Count]);
                model.Item.DateFields = new List<DateField>(new DateField[model.optFields[3].Count]);
                model.Item.CheckboxFields = new List<CheckboxField>(new CheckboxField[model.optFields[4].Count]);
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
                model.Item.Collection=collection;
                model.Item.CollectionId=collection.Id;
                model.Item.CreationTime=DateTime.Now;
                
                List<Tag> tags = new List<Tag>();
                foreach (string el in model.TagString.Split(' ')) {
                    tags.Add(new Tag { Item = model.Item, ItemId = model.Item.Id, Text = el });
                }
                model.Item.DigitFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; });
                model.Item.TextFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; });
                model.Item.WordFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; });
                model.Item.DateFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; });
                model.Item.CheckboxFields.ForEach(f => { f.Item = model.Item; f.ItemId = model.Item.Id; });
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
                    model.optFields[i] = new List<string>(collection.OptFields[i].Split(' '));
                }
                model.Item = db.Items.Include(i=>i.DigitFields).
                    Include(i => i.TextFields).
                    Include(i => i.WordFields).
                    Include(i => i.DateFields).
                    Include(i => i.CheckboxFields).
                    Include(i => i.Tags).
                    FirstOrDefault(i => i.Id == itemId);
                if (model.Item is null) {
                    return Redirect($"/Collection/Index/{id}/{name}");
                }
                List<string> tags = new List<string>();
                for (int i = 0; i < model.Item.Tags.Count; i++) {
                    tags.Add(model.Item.Tags[i].Text);
                }
                model.TagString = String.Join(' ', tags);
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
                item.Name = model.Item.Name;
                List<Tag> tags = new List<Tag>();
                db.Tags.RemoveRange(item.Tags);
                foreach (string el in model.TagString.Split(' '))
                {
                    tags.Add(new Tag { Item = item, ItemId = item.Id, Text = el });
                }
                db.Tags.AddRange(tags);
                for (int i = 0; i < model.Item.TextFields.Count;i++) { 
                    item.TextFields[i].Text = model.Item.TextFields[i].Text;
                }
                for (int i = 0; i < model.Item.DigitFields.Count; i++)
                {
                    item.DigitFields[i].Digit = model.Item.DigitFields[i].Digit;
                }
                for (int i = 0; i < model.Item.WordFields.Count; i++)
                {
                    item.WordFields[i].Word = model.Item.WordFields[i].Word;
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
            var collection = db.Collections.Include(c => c.Items).FirstOrDefault(c => c.Id==id);
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
