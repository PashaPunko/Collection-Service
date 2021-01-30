using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;
using CloudinaryDotNet;

namespace PostgresTest
{
    public static class ApplicationConstants {
        public static List<string> Themes { get; set; } = new List<string> {"Books", "Food", "Drinks", 
            "Places", "Telephones", "Toys", "Games", "Clothes", "Tickets", "Money", "Signs", "Animals", "Plants",
        "Music", "URLs", "Other"};
        public static Account cloudinaryAccount = new Account(
                "dcdh3xiuj",
                "792633569332572",
                "lHTZd2k2iF6w-712lF9BXjjjgxA");
        public static string DefaultPicture = "https://res.cloudinary.com/dcdh3xiuj/image/upload/v1612028560/unnamed_m20wd6.jpg";
    }
}