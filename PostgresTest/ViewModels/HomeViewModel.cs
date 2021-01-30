using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;

namespace PostgresTest.ViewModels
{
    public class ConstantValues
    {
        public string Tags { get; set; } = "";
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }
}