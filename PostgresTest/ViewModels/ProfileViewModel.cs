using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;

namespace PostgresTest.ViewModels
{
    public class ProfileViewModel
    {
        public Collection Collection { get; set; }
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }
}