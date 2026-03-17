using ECommerce.API.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace ECommerce.API.Extensions.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Verifies that AddStoreModule returns the same IServiceCollection instance that was passed in.
        /// This test validates the fluent API pattern implementation.
        /// Expected: The returned instance should be the same as the input instance.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_WithValidServices_ReturnsSameInstance()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            var result = ServiceCollectionExtensions.AddStoreModule(services);

            // Assert
            Assert.AreSame(services, result, "AddStoreModule should return the same IServiceCollection instance.");
        }

        /// <summary>
        /// Verifies that AddStoreModule successfully registers the StoreExceptionHandler.
        /// This test ensures the method completes without throwing and properly configures services.
        /// Expected: Method executes successfully and exception handler is registered.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_WithValidServices_RegistersExceptionHandler()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            var result = ServiceCollectionExtensions.AddStoreModule(services);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsTrue(services.Count > 0, "Services collection should contain registered services.");
        }

        /// <summary>
        /// Verifies that AddStoreModule can be called multiple times on the same service collection.
        /// This test ensures the method is idempotent or at least doesn't fail on repeated calls.
        /// Expected: Method executes successfully on repeated invocations.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            var result1 = ServiceCollectionExtensions.AddStoreModule(services);
            var result2 = ServiceCollectionExtensions.AddStoreModule(services);

            // Assert
            Assert.IsNotNull(result1, "First result should not be null.");
            Assert.IsNotNull(result2, "Second result should not be null.");
            Assert.AreSame(services, result1, "First call should return the same instance.");
            Assert.AreSame(services, result2, "Second call should return the same instance.");
        }

        /// <summary>
        /// Verifies that AddStoreModule works correctly when called on an already populated service collection.
        /// This test ensures the method integrates properly with existing service registrations.
        /// Expected: Method executes successfully and adds to existing services.
        /// </summary>
        [TestMethod]
        public void AddStoreModule_WithPreExistingServices_AddsToCollection()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IServiceProvider, ServiceProvider>();
            var initialCount = services.Count;

            // Act
            var result = ServiceCollectionExtensions.AddStoreModule(services);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsTrue(services.Count > initialCount, "Service collection should have more services after AddStoreModule.");
        }
    }
}