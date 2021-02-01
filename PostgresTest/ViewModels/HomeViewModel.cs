using System.ComponentModel.DataAnnotations;
using UserCollections.Models;
using System.Collections.Generic;

namespace UserCollections.ViewModels
{
    public class ConstantValues
    {
        public string Tags { get; set; } = "";
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }
}