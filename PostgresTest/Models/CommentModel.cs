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
    public class Comment
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }
}
