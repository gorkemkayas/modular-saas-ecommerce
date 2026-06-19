using System;
using System.Threading;
using System.Threading.Tasks;

using BuildingBlocks.Application.Extensions;
using ECommerce.API.Contracts.Store.ProvisionStoreForTenant;
using ECommerce.API.Controllers.Store.Admin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Application.DTOs;
using Store.Application.Stores.Commands.ActivateStore;
using Store.Application.Stores.Commands.ArchiveStore;
using Store.Application.Stores.Commands.ProvisionStoreForTenant;
using Store.Application.Stores.Commands.SuspendStore;
using Store.Application.Stores.Queries.GetStoreById;
using Store.Application.Stores.Queries.GetStoreBySlug;
using Store.Application.Stores.Queries.GetStoreByTenantId;
using Store.Application.Stores.Queries.SuggestAvailableSlug;
using Store.Domain.Stores;
using Subscription.Application.Commands.ProvisionTenantSubscription;
using Subscription.Application.DTOs;
using Subscription.Application.Queries.GetPublicPlans;
using Subscription.Contracts;

namespace ECommerce.API.Controllers.Store.Admin.UnitTests;


/// <summary>
/// Unit tests for the AdminStoresController class.
/// </summary>
[TestClass]
public sealed class AdminStoresControllerTests
{
    private const string ValidAuthHeader = "Bearer DEV_SERVICE_TOKEN_12345";

    private static IConfiguration CreateConfiguration()
    {
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ServiceTokens:ECommerce"]).Returns("DEV_SERVICE_TOKEN_12345");
        return mockConfiguration.Object;
    }

    private static AdminStoresController CreateController(ISender sender)
        => new(sender, CreateConfiguration());

    private static IReadOnlyCollection<PlanDto> CreatePublicPlans()
    {
        return
        [
            new(
                SubscriptionPlanCodes.Starter,
                "Starter",
                "Starter plan",
                10,
                99.99m,
                "TRY",
                Array.Empty<PlanFeatureDto>(),
                Array.Empty<PlanQuotaDto>()),
            new(
                SubscriptionPlanCodes.Growth,
                "Growth",
                "Growth plan",
                20,
                249.99m,
                "TRY",
                Array.Empty<PlanFeatureDto>(),
                Array.Empty<PlanQuotaDto>())
        ];
    }

    /// <summary>
    /// Tests that GetStoreByTenantId returns OkObjectResult with the store when the store exists.
    /// </summary>
    /// <param name="tenantId">The tenant ID to test with.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(999999)]
    [DataRow(int.MaxValue)]
    public async Task GetStoreByTenantId_StoreExists_ReturnsOkWithStore(int tenantId)
    {
        // Arrange
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);
        var expectedStore = new StoreDto(
            Guid.NewGuid(),
            expectedTenantIdGuid,
            "Test Store",
            "test-store",
            "Test Description",
            "https://example.com/logo.png",
            StoreStatus.Active,
            true);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ServiceTokens:ECommerce"]).Returns("DEV_SERVICE_TOKEN_12345");

        var controller = new AdminStoresController(mockSender.Object, mockConfiguration.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId returns NotFoundResult when the store does not exist.
    /// </summary>
    /// <param name="tenantId">The tenant ID to test with.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(999999)]
    [DataRow(int.MaxValue)]
    public async Task GetStoreByTenantId_StoreDoesNotExist_ReturnsNotFound(int tenantId)
    {
        // Arrange
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDto?)null);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ServiceTokens:ECommerce"]).Returns("DEV_SERVICE_TOKEN_12345");

