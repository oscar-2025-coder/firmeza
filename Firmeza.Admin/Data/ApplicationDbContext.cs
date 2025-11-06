using Microsoft.EntityFrameworkCore;
using Firmeza.admi.Models;
namespace Firmeza.Admin.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Sale> Sales { get; set; } = default!;
    public DbSet<SaleItem> SaleItems { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // PRODUCT
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.UnitPrice)
                .HasPrecision(12, 2);
        });
        
        // CUSTOMER
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(c => c.DocumentNumber)
                .IsUnique();
        });
        // SALE
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.Property(s => s.Date)
                .IsRequired();

            entity.Property(s => s.Subtotal).HasPrecision(12, 2);
            entity.Property(s => s.Tax).HasPrecision(12, 2);
            entity.Property(s => s.Total).HasPrecision(12, 2);

            // Enum como string (legible) and max length
            entity.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Relationship with Customer
            entity.HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });


    }
}