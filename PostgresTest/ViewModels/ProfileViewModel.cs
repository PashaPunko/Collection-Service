using System.ComponentModel.DataAnnotations;
using UserCollections.Models;
using System.Collections.Generic;

namespace UserCollections.ViewModels
{
    public class ProfileViewModel
    {
        public Collection Collection { get; set; }
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }
}