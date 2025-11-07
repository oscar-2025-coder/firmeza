using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Firmeza.Admin.Models;
namespace Firmeza.Admin.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
        // SALE ITEM
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.Property(i => i.Quantity).IsRequired();

            // Dinero con precisión
            entity.Property(i => i.UnitPrice).HasPrecision(12, 2);
            entity.Property(i => i.Amount).HasPrecision(12, 2);

            // FK: SaleItem -> Sale (borrar venta = borrar sus ítems)
            entity.HasOne(i => i.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK: SaleItem -> Product (proteger historial)
            entity.HasOne(i => i.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });


    }
}