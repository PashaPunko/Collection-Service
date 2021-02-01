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
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace UserCollections.Controllers
{
    [Route("[controller]")]
    public class ProfileController : Controller
    {
        private ApplicationContext db;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        public ProfileController(ApplicationContext context, 
                                 UserManager<User> userManager, 
                                 SignInManager<User> signInManager)
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
        [Route("Index/{name}")]
        [HttpGet]
        public async Task<IActionResult> Index(string name)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            User user = db.Users.Include(u => u.Collections).
                FirstOrDefault(user => user.UserName == name);
            ProfileViewModel model = new ProfileViewModel();
            if (user != null) {
                List<Collection> coll = user.Collections;
                model.Collections = coll;
                ViewBag.Name = name;
            }
            return View(model);
        }
        [Route("Create/{name}")]
        [HttpGet]
        public async Task<IActionResult> Create(string name)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection model = new Collection();
            
            ViewBag.Name = name;
            return View(model);
        }
        [Route("Create/{name}")]
        [HttpPost]
        public async Task<IActionResult> Create( string name, Collection model)
        {
            model.CollectionName = model.CollectionName is null ? "Default Name" : model.CollectionName;
            model.Description = model.Description is null ? "" : model.Description;
            model.Theme = model.Theme is null ? "Other" : model.Theme;
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            var user = db.Users.FirstOrDefault(user => user.UserName == name);
            model.User = user;
            model.UserId = user.Id;
            db.Collections.Add(model);
            db.SaveChanges();
            return Redirect($"/Profile/Index/{name}");
        }
        [Route("Upload/{name}")]
        [HttpPost]
        public IActionResult Upload(string name, IFormFile file)
        {

            Cloudinary cloudinary = new Cloudinary(ApplicationConstants.cloudinaryAccount);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.Name, file.OpenReadStream())
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            return Json(uploadResult.Url);
        }
        [Route("Delete/{name}/{id}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, string name)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection collection = db.Collections.FirstOrDefault(col => col.Id == id);
            if (collection != null)
            {
                var user = db.Users.FirstOrDefault(u => u.UserName == name);
                db.Collections.Remove(collection);
                db.SaveChanges();
            }
            return Redirect($"/Profile/Index/{name}");
        }
        [Route("Edit/{name}/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(string name, int id)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            
            Collection model = db.Collections.FirstOrDefault(col => col.Id == id);
            if (model != null)
            {
                ViewBag.Name = name;
                return View(model);
            }
            else return Redirect($"/Profile/Index/{name}");
        }
        [Route("Edit/{name}/{id}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Collection collection, string name, int id)
        {
            ViewBag.Status = await ValidateStatus();
            if (ViewBag.Status == 3 || (User.Identity.Name != name && ViewBag.Status == 2))
            {
                return RedirectToAction("Index", "Home");
            }
            Collection coll = db.Collections.FirstOrDefault(col => col.Id == id);
            if (coll != null)
            {
                collection.CollectionName = collection.CollectionName is null ? "Default Name" : collection.CollectionName;
                collection.Description = collection.Description is null ? "" : collection.Description;
                collection.Theme = collection.Theme is null ? "Other" : collection.Theme;
                coll.Image = collection.Image;
                coll.CollectionName = collection.CollectionName;
                coll.Theme = collection.Theme;
                coll.Description = collection.Description;
                db.SaveChanges();
            }
            return Redirect($"/Profile/Index/{name}");
        }

    }
}
