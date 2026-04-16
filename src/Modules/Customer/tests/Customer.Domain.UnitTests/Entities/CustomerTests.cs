using Customer.Domain.Enums;
using Customer.Domain.ValueObjects;

namespace Customer.Domain.UnitTests.Entities;

[TestClass]
public sealed class CustomerTests
{
    [TestMethod]
    public void AddAddress_WhenSecondAddressBecomesDefaultShipping_RemovesDefaultFromFirst()
    {
        var customer = Domain.Entities.Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EmailAddress.Create("customer@example.com"),
            PersonName.Create("Ada", "Lovelace"));

        var firstAddressId = customer.AddAddress(
            AddressType.Home,
            "Home",
            "Ada Lovelace",
            PhoneNumber.Create("+905551112233"),
            "Turkey",
            "Istanbul",
            "Kadikoy",
            "Street 1",
            null,
            "34710",
            isDefaultShipping: true,
            isDefaultBilling: false);

        var secondAddressId = customer.AddAddress(
            AddressType.Work,
            "Office",
            "Ada Lovelace",
            PhoneNumber.Create("+905551112244"),
            "Turkey",
            "Istanbul",
            "Besiktas",
            "Street 2",
            null,
            "34353",
            isDefaultShipping: true,
            isDefaultBilling: false);

        var firstAddress = customer.Addresses.Single(x => x.Id == firstAddressId);
        var secondAddress = customer.Addresses.Single(x => x.Id == secondAddressId);

        Assert.IsFalse(firstAddress.IsDefaultShipping);
        Assert.IsTrue(secondAddress.IsDefaultShipping);
    }

    [TestMethod]
    public void RemoveAddress_WhenRemovingDefaultBilling_AssignsAnotherBillingAddress()
    {
        var customer = Domain.Entities.Customer.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EmailAddress.Create("customer@example.com"),
            PersonName.Create("Ada", "Lovelace"));

        var firstAddressId = customer.AddAddress(
            AddressType.Home,
            "Home",
            "Ada Lovelace",
            PhoneNumber.Create("+905551112233"),
            "Turkey",
            "Istanbul",
            "Kadikoy",
            "Street 1",
            null,
            "34710",
            isDefaultShipping: true,
            isDefaultBilling: true);

        customer.AddAddress(
            AddressType.Work,
            "Office",
            "Ada Lovelace",
            PhoneNumber.Create("+905551112244"),
            "Turkey",
            "Istanbul",
            "Besiktas",
            "Street 2",
            null,
            "34353",
            isDefaultShipping: false,
            isDefaultBilling: false);

        customer.RemoveAddress(firstAddressId);

        Assert.AreEqual(1, customer.Addresses.Count);
        Assert.IsTrue(customer.Addresses.Single().IsDefaultBilling);
    }
}
