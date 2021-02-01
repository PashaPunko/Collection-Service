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
