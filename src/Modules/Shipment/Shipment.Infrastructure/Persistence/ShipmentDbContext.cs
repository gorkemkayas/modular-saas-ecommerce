using Microsoft.EntityFrameworkCore;
using Shipment.Application.Abstractions;
using Shipment.Domain.Entities;

namespace Shipment.Infrastructure.Persistence;

public sealed class ShipmentDbContext : DbContext, IUnitOfWork
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Shipment.Domain.Entities.Shipment> Shipments => Set<Shipment.Domain.Entities.Shipment>();
    public DbSet<ShippingCarrier> ShippingCarriers => Set<ShippingCarrier>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<ShipmentPackage> ShipmentPackages => Set<ShipmentPackage>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipmentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
