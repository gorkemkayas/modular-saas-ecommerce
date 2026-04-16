using Customer.Application.Abstractions;
using Customer.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Customer.Application.UnitTests.Customers.Commands.SyncCustomerFromIdentity;

[TestClass]
public sealed class SyncCustomerFromIdentityCommandHandlerTests
{
    [TestMethod]
    public async Task Handle_WhenCustomerDoesNotExist_CreatesCustomerAndReturnsId()
    {
        var tenantId = Guid.NewGuid();
        var externalUserId = Guid.NewGuid();
        var command = new Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommand(
            tenantId,
            externalUserId,
            "customer@example.com",
            "Ada",
            "Lovelace");

        Domain.Entities.Customer? addedCustomer = null;

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository
            .Setup(x => x.GetByExternalUserIdAsync(tenantId, externalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Customer?)null);
        customerRepository
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Customer, CancellationToken>((customer, _) => addedCustomer = customer)
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommandHandler(
            customerRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommandHandler>>());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.IsNotNull(addedCustomer);
        Assert.AreEqual(result, addedCustomer.Id);
        Assert.AreEqual(tenantId, addedCustomer.TenantId);
        Assert.AreEqual(externalUserId, addedCustomer.ExternalUserId);
        Assert.AreEqual("customer@example.com", addedCustomer.Email.Value);
        Assert.AreEqual("Ada", addedCustomer.Name.FirstName);
        Assert.AreEqual("Lovelace", addedCustomer.Name.LastName);
    }

    [TestMethod]
    public async Task Handle_WhenCustomerExists_SynchronizesIdentityWithoutAddingNewCustomer()
    {
        var tenantId = Guid.NewGuid();
        var externalUserId = Guid.NewGuid();
        var existingCustomer = Domain.Entities.Customer.Create(
            tenantId,
            externalUserId,
            Domain.ValueObjects.EmailAddress.Create("old@example.com"),
            Domain.ValueObjects.PersonName.Create("Old", "Name"));

        var command = new Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommand(
            tenantId,
            externalUserId,
            "new@example.com",
            "New",
            "Name");

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository
            .Setup(x => x.GetByExternalUserIdAsync(tenantId, externalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommandHandler(
            customerRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<Application.Customers.Commands.SyncCustomerFromIdentity.SyncCustomerFromIdentityCommandHandler>>());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.AreEqual(existingCustomer.Id, result);
        Assert.AreEqual("new@example.com", existingCustomer.Email.Value);
        Assert.AreEqual("New", existingCustomer.Name.FirstName);
        customerRepository.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
