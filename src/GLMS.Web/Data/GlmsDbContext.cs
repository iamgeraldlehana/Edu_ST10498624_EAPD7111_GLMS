using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data;

public class GlmsDbContext(DbContextOptions<GlmsDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ContractNotification> ContractNotifications => Set<ContractNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractNotification>()
            .HasOne(notification => notification.Contract)
            .WithMany()
            .HasForeignKey(notification => notification.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Client>()
            .HasMany(client => client.Contracts)
            .WithOne(contract => contract.Client)
            .HasForeignKey(contract => contract.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contract>()
            .HasMany(contract => contract.ServiceRequests)
            .WithOne(request => request.Contract)
            .HasForeignKey(request => request.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.CostUsd)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.CostZar)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ServiceRequest>()
            .Property(request => request.CurrencyRate)
            .HasColumnType("decimal(18,4)");
    }
}
