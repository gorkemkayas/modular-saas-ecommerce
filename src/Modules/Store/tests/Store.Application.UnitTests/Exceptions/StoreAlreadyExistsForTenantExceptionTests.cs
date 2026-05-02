using Store.Application.Exceptions;

namespace Store.Application.UnitTests.Exceptions;

[TestClass]
public sealed class StoreAlreadyExistsForTenantExceptionTests
{
    [TestMethod]
    public void Constructor_SetsTenantIdAndMessage()
    {
        var tenantId = Guid.NewGuid();

        var exception = new StoreAlreadyExistsForTenantException(tenantId);

        Assert.AreEqual(tenantId, exception.TenantId);
        Assert.AreEqual($"A store already exists for tenant '{tenantId}'.", exception.Message);
    }
}
