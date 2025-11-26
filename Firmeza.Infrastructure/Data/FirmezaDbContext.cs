using Firmeza.Admin.Models;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApplicationUser = Firmeza.Infrastructure.Identity.ApplicationUser;

namespace Firmeza.Infrastructure.Data;

public class FirmezaDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public FirmezaDbContext(DbContextOptions<FirmezaDbContext> options)
        : base(options)
    {
    }

    // === ENTIDADES PRINCIPALES ===
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =====================================================
        // CUSTOMER CONFIGURATION
        // =====================================================
        builder.Entity<Customer>()
            .HasIndex(c => c.DocumentNumber)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber)
            .IsUnique();

        // =====================================================
        // SALE CONFIGURATION
        // =====================================================
        builder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .Property(s => s.Total)
            .HasPrecision(12, 2);

        builder.Entity<Sale>()
            .Property(s => s.Subtotal)
            .HasPrecision(12, 2);

        builder.Entity<Sale>()
            .Property(s => s.Tax)
            .HasPrecision(12, 2);

        // ENUM mapeado como string en DB
        builder.Entity<Sale>()
            .Property(s => s.Status)
            .HasConversion<string>();

        // =====================================================
        // SALE ITEMS CONFIGURATION
        // =====================================================
        builder.Entity<SaleItem>()
            .HasOne(si => si.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SaleItem>()
            .HasOne(si => si.Product)
            .WithMany(p => p.SaleItems)
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SaleItem>()
            .Property(si => si.UnitPrice)
            .HasPrecision(12, 2);

        builder.Entity<SaleItem>()
            .Property(si => si.Amount)
            .HasPrecision(12, 2);

        // =====================================================
        // PRODUCT CONFIGURATION
        // =====================================================
        builder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(12, 2);

        builder.Entity<Product>()
            .Property(p => p.Stock)
            .HasDefaultValue(0);
    }
}
