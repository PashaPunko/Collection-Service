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
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public Collection Collection { get; set; }
        public int CollectionId { get; set; }
        public DateTime CreationTime { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<TextField> TextFields { get; set; } = new List<TextField>();
        public List<DigitField> DigitFields { get; set; } = new List<DigitField>();
        public List<WordField> WordFields { get; set; } = new List<WordField>();
        public List<DateField> DateFields { get; set; } = new List<DateField>();
        public List<CheckboxField> CheckboxFields { get; set; } = new List<CheckboxField>();
        public NpgsqlTsVector SearchVector { get; set; }
    }
}
