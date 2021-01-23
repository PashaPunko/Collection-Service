using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;

namespace PostgresTest.ViewModels
{
    public class HomeViewModel
    {
        public List<KeyValuePair<string, int>> Tags { get; set; } = new List<KeyValuePair<string, int>>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }
}