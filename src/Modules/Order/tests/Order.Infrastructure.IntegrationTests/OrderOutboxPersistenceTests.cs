using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.Models;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.Infrastructure.IntegrationTests;

[TestClass]
public sealed class OrderOutboxPersistenceTests
{
    [TestMethod]
    public async Task SaveChangesAsync_WhenOrderIsCancelled_PersistsOrderAndOutboxMessage()
    {
        await using var serviceProvider =
            OrderIntegrationTestHost.CreateServiceProvider();

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        await dbContext.Database.MigrateAsync();

        var order = CreateOrder();

        order.Cancel("Customer changed mind");

        var domainEvent = order.DomainEvents
            .OfType<OrderCancelledDomainEvent>()
            .Single();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var persistedOrder = await dbContext.Orders
            .AsNoTracking()
            .SingleAsync(x => x.Id == order.Id);

        var outboxMessage = await dbContext.OutboxMessages
            .AsNoTracking()
            .SingleAsync(x => x.Id == domainEvent.Id);

        Assert.AreEqual(OrderStatus.Cancelled, persistedOrder.Status);
        Assert.AreEqual(
            "Customer changed mind",
            persistedOrder.CancellationReason);

        Assert.AreEqual(domainEvent.Id, outboxMessage.Id);
        Assert.AreEqual(order.StoreId, outboxMessage.StoreId);
        Assert.AreEqual(
            OrderCancelledIntegrationEvent.EventType,
            outboxMessage.EventType);
        Assert.AreEqual(
            OrderCancelledIntegrationEvent.EventType,
            outboxMessage.RoutingKey);
        Assert.IsNull(outboxMessage.PublishedAtUtc);

        Assert.HasCount(0, order.DomainEvents);
    }

    private static OrderEntity CreateOrder()
    {
        return OrderEntity.Place(
            Guid.NewGuid(),
            OrderNumber.Create(
                $"ORD-{Guid.NewGuid():N}"),
            CustomerSnapshot.Create(
                Guid.NewGuid(),
                "customer@example.com",
                "Jane Doe",
                "+90 555 000 00 00"),
            CreateAddress("Billing"),
            CreateAddress("Shipping"),
            "TRY",
            new[]
            {
                new OrderItemDraft(
                    Guid.NewGuid(),
                    null,
                    "Phone",
                    null,
                    "SKU-1",
                    1,
                    OrderPriceSnapshot.Create(
                        100m,
                        "TRY",
                        null,
                        Guid.NewGuid(),
                        Guid.NewGuid()))
            });
    }

    private static OrderAddressSnapshot CreateAddress(string title)
    {
        return OrderAddressSnapshot.Create(
            title,
            "Jane Doe",
            "+90 555 000 00 00",
            "Turkey",
            "Istanbul",
            "Kadikoy",
            "Street 1",
            null,
            "34000");
    }
}