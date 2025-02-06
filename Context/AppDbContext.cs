using Microsoft.EntityFrameworkCore;
using treinamento_estagiarios.Models;

namespace treinamento_estagiarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProduct>()
                .HasKey(up => new { up.UserId, up.ProductId });

            modelBuilder.Entity<UserProduct>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProducts)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProduct>()
                .HasOne(up => up.Product)
                .WithMany(p => p.UserProducts)
                .HasForeignKey(up => up.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}