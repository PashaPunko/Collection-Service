﻿using System;
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
using Microsoft.AspNetCore.Authorization;
namespace UserCollections.Controllers
{
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ApplicationContext bd;
        public AdminController(RoleManager<IdentityRole> roleManager, 
                              UserManager<User> userManager, 
                              SignInManager<User> signInManager, 
                              ApplicationContext bd)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.bd = bd;
        }
        private async Task<bool> ValidateStatus()
        {
            User user = await userManager.FindByNameAsync(this.User.Identity.Name);
            if (user == null || (await userManager.GetRolesAsync(user)).Contains("Blocked") ||
                !(await userManager.GetRolesAsync(user)).Contains("Admin"))
            {
                signInManager.SignOutAsync().Wait();
                return false;
            }
            return true;
        }
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!await ValidateStatus())
                {
                    return RedirectToAction("Index", "Home");
                }
                Users model = new Users();
                foreach (var user in userManager.Users.ToList()) {
                    var userRoles = await userManager.GetRolesAsync(user);
                    SortedDictionary<string, bool> roles = new SortedDictionary<string, bool>();
                    foreach (var role in roleManager.Roles) {
                        roles.Add(role.Name, userRoles.Contains(role.Name) ? true : false);
                    }
                    model.AllUsers.Add(new UserViewModel
                    {
                        UserId = user.Id,
                        UserEmail = user.Email,
                        UserName = user.UserName,
                        UserRoles = roles,
                        IsChecked = false
                    });
                }
                ViewBag.Status = 1;
                return View(model);
            }
            else return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(Users model)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!await ValidateStatus())
                {
                    return RedirectToAction("Login", "Account");
                }
                foreach (var el in model.AllUsers)
                {
                    if (el.IsChecked)
                    {
                        User user = userManager.Users.Include(u=>u.Collections).Where(u => u.Id == el.UserId).FirstOrDefault();
                        bd.Collections.RemoveRange(user.Collections);
                        await userManager.DeleteAsync(user);
                        bd.SaveChanges();
                    }
                };
                return RedirectToAction("Index", "Admin");
            }
            else return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(Users model)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!await ValidateStatus())
                {
                     return RedirectToAction("Index", "Home");
                }
                foreach (var el in model.AllUsers)
                {
                    if (el.IsChecked)
                    {
                        var roles = el.UserRoles.Where(r => r.Value).Select(r => r.Key).ToList();
                        User user = userManager.Users.Where(u => u.Id == el.UserId).FirstOrDefault();
                        var userRoles = await userManager.GetRolesAsync(user);
                        var addedRoles = roles.Except(userRoles);
                        var removedRoles = userRoles.Except(roles);
                        await userManager.AddToRolesAsync(user, addedRoles);
                        await userManager.RemoveFromRolesAsync(user, removedRoles);
                    }
                };
                return RedirectToAction("Index", "Admin");
            }
            else return RedirectToAction("Index", "Home");
        }

    }
}
