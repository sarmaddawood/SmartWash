using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartWash.Models;

namespace SmartWash.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<LaundryOrder> LaundryOrders { get; set; }
    public DbSet<LaundryItem> LaundryItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CustomerUpload> CustomerUploads { get; set; }
    public DbSet<ServicePrice> ServicePrices { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Additional configurations if needed
        builder.Entity<LaundryOrder>()
            .HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId);
    }
}
