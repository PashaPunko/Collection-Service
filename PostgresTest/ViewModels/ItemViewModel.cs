using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;

namespace PostgresTest.ViewModels
{
    public class ItemViewModel
    {
        public List<List<string>> optFields { get; set; } = new List<List<string>>(new List<string>[5]);
        public Item Item { get; set; } = new Item();
        public string TagString { get; set; } = "";
        public string Tags { get; set; } = "";
    }
}