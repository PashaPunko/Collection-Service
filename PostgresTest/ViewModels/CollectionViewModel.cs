using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

namespace PostgresTest.ViewModels
{
    public class CollectionViewModel
    {
        public List<List<string>> optFields { get; set; } = new List<List<string>>(new List<string>[5]);
        public string CollectionName { get; set; }
        public string Discription { get; set; }
        public string Theme { get; set; }
        public string Image { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }
}