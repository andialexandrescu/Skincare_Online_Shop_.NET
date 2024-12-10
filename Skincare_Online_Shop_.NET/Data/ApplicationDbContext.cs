using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Models;
using static Skincare_Online_Shop_.NET.Models.CartProducts;

namespace Skincare_Online_Shop_.NET.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartProduct> CartProducts { get; set; }

        // definirea relatiei many-to-many dintre Product si Cart
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // stergere review-uri in cascada pt stergerea unui produs caruia ii corespund review-urile respective
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .OnDelete(DeleteBehavior.Cascade);
            // similar pentru categorii
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .OnDelete(DeleteBehavior.Cascade);

            // definire pk compus
            modelBuilder.Entity<CartProduct>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.CartId });

            // definire relatii cu modelele Product si Cart (FK)
            modelBuilder.Entity<CartProduct>()
                .HasOne(ab => ab.Product)
                .WithMany(ab => ab.CartProducts)
                .HasForeignKey(ab => ab.ProductId);
            modelBuilder.Entity<CartProduct>()
                .HasOne(ab => ab.Cart)
                .WithMany(ab => ab.CartProducts)
                .HasForeignKey(ab => ab.CartId);
        }
    }
}
