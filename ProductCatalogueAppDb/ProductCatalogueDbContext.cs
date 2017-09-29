﻿using Microsoft.EntityFrameworkCore;
using ProductCatalogueModels;

namespace ProductCatalogueAppDb
{
    public class ProductCatalogueDbContext : DbContext
    {
        public ProductCatalogueDbContext(DbContextOptions<ProductCatalogueDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ProductCategory>()
             .HasKey(t => new { t.ProductId, t.CategoryId });

            builder.Entity<ProductCategory>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.Categories)
                .HasForeignKey(pt => pt.ProductId);

            builder.Entity<ProductCategory>()
                .HasOne(pt => pt.Category)
                .WithMany(t => t.Products)
                .HasForeignKey(pt => pt.CategoryId);
        }

        public DbSet<Category> Category { get; set; }

        public DbSet<Product> Product { get; set; }

    }
}