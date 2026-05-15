using Asisya.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Asisya.Infrastructure.Data;

public class AsisyaDbContext : DbContext
{
    public AsisyaDbContext(DbContextOptions<AsisyaDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryID);
            entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(50);
            entity.HasIndex(c => c.CategoryName).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.SupplierID);
            entity.Property(s => s.CompanyName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductID);
            entity.Property(p => p.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.UnitPrice).HasColumnType("numeric(18,2)");
            entity.HasIndex(p => p.ProductName);
            entity.HasIndex(p => p.CategoryID);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryID)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.SupplierID)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserID);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Role).HasMaxLength(20);
        });
    }
}
