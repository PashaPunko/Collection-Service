using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NpgsqlTypes;

namespace UserCollections.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string CollectionName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Theme { get; set; } = "";
        public string Image { get; set; } = "";
        public List<string> OptFields { get; set; } = new List<string>(new string[5]);
        public List<Item> Items { get; set; } = new List<Item>();
        public string UserId { get; set; }
        public User User { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }
}
