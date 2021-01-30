using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NpgsqlTypes;

namespace PostgresTest.Models
{
    public class IField<T> {
        public int Id { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public virtual T GetField() {
            return default(T);
        }
    }
    public class Comment
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }
    public class Tag :IField<string>
    {
        public string Text { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Text;
        }
    }
    public class Like
    {
        
        public int Id { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public string UserName { get; set; }
    }
    public class TextField : IField<string>
    {
        public string Text { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Text;
        }
    }
    public class DigitField : IField<int>
    {
        public int Digit { get; set; }
        public override int GetField()
        {
            return Digit;
        }
    }
    public class WordField : IField<string>
    {
        public string Word { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Word;
        }
    }
    public class DateField:IField<DateTime>
    {
        public DateTime Date { get; set; }
        public override DateTime GetField()
        {
            return Date;
        }
    }
    public class CheckboxField : IField<bool>
    {
        public bool Checkbox { get; set; }
        public override bool GetField()
        {
            return Checkbox;
        }
    }
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

    public class Collection
    {
        public int Id { get; set; }
        public string CollectionName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Theme { get; set; } = "";
        public string Image { get; set; } = "";
        public List<string> OptFields { get; set; } = new List<string>(new string[5]);
        public List<Item> Items { get; set; } = new List<Item>();
        public string UserId { get; set; }
        public User User { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
    }
    public class User : IdentityUser
    {
        public List<Collection> Collections { get; set; } = new List<Collection>();
    }

    public class ApplicationContext : IdentityDbContext<User>
        {
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TextField> TextFields { get; set; }
        public DbSet<DigitField> DigitFields { get; set; }
        public DbSet<WordField> WordFields { get; set; }
        public DbSet<DateField> DateFields { get; set; }
        public DbSet<CheckboxField> CheckboxFields { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
                : base(options)
            {
            Database.EnsureCreated();
            }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Collection>()
        .HasGeneratedTsVectorColumn(
            c => c.SearchVector,
            "english",  // Text search config
            c => new { c.CollectionName, c.Description, c.Theme })  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
            modelBuilder.Entity<Item>()
        .HasGeneratedTsVectorColumn(
            i => i.SearchVector,
            "english",  // Text search config
            i => i.Name )  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
            modelBuilder.Entity<TextField>()
        .HasGeneratedTsVectorColumn(
            f => f.SearchVector,
            "english",  // Text search config
            f => f.Text)  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
            modelBuilder.Entity<WordField>()
        .HasGeneratedTsVectorColumn(
            f => f.SearchVector,
            "english",  // Text search config
            f => f.Word)  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
            modelBuilder.Entity<Comment>()
        .HasGeneratedTsVectorColumn(
            c => c.SearchVector,
            "english",  // Text search config
            c => c.Text)  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
            modelBuilder.Entity<Tag>()
        .HasGeneratedTsVectorColumn(
            t => t.SearchVector,
            "english",  // Text search config
            t => t.Text)  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN");
        }
    }
}
