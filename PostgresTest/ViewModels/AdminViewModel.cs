using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PostgresTest.Models
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public SortedDictionary<string, bool> UserRoles { get; set; }
        public bool IsChecked { get; set; }
        public UserViewModel()
        {
            
            UserRoles = new SortedDictionary<string, bool>();
        }
    }
    public class Users
    {
        public IList<UserViewModel> AllUsers { get; set; }
        public Users()
        {
            AllUsers = new List<UserViewModel>();
        }
    }
    
}
