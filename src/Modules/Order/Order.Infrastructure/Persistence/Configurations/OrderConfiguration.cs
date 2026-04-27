using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StoreId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.PaymentStatus).IsRequired();
        builder.Property(x => x.FulfillmentStatus).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.ReservationReference).HasMaxLength(200);
        builder.Property(x => x.PaymentReference).HasMaxLength(200);
        builder.Property(x => x.ShipmentReference).HasMaxLength(200);
        builder.Property(x => x.PlacedAtUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.OwnsOne(x => x.OrderNumber, owned =>
        {
            owned.Property(x => x.Value)
                .HasColumnName("OrderNumber")
                .HasMaxLength(40)
                .IsRequired();
        });

        builder.OwnsOne(x => x.CustomerSnapshot, owned =>
        {
            owned.Property(x => x.CustomerId).HasColumnName("SnapshotCustomerId").IsRequired();
            owned.Property(x => x.Email).HasColumnName("CustomerEmail").HasMaxLength(320).IsRequired();
            owned.Property(x => x.FullName).HasColumnName("CustomerFullName").HasMaxLength(200).IsRequired();
            owned.Property(x => x.PhoneNumber).HasColumnName("CustomerPhoneNumber").HasMaxLength(50);
        });

        builder.OwnsOne(x => x.BillingAddressSnapshot, owned =>
        {
            ConfigureAddress(owned, "Billing");
        });

        builder.OwnsOne(x => x.ShippingAddressSnapshot, owned =>
        {
            ConfigureAddress(owned, "Shipping");
        });

        builder.OwnsOne(x => x.Totals, owned =>
        {
            owned.Property(x => x.SubtotalAmount).HasColumnName("SubtotalAmount").HasPrecision(18, 2).IsRequired();
            owned.Property(x => x.DiscountAmount).HasColumnName("DiscountAmount").HasPrecision(18, 2).IsRequired();
            owned.Property(x => x.ShippingAmount).HasColumnName("ShippingAmount").HasPrecision(18, 2).IsRequired();
            owned.Property(x => x.TaxAmount).HasColumnName("TaxAmount").HasPrecision(18, 2).IsRequired();
            owned.Property(x => x.GrandTotalAmount).HasColumnName("GrandTotalAmount").HasPrecision(18, 2).IsRequired();
        });

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.StoreId, x.CustomerId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.StoreId, x.Status, x.CreatedAtUtc });
    }

    private static void ConfigureAddress(OwnedNavigationBuilder<OrderEntity, Order.Domain.ValueObjects.OrderAddressSnapshot> owned, string prefix)
    {
        owned.Property(x => x.Title).HasColumnName($"{prefix}Title").HasMaxLength(150).IsRequired();
        owned.Property(x => x.ContactName).HasColumnName($"{prefix}ContactName").HasMaxLength(200).IsRequired();
        owned.Property(x => x.PhoneNumber).HasColumnName($"{prefix}PhoneNumber").HasMaxLength(50).IsRequired();
        owned.Property(x => x.Country).HasColumnName($"{prefix}Country").HasMaxLength(100).IsRequired();
        owned.Property(x => x.City).HasColumnName($"{prefix}City").HasMaxLength(100).IsRequired();
        owned.Property(x => x.District).HasColumnName($"{prefix}District").HasMaxLength(100).IsRequired();
        owned.Property(x => x.Line1).HasColumnName($"{prefix}Line1").HasMaxLength(250).IsRequired();
        owned.Property(x => x.Line2).HasColumnName($"{prefix}Line2").HasMaxLength(250);
        owned.Property(x => x.PostalCode).HasColumnName($"{prefix}PostalCode").HasMaxLength(30);
    }
}
