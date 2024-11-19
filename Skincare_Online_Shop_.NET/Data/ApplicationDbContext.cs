using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Models;

namespace Skincare_Online_Shop_.NET.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne<Category>(a => a.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(a => a.CategoryId);

            modelBuilder.Entity<Review>()
                .HasOne<Product>(a => a.Product)
                .WithMany(c => c.Reviews)
                .HasForeignKey(a => a.ProductId);
        }
    }
}