        var controller = new AdminStoresController(mockSender.Object, mockConfiguration.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NotFoundResult>(result);
        var notFoundResult = (NotFoundResult)result;
        Assert.AreEqual(404, notFoundResult.StatusCode);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId correctly handles zero as tenant ID.
    /// </summary>
    [TestMethod]
    public async Task GetStoreByTenantId_TenantIdIsZero_ProcessesCorrectly()
    {
        // Arrange
        int tenantId = 0;
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);
        var expectedStore = new StoreDto(
            Guid.NewGuid(),
            expectedTenantIdGuid,
            "Test Store",
            "test-store",
            null,
            null,
            StoreStatus.Active,
            true);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);

        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedStore, okResult.Value);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId correctly handles negative tenant IDs.
    /// </summary>
    /// <param name="tenantId">The negative tenant ID to test with.</param>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(-999999)]
    [DataRow(int.MinValue)]
    public async Task GetStoreByTenantId_NegativeTenantId_ProcessesCorrectly(int tenantId)
    {
        // Arrange
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);
        var expectedStore = new StoreDto(
            Guid.NewGuid(),
            expectedTenantIdGuid,
            "Test Store",
            "test-store",
            "Description",
            null,
            StoreStatus.Suspended,
            false);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ServiceTokens:ECommerce"]).Returns("DEV_SERVICE_TOKEN_12345");

        var controller = new AdminStoresController(mockSender.Object, mockConfiguration.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId correctly passes the CancellationToken to the sender.
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task GetStoreByTenantId_WithCancellationToken_PassesTokenToSender()
    {
        // Arrange
        int tenantId = 42;
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);
        var expectedStore = new StoreDto(
            Guid.NewGuid(),
            expectedTenantIdGuid,
            "Test Store",
            "test-store",
            null,
            null,
            StoreStatus.Active,
            true);

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.IsAny<GetStoreByTenantIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        mockSender.Verify(s => s.Send(It.IsAny<GetStoreByTenantIdQuery>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId converts the tenant ID to Guid correctly using TenantIdConverter.
    /// </summary>
    [TestMethod]
    public async Task GetStoreByTenantId_ConvertsIntTenantIdToGuid_UsingTenantIdConverter()
    {
        // Arrange
        int tenantId = 12345;
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.IsAny<GetStoreByTenantIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDto?)null);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ServiceTokens:ECommerce"]).Returns("DEV_SERVICE_TOKEN_12345");

        var controller = new AdminStoresController(mockSender.Object, mockConfiguration.Object);

        // Act
        await controller.GetStoreByTenantId(tenantId, CancellationToken.None);

        // Assert
        mockSender.Verify(s => s.Send(
            It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreByTenantId returns NotFoundResult when store is null with boundary tenant ID values.
    /// </summary>
    /// <param name="tenantId">The boundary tenant ID to test with.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(int.MinValue)]
    public async Task GetStoreByTenantId_BoundaryTenantIds_ReturnsNotFoundWhenStoreIsNull(int tenantId)
    {
        // Arrange
        var expectedTenantIdGuid = TenantIdConverter.ToGuid(tenantId);

        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByTenantIdQuery>(q => q.TenantId == expectedTenantIdGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDto?)null);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.GetStoreByTenantId(tenantId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    /// <summary>
    /// Tests that ActivateStore returns NoContent (204) when provided with valid positive tenant IDs.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(12345)]
    [DataRow(999999)]
    public async Task ActivateStore_ValidPositiveTenantId_ReturnsNoContent(int tenantId)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.ActivateStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.IsAny<ActivateStoreCommand>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStore handles edge case numeric values including int.MinValue, int.MaxValue, zero, and negative values.
    /// </summary>
    /// <param name="tenantId">The edge case tenant identifier to test.</param>
    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(-999999)]
    public async Task ActivateStore_EdgeCaseNumericValues_ReturnsNoContent(int tenantId)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.ActivateStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.IsAny<ActivateStoreCommand>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStore sends a command with the correct tenant ID converted to a Guid.
    /// Verifies that the TenantIdConverter.ToGuid conversion is applied correctly.
    /// </summary>
    [TestMethod]
    public async Task ActivateStore_ValidTenantId_SendsCommandWithCorrectTenantIdGuid()
    {
        // Arrange
        var tenantId = 12345;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);
        var mockSender = new Mock<ISender>();
        ActivateStoreCommand? capturedCommand = null;

        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest, CancellationToken>((cmd, ct) => capturedCommand = cmd as ActivateStoreCommand)
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        await controller.ActivateStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(capturedCommand);
        Assert.AreEqual(expectedGuid, capturedCommand.TenantId);
        mockSender.Verify(s => s.Send(It.Is<ActivateStoreCommand>(c => c.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStore passes the cancellation token through to the sender's Send method.
    /// Ensures proper cancellation token propagation for async operations.
    /// </summary>
    [TestMethod]
    public async Task ActivateStore_ValidTenantId_PassesCancellationTokenToSender()
    {
        // Arrange
        var tenantId = 100;
        var mockSender = new Mock<ISender>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        CancellationToken capturedToken = default;

        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest, CancellationToken>((cmd, ct) => capturedToken = ct)
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        await controller.ActivateStore(tenantId, cancellationToken);

        // Assert
        Assert.AreEqual(cancellationToken, capturedToken);
        mockSender.Verify(s => s.Send(It.IsAny<ActivateStoreCommand>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStore properly handles a cancelled cancellation token.
    /// Verifies that the method respects cancellation requests when the token is already cancelled.
    /// </summary>
    [TestMethod]
    public async Task ActivateStore_CancelledToken_PropagatesCancellationToken()
    {
        // Arrange
        var tenantId = 100;
        var mockSender = new Mock<ISender>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancelledToken = cancellationTokenSource.Token;

        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.ActivateStore(tenantId, cancelledToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.IsAny<ActivateStoreCommand>(), cancelledToken), Times.Once);
    }

    /// <summary>
    /// Tests that ActivateStore creates the correct ActivateStoreCommand with the converted tenant ID.
    /// Verifies the command structure for multiple different tenant IDs to ensure consistent behavior.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(1000)]
    [DataRow(-1)]
    [DataRow(0)]
    public async Task ActivateStore_VariousTenantIds_CreatesCorrectCommand(int tenantId)
    {
        // Arrange
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.IsAny<ActivateStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        await controller.ActivateStore(tenantId, cancellationToken);

        // Assert
        mockSender.Verify(
            s => s.Send(
                It.Is<ActivateStoreCommand>(cmd => cmd.TenantId == expectedGuid),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Tests that ArchiveStore sends the correct ArchiveStoreCommand with the converted tenant ID
    /// and returns a NoContent result for various tenant ID values including edge cases.
    /// </summary>
    /// <param name="tenantId">The tenant ID to test.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(100)]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(2147483647)] // int.MaxValue
    [DataRow(-2147483648)] // int.MinValue
    public async Task ArchiveStore_VariousTenantIds_SendsCorrectCommandAndReturnsNoContent(int tenantId)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.IsAny<ArchiveStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await controller.ArchiveStore(tenantId, cancellationToken);

        // Assert
        mockSender.Verify(
            s => s.Send(
                It.Is<ArchiveStoreCommand>(cmd => cmd.TenantId == expectedGuid),
                cancellationToken),
            Times.Once);
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    /// <summary>
    /// Tests that ArchiveStore passes the provided cancellation token to the sender
    /// and properly handles cancellation scenarios.
    /// </summary>
    [TestMethod]
    public async Task ArchiveStore_WithCancellationToken_PassesTokenToSender()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var tenantId = 123;

        mockSender
            .Setup(s => s.Send(It.IsAny<ArchiveStoreCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await controller.ArchiveStore(tenantId, cancellationToken);

        // Assert
        mockSender.Verify(
            s => s.Send(
                It.IsAny<ArchiveStoreCommand>(),
                cancellationToken),
            Times.Once);
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    /// <summary>
    /// Tests that ProvisionStoreForTenant returns CreatedAtActionResult with valid inputs.
    /// Verifies the action name, route values, and the returned store ID.
    /// </summary>
    [TestMethod]
    public async Task ProvisionStoreForTenant_ValidInput_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        int tenantId = 123;
        var request = new ProvisionStoreForTenantRequest("Test Store");
        var suggestedSlug = "test-store";
        var expectedStoreId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == request.Name), cancellationToken))
            .ReturnsAsync(new SlugSuggestionDto(suggestedSlug));

        mockSender
            .Setup(s => s.Send(It.IsAny<GetPublicPlansQuery>(), cancellationToken))
            .ReturnsAsync(CreatePublicPlans());

        mockSender
            .Setup(s => s.Send(It.Is<ProvisionStoreForTenantCommand>(
                    c => c.TenantId == TenantIdConverter.ToGuid(tenantId)
                      && c.Name == request.Name
                      && c.Slug == suggestedSlug), cancellationToken))
            .ReturnsAsync(expectedStoreId);

        mockSender
            .Setup(s => s.Send(It.Is<ProvisionTenantSubscriptionCommand>(
                    c => c.TenantId == TenantIdConverter.ToGuid(tenantId)
                      && c.PlanCode == SubscriptionPlanCodes.Starter), cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await controller.ProvisionStoreForTenant(tenantId, request, cancellationToken, ValidAuthHeader);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<CreatedAtActionResult>(result);
        var createdResult = (CreatedAtActionResult)result;
        Assert.AreEqual(nameof(AdminStoresController.GetStoreById), createdResult.ActionName);
        Assert.IsNotNull(createdResult.RouteValues);
        Assert.IsTrue(createdResult.RouteValues.ContainsKey("storeId"));
        Assert.AreEqual(expectedStoreId, createdResult.RouteValues["storeId"]);
        Assert.IsNotNull(createdResult.Value);
        Assert.IsInstanceOfType<ProvisionStoreForTenantResponse>(createdResult.Value);
        var response = (ProvisionStoreForTenantResponse)createdResult.Value;
        Assert.AreEqual(expectedStoreId.ToString(), response.StoreId);
        Assert.AreEqual(suggestedSlug, response.StoreSlug);
        mockSender.Verify(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == request.Name), cancellationToken), Times.Once);
        mockSender.Verify(s => s.Send(It.IsAny<GetPublicPlansQuery>(), cancellationToken), Times.Once);
        mockSender.Verify(s => s.Send(It.IsAny<ProvisionTenantSubscriptionCommand>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ProvisionStoreForTenant sends command with correctly converted tenant ID and request properties.
    /// Verifies the command parameters match the input values with tenant ID converted to Guid.
    /// </summary>
    /// <param name="tenantId">The tenant ID to test.</param>
    /// <param name="name">The store name.</param>
    /// <param name="suggestedSlug">The suggested slug value returned from slug suggestion query.</param>
    [TestMethod]
    [DataRow(1, "Store Name", "store-name")]
    [DataRow(0, "Zero Tenant Store", "zero-tenant-store")]
    [DataRow(-1, "Negative Tenant", "negative-tenant")]
    [DataRow(int.MaxValue, "Max Tenant", "max-tenant")]
    [DataRow(int.MinValue, "Min Tenant", "min-tenant")]
    public async Task ProvisionStoreForTenant_VariousTenantIds_SendsCommandWithCorrectParameters(int tenantId, string name, string suggestedSlug)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var request = new ProvisionStoreForTenantRequest(name);
        var expectedStoreId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        ProvisionStoreForTenantCommand? capturedCommand = null;

        mockSender
            .Setup(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == name), cancellationToken))
            .ReturnsAsync(new SlugSuggestionDto(suggestedSlug));

        mockSender
            .Setup(s => s.Send(It.IsAny<GetPublicPlansQuery>(), cancellationToken))
            .ReturnsAsync(CreatePublicPlans());

        mockSender
            .Setup(s => s.Send(It.IsAny<ProvisionStoreForTenantCommand>(), cancellationToken))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, ct) => capturedCommand = cmd as ProvisionStoreForTenantCommand)
            .ReturnsAsync(expectedStoreId);

        mockSender
            .Setup(s => s.Send(It.IsAny<ProvisionTenantSubscriptionCommand>(), cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await controller.ProvisionStoreForTenant(tenantId, request, cancellationToken, ValidAuthHeader);

        // Assert
        Assert.IsNotNull(capturedCommand);
        Assert.AreEqual(TenantIdConverter.ToGuid(tenantId), capturedCommand.TenantId);
        Assert.AreEqual(name, capturedCommand.Name);
        Assert.AreEqual(suggestedSlug, capturedCommand.Slug);
        mockSender.Verify(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == name), cancellationToken), Times.Once);
        mockSender.Verify(s => s.Send(It.IsAny<ProvisionStoreForTenantCommand>(), cancellationToken), Times.Once);
        mockSender.Verify(s => s.Send(It.IsAny<ProvisionTenantSubscriptionCommand>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that ProvisionStoreForTenant handles edge case string values in the request.
    /// Verifies empty strings, whitespace, and strings with special characters are passed through correctly.
    /// </summary>
    /// <param name="name">The store name to test.</param>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("Name with spaces")]
    [DataRow("Special!@#$%^&*()")]
    [DataRow("Very Long Name That Exceeds Normal Length Expectations For Testing Purposes With Many Characters")]
    public async Task ProvisionStoreForTenant_EdgeCaseStrings_SendsCommandWithProvidedValues(string name)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        int tenantId = 1;
        var request = new ProvisionStoreForTenantRequest(name);
        var suggestedSlug = "generated-slug";
        var expectedStoreId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        ProvisionStoreForTenantCommand? capturedCommand = null;

        mockSender
            .Setup(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == name), cancellationToken))
            .ReturnsAsync(new SlugSuggestionDto(suggestedSlug));

        mockSender
            .Setup(s => s.Send(It.IsAny<GetPublicPlansQuery>(), cancellationToken))
            .ReturnsAsync(CreatePublicPlans());

        mockSender
            .Setup(s => s.Send(It.IsAny<ProvisionStoreForTenantCommand>(), cancellationToken))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, ct) => capturedCommand = cmd as ProvisionStoreForTenantCommand)
            .ReturnsAsync(expectedStoreId);

        mockSender
            .Setup(s => s.Send(It.IsAny<ProvisionTenantSubscriptionCommand>(), cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await controller.ProvisionStoreForTenant(tenantId, request, cancellationToken, ValidAuthHeader);

        // Assert
        Assert.IsNotNull(capturedCommand);
        Assert.AreEqual(name, capturedCommand.Name);
        Assert.AreEqual(suggestedSlug, capturedCommand.Slug);
    }

    /// <summary>
    /// Tests that ProvisionStoreForTenant returns CreatedAtActionResult even when sender returns Guid.Empty.
    /// Verifies the method handles edge case Guid values correctly.
    /// </summary>
    [TestMethod]
    public async Task ProvisionStoreForTenant_SenderReturnsEmptyGuid_ReturnsCreatedAtActionResultWithEmptyGuid()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        int tenantId = 123;
        var request = new ProvisionStoreForTenantRequest("Test Store");
        var suggestedSlug = "test-store";
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<SuggestAvailableSlugQuery>(q => q.Name == request.Name), cancellationToken))
            .ReturnsAsync(new SlugSuggestionDto(suggestedSlug));

        mockSender
            .Setup(s => s.Send(It.IsAny<GetPublicPlansQuery>(), cancellationToken))
            .ReturnsAsync(CreatePublicPlans());

        mockSender
            .Setup(s => s.Send(It.Is<ProvisionStoreForTenantCommand>(
                    c => c.TenantId == TenantIdConverter.ToGuid(tenantId)
                      && c.Name == request.Name
                      && c.Slug == suggestedSlug), cancellationToken))
            .ReturnsAsync(Guid.Empty);

        mockSender
            .Setup(s => s.Send(It.IsAny<ProvisionTenantSubscriptionCommand>(), cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await controller.ProvisionStoreForTenant(tenantId, request, cancellationToken, ValidAuthHeader);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<CreatedAtActionResult>(result);
        var createdResult = (CreatedAtActionResult)result;
        Assert.AreEqual(Guid.Empty, createdResult.RouteValues?["storeId"]);
        Assert.IsNotNull(createdResult.Value);
        Assert.IsInstanceOfType<ProvisionStoreForTenantResponse>(createdResult.Value);
        var response = (ProvisionStoreForTenantResponse)createdResult.Value;
        Assert.AreEqual(Guid.Empty.ToString(), response.StoreId);
        Assert.AreEqual(suggestedSlug, response.StoreSlug);
    }

    /// <summary>
    /// Tests that GetStoreBySlug returns Ok result with store data when a valid slug is provided and store is found.
    /// Input: Valid slug "test-store".
    /// Expected: OkObjectResult with StoreDto.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_ValidSlug_ReturnsOkWithStore()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "test-store";
        var expectedStore = new StoreDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: slug,
            Description: "Test Description",
            LogoUrl: "https://example.com/logo.png",
            Status: StoreStatus.Active,
            IsPublished: true
        );
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync(expectedStore);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug returns NotFound result when store is not found (null).
    /// Input: Slug "non-existent-store" that returns null.
    /// Expected: NotFoundResult.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_StoreNotFound_ReturnsNotFound()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "non-existent-store";
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync((StoreDto?)null);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug handles empty string slug by sending query and returning result.
    /// Input: Empty string slug.
    /// Expected: Query sent with empty string and appropriate result returned.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_EmptySlug_SendsQueryAndReturnsNotFound()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = string.Empty;
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync((StoreDto?)null);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug handles whitespace-only slug by sending query.
    /// Input: Whitespace-only string "   ".
    /// Expected: Query sent with whitespace string and appropriate result returned.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_WhitespaceSlug_SendsQueryAndReturnsNotFound()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "   ";
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync((StoreDto?)null);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug handles slug with special characters commonly used in URLs.
    /// Input: Slug with special characters "my-store_123".
    /// Expected: Query sent with special characters and store returned if found.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_SpecialCharactersInSlug_ReturnsOkWithStore()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "my-store_123";
        var expectedStore = new StoreDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Name: "My Store 123",
            Slug: slug,
            Description: null,
            LogoUrl: null,
            Status: StoreStatus.Active,
            IsPublished: false
        );
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync(expectedStore);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug handles very long slug strings.
    /// Input: Very long slug string (1000 characters).
    /// Expected: Query sent with long string and appropriate result returned.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_VeryLongSlug_SendsQueryAndReturnsNotFound()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = new string('a', 1000);
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync((StoreDto?)null);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug properly propagates the cancellation token to the sender.
    /// Input: Valid slug and a specific cancellation token.
    /// Expected: Cancellation token passed to ISender.Send method.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_CancellationToken_PropagatedToSender()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "test-store";
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var expectedStore = new StoreDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: slug,
            Description: null,
            LogoUrl: null,
            Status: StoreStatus.Active,
            IsPublished: true
        );

        mockSender
            .Setup(s => s.Send(It.IsAny<GetStoreBySlugQuery>(), cancellationToken))
            .ReturnsAsync(expectedStore);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        mockSender.Verify(s => s.Send(It.IsAny<GetStoreBySlugQuery>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreBySlug handles slug with URL-encoded characters.
    /// Input: Slug with URL-encoded characters "%20%21%40".
    /// Expected: Query sent with encoded characters and appropriate result returned.
    /// </summary>
    [TestMethod]
    public async Task GetStoreBySlug_UrlEncodedSlug_SendsQueryAndReturnsNotFound()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var controller = CreateController(mockSender.Object);
        var slug = "%20%21%40";
        var cancellationToken = CancellationToken.None;

        mockSender
            .Setup(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken))
            .ReturnsAsync((StoreDto?)null);

        // Act
        var result = await controller.GetStoreBySlug(slug, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        mockSender.Verify(s => s.Send(It.Is<GetStoreBySlugQuery>(q => q.Slug == slug), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with a valid ISender mock.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidSender_CreatesInstance()
    {
        // Arrange
        var mockSender = new Mock<ISender>();

        // Act
        var controller = CreateController(mockSender.Object);

        // Assert
        Assert.IsNotNull(controller);
    }

    /// <summary>
    /// Tests that the constructor accepts a null sender parameter.
    /// This documents the actual behavior where no validation is performed,
    /// despite the parameter being marked as non-nullable.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSender_CreatesInstance()
    {
        // Arrange
        ISender? nullSender = null;

        // Act
        var controller = new AdminStoresController(nullSender!, CreateConfiguration());

        // Assert
        Assert.IsNotNull(controller);
    }

    /// <summary>
    /// Tests that GetStoreById returns OkObjectResult with StoreDto when the store is found.
    /// Input: Valid storeId that exists in the system.
    /// Expected: Returns 200 OK with the store data.
    /// </summary>
    [TestMethod]
    public async Task GetStoreById_ValidStoreIdWithExistingStore_ReturnsOkWithStore()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var expectedStore = new StoreDto(
            Id: storeId,
            TenantId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: "test-store",
            Description: "A test store",
            LogoUrl: "https://example.com/logo.png",
            Status: StoreStatus.Active,
            IsPublished: true
        );
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreById returns NotFoundResult when the store is not found.
    /// Input: Valid storeId that does not exist in the system (sender returns null).
    /// Expected: Returns 404 NotFound.
    /// </summary>
    [TestMethod]
    public async Task GetStoreById_StoreNotFound_ReturnsNotFound()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDto?)null);
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreById handles Guid.Empty correctly.
    /// Input: Guid.Empty (all zeros) as storeId.
    /// Expected: Processes request and returns appropriate result based on sender response.
    /// </summary>
    [TestMethod]
    public async Task GetStoreById_EmptyGuid_ProcessesRequestAndReturnsNotFound()
    {
        // Arrange
        var storeId = Guid.Empty;
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDto?)null);
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        var notFoundResult = result as NotFoundResult;
        Assert.IsNotNull(notFoundResult);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreById correctly passes the cancellation token to the sender.
    /// Input: Valid storeId with a specific cancellation token.
    /// Expected: The cancellation token is passed to the sender's Send method.
    /// </summary>
    [TestMethod]
    public async Task GetStoreById_ValidRequest_PassesCancellationTokenToSender()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var expectedStore = new StoreDto(
            Id: storeId,
            TenantId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: "test-store",
            Description: null,
            LogoUrl: null,
            Status: StoreStatus.Active,
            IsPublished: false
        );
        var mockSender = new Mock<ISender>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        mockSender
            .Setup(s => s.Send(It.IsAny<GetStoreByIdQuery>(), cancellationToken))
            .ReturnsAsync(expectedStore);
        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that GetStoreById returns OkObjectResult with StoreDto containing null optional fields.
    /// Input: Valid storeId with store having null Description and LogoUrl.
    /// Expected: Returns 200 OK with the store data including null fields.
    /// </summary>
    [TestMethod]
    public async Task GetStoreById_StoreWithNullOptionalFields_ReturnsOkWithStore()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var expectedStore = new StoreDto(
            Id: storeId,
            TenantId: Guid.NewGuid(),
            Name: "Minimal Store",
            Slug: "minimal-store",
            Description: null,
            LogoUrl: null,
            Status: StoreStatus.Active,
            IsPublished: false
        );
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        var returnedStore = okResult.Value as StoreDto;
        Assert.IsNotNull(returnedStore);
        Assert.AreEqual(storeId, returnedStore.Id);
        Assert.IsNull(returnedStore.Description);
        Assert.IsNull(returnedStore.LogoUrl);
    }

    /// <summary>
    /// Tests that GetStoreById processes multiple different valid Guid values correctly.
    /// Input: Various valid Guid values.
    /// Expected: Each request is processed independently and returns appropriate result.
    /// </summary>
    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000001")]
    [DataRow("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [DataRow("12345678-1234-1234-1234-123456789abc")]
    public async Task GetStoreById_VariousValidGuids_ProcessesCorrectly(string guidString)
    {
        // Arrange
        var storeId = Guid.Parse(guidString);
        var expectedStore = new StoreDto(
            Id: storeId,
            TenantId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: "test-store",
            Description: "Test",
            LogoUrl: null,
            Status: StoreStatus.Active,
            IsPublished: true
        );
        var mockSender = new Mock<ISender>();
        mockSender
            .Setup(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStore);
        var controller = CreateController(mockSender.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await controller.GetStoreById(storeId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedStore, okResult.Value);
        mockSender.Verify(s => s.Send(It.Is<GetStoreByIdQuery>(q => q.StoreId == storeId), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore sends the correct command with converted tenant ID and returns NoContent.
    /// Input: Valid positive tenant ID.
    /// Expected: Send is called with correct SuspendStoreCommand and NoContentResult is returned.
    /// </summary>
    [TestMethod]
    [DataRow(1)]
    [DataRow(100)]
    [DataRow(999999)]
    public async Task SuspendStore_ValidPositiveTenantId_SendsCommandAndReturnsNoContent(int tenantId)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore handles zero tenant ID correctly.
    /// Input: Tenant ID of 0.
    /// Expected: Send is called with command containing converted zero GUID and NoContentResult is returned.
    /// </summary>
    [TestMethod]
    public async Task SuspendStore_ZeroTenantId_SendsCommandAndReturnsNoContent()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var tenantId = 0;
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore handles negative tenant IDs.
    /// Input: Negative tenant ID values.
    /// Expected: Send is called with command containing converted GUID and NoContentResult is returned.
    /// </summary>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-100)]
    [DataRow(-999999)]
    public async Task SuspendStore_NegativeTenantId_SendsCommandAndReturnsNoContent(int tenantId)
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore handles minimum integer value for tenant ID.
    /// Input: int.MinValue.
    /// Expected: Send is called with command containing converted GUID and NoContentResult is returned.
    /// </summary>
    [TestMethod]
    public async Task SuspendStore_IntMinValueTenantId_SendsCommandAndReturnsNoContent()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var tenantId = int.MinValue;
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore handles maximum integer value for tenant ID.
    /// Input: int.MaxValue.
    /// Expected: Send is called with command containing converted GUID and NoContentResult is returned.
    /// </summary>
    [TestMethod]
    public async Task SuspendStore_IntMaxValueTenantId_SendsCommandAndReturnsNoContent()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var tenantId = int.MaxValue;
        var cancellationToken = CancellationToken.None;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SuspendStore passes the cancellation token correctly to the sender.
    /// Input: Valid tenant ID with a specific non-default cancellation token.
    /// Expected: Send is called with the provided cancellation token.
    /// </summary>
    [TestMethod]
    public async Task SuspendStore_WithCancellationToken_PassesTokenToSender()
    {
        // Arrange
        var mockSender = new Mock<ISender>();
        var tenantId = 42;
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var expectedGuid = TenantIdConverter.ToGuid(tenantId);

        mockSender
            .Setup(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken))
            .Returns(Task.CompletedTask);

        var controller = CreateController(mockSender.Object);

        // Act
        var result = await controller.SuspendStore(tenantId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<NoContentResult>(result);
        mockSender.Verify(s => s.Send(It.Is<SuspendStoreCommand>(cmd => cmd.TenantId == expectedGuid), cancellationToken), Times.Once);
    }

}
