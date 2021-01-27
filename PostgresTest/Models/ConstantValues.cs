using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;

namespace PostgresTest
{
    public static class ApplicationConstants {
        public static List<string> Themes { get; set; } = new List<string> {"Books", "Food", "Drinks", 
            "Places", "Telephones", "Toys", "Games", "Clothes", "Tickets", "Money", "Signs", "Animals", "Plants",
        "Music", "URLs", "Other"};
    }
}