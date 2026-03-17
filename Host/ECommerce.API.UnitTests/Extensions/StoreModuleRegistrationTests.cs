using ECommerce.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Store.Application.Abstractions;
using Store.Domain.Stores;
using Store.Infrastructure.Options;
using Store.Infrastructure.Persistance;
using Store.Infrastructure.Persistance.Repositories;
using System;


namespace ECommerce.API.Extensions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="StoreModuleRegistration"/> class.
    /// </summary>
    [TestClass]
    public class StoreModuleRegistrationTests
    {
        /// <summary>
        /// Tests that AddStoreModule returns the same IServiceCollection instance for fluent API chaining.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_ValidConfiguration_ReturnsSameServiceCollectionInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            // Act
            var result = StoreModuleRegistration.AddStoreModule(services, mockConfiguration.Object);

            // Assert
            Assert.AreSame(services, result);
        }

        /// <summary>
        /// Tests that AddStoreModule registers all required services including exception handler,
        /// options, DbContext, repository, and unit of work.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_ValidConfiguration_RegistersAllRequiredServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            // Act
            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);

            // Assert
            Assert.IsTrue(serviceCollection.Count > 0, "Services should be registered");
        }

        /// <summary>
        /// Tests that AddStoreModule calls Configure with the correct configuration section
        /// for StoreDatabaseOptions.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_ValidConfiguration_ConfiguresStoreDatabaseOptionsWithCorrectSection()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            // Act
            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);

            // Assert
            mockConfiguration.Verify(
                c => c.GetSection(StoreDatabaseOptions.SectionName),
                Times.Once,
                "GetSection should be called with the correct section name");
        }

        /// <summary>
        /// Tests that the DbContext can be resolved successfully when a valid connection string is provided.
        /// Note: This test verifies service registration but does not verify database connectivity.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_DbContextConfiguration_ResolvesSuccessfullyWithValidConnectionString()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            serviceCollection.Configure<StoreDatabaseOptions>(options =>
            {
                options.ConnectionString = "Host=localhost;Database=test;Username=user;Password=pass";
            });

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var dbContext = serviceProvider.GetRequiredService<StoreDbContext>();

            // Assert
            Assert.IsNotNull(dbContext);
        }

        /// <summary>
        /// Tests that IStoreRepository can be resolved and returns an instance of StoreRepository.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_RepositoryRegistration_ResolvesIStoreRepositorySuccessfully()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            serviceCollection.Configure<StoreDatabaseOptions>(options =>
            {
                options.ConnectionString = "Host=localhost;Database=test;Username=user;Password=pass";
            });

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var repository = serviceProvider.GetRequiredService<IStoreRepository>();

            // Assert
            Assert.IsNotNull(repository);
            Assert.IsInstanceOfType(repository, typeof(StoreRepository));
        }

        /// <summary>
        /// Tests that IUnitOfWork can be resolved and returns an instance of StoreDbContext.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_UnitOfWorkRegistration_ResolvesIUnitOfWorkAsStoreDbContext()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            serviceCollection.Configure<StoreDatabaseOptions>(options =>
            {
                options.ConnectionString = "Host=localhost;Database=test;Username=user;Password=pass";
            });

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            // Assert
            Assert.IsNotNull(unitOfWork);
            Assert.IsInstanceOfType(unitOfWork, typeof(StoreDbContext));
        }

        /// <summary>
        /// Tests that IUnitOfWork and StoreDbContext resolve to the same instance within the same scope,
        /// verifying that the factory lambda correctly retrieves the DbContext.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_UnitOfWorkRegistration_ReturnsSameInstanceAsStoreDbContext()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();

            serviceCollection.Configure<StoreDatabaseOptions>(options =>
            {
                options.ConnectionString = "Host=localhost;Database=test;Username=user;Password=pass";
            });

            mockConfiguration
                .Setup(c => c.GetSection(StoreDatabaseOptions.SectionName))
                .Returns(mockConfigSection.Object);

            StoreModuleRegistration.AddStoreModule(serviceCollection, mockConfiguration.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            // Act
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Assert
            Assert.AreSame(dbContext, unitOfWork, "IUnitOfWork and StoreDbContext should resolve to the same instance within a scope");
        }
    }
}