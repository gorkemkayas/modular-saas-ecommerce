using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Store.Domain;
using Store.Domain.Abstractions;
using Store.Domain.Exceptions;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Domain.Stores.UnitTests
{
    [TestClass]
    public sealed class StoreTests
    {
        /// <summary>
        /// Tests that UpdateProfile successfully updates with null description and logoUrl.
        /// Input: Valid name, null description, null logoUrl.
        /// Expected: Name is updated, description and logoUrl are set to null.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_ValidNameWithNullDescriptionAndLogoUrl_UpdatesNameAndSetsNullValues()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, "Original Description", "Original Logo");
            var newName = "New Store Name";

            // Act
            store.UpdateProfile(newName, null, null);

            // Assert
            Assert.AreEqual("New Store Name", store.Name);
            Assert.IsNull(store.Description);
            Assert.IsNull(store.LogoUrl);
            Assert.IsNotNull(store.UpdatedAtUtc);
        }

        /// <summary>
        /// Tests that UpdateProfile trims whitespace from name parameter.
        /// Input: Name with leading and trailing whitespace.
        /// Expected: Name is stored with whitespace trimmed.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_NameWithWhitespace_TrimsWhitespace()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act
            store.UpdateProfile("   Test Store   ", null, null);

            // Assert
            Assert.AreEqual("Test Store", store.Name);
        }

        /// <summary>
        /// Tests that UpdateProfile trims whitespace from description parameter.
        /// Input: Description with leading and trailing whitespace.
        /// Expected: Description is stored with whitespace trimmed.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_DescriptionWithWhitespace_TrimsWhitespace()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act
            store.UpdateProfile("Test Store", "   Test Description   ", null);

            // Assert
            Assert.AreEqual("Test Description", store.Description);
        }

        /// <summary>
        /// Tests that UpdateProfile trims whitespace from logoUrl parameter.
        /// Input: LogoUrl with leading and trailing whitespace.
        /// Expected: LogoUrl is stored with whitespace trimmed.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_LogoUrlWithWhitespace_TrimsWhitespace()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act
            store.UpdateProfile("Test Store", null, "   https://example.com/logo.png   ");

            // Assert
            Assert.AreEqual("https://example.com/logo.png", store.LogoUrl);
        }

        /// <summary>
        /// Tests that UpdateProfile updates UpdatedAtUtc to a recent time.
        /// Input: Valid update parameters.
        /// Expected: UpdatedAtUtc is set to current UTC time.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_ValidInput_UpdatesUpdatedAtUtc()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);
            var beforeUpdate = DateTime.UtcNow;

            // Act
            store.UpdateProfile("New Name", "New Description", "New Logo");
            var afterUpdate = DateTime.UtcNow;

            // Assert
            Assert.IsTrue(store.UpdatedAtUtc >= beforeUpdate);
            Assert.IsTrue(store.UpdatedAtUtc <= afterUpdate);
        }

        /// <summary>
        /// Tests that UpdateProfile handles empty string for description correctly.
        /// Input: Empty string for description.
        /// Expected: Description is set to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_EmptyDescription_SetsEmptyDescription()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, "Original Description", null);

            // Act
            store.UpdateProfile("New Name", string.Empty, null);

            // Assert
            Assert.AreEqual(string.Empty, store.Description);
        }

        /// <summary>
        /// Tests that UpdateProfile handles empty string for logoUrl correctly.
        /// Input: Empty string for logoUrl.
        /// Expected: LogoUrl is set to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_EmptyLogoUrl_SetsEmptyLogoUrl()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, "Original Logo");

            // Act
            store.UpdateProfile("New Name", null, string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, store.LogoUrl);
        }

        /// <summary>
        /// Tests that UpdateProfile handles whitespace-only description by trimming to empty string.
        /// Input: Whitespace-only description.
        /// Expected: Description is set to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_WhitespaceOnlyDescription_SetsEmptyDescription()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act
            store.UpdateProfile("New Name", "   ", null);

            // Assert
            Assert.AreEqual(string.Empty, store.Description);
        }

        /// <summary>
        /// Tests that UpdateProfile handles whitespace-only logoUrl by trimming to empty string.
        /// Input: Whitespace-only logoUrl.
        /// Expected: LogoUrl is set to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_WhitespaceOnlyLogoUrl_SetsEmptyLogoUrl()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act
            store.UpdateProfile("New Name", null, "   ");

            // Assert
            Assert.AreEqual(string.Empty, store.LogoUrl);
        }

        /// <summary>
        /// Tests that UpdateProfile handles very long name string.
        /// Input: Very long valid name string.
        /// Expected: Name is updated successfully with trimmed value.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_VeryLongName_UpdatesSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);
            var longName = new string('A', 10000);

            // Act
            store.UpdateProfile(longName, null, null);

            // Assert
            Assert.AreEqual(longName, store.Name);
        }

        /// <summary>
        /// Tests that UpdateProfile handles special characters in name.
        /// Input: Name with special characters.
        /// Expected: Name is updated with special characters preserved.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_NameWithSpecialCharacters_UpdatesSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);
            var specialName = "Store's Name & More! @#$%^&*()";

            // Act
            store.UpdateProfile(specialName, null, null);

            // Assert
            Assert.AreEqual(specialName, store.Name);
        }

        /// <summary>
        /// Tests that UpdateProfile handles unicode characters in name, description, and logoUrl.
        /// Input: Unicode strings for all parameters.
        /// Expected: All properties are updated with unicode characters preserved.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_UnicodeCharacters_UpdatesSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);
            var unicodeName = "商店名称 🛍️";
            var unicodeDescription = "説明文 ✨";
            var unicodeLogoUrl = "https://例え.com/ロゴ.png";

            // Act
            store.UpdateProfile(unicodeName, unicodeDescription, unicodeLogoUrl);

            // Assert
            Assert.AreEqual(unicodeName, store.Name);
            Assert.AreEqual(unicodeDescription, store.Description);
            Assert.AreEqual(unicodeLogoUrl, store.LogoUrl);
        }

        /// <summary>
        /// Tests that UpdateProfile can be called multiple times successfully.
        /// Input: Multiple sequential calls with different parameters.
        /// Expected: Each call updates the properties correctly.
        /// </summary>
        [TestMethod]
        public void UpdateProfile_MultipleCalls_UpdatesSuccessfully()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Original Name", slug, null, null);

            // Act & Assert - First update
            store.UpdateProfile("First Name", "First Description", "First Logo");
            Assert.AreEqual("First Name", store.Name);
            Assert.AreEqual("First Description", store.Description);
            Assert.AreEqual("First Logo", store.LogoUrl);

            // Act & Assert - Second update
            store.UpdateProfile("Second Name", "Second Description", "Second Logo");
            Assert.AreEqual("Second Name", store.Name);
            Assert.AreEqual("Second Description", store.Description);
            Assert.AreEqual("Second Logo", store.LogoUrl);

            // Act & Assert - Third update
            store.UpdateProfile("Third Name", null, null);
            Assert.AreEqual("Third Name", store.Name);
            Assert.IsNull(store.Description);
            Assert.IsNull(store.LogoUrl);
        }

        /// <summary>
        /// Tests that Unpublish sets IsPublished to false and updates UpdatedAtUtc
        /// when the store is not archived.
        /// </summary>
        [TestMethod]
        public void Unpublish_WhenStoreIsNotArchived_SetsIsPublishedToFalseAndUpdatesTimestamp()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);
            var beforeUnpublish = DateTime.UtcNow;

            // Act
            store.Unpublish();

            // Assert
            Assert.IsFalse(store.IsPublished);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeUnpublish);
            Assert.IsTrue(store.UpdatedAtUtc <= DateTime.UtcNow.AddSeconds(1));
        }

        /// <summary>
        /// Tests that Unpublish can be called multiple times and updates the timestamp each time
        /// when the store is not archived.
        /// </summary>
        [TestMethod]
        public void Unpublish_WhenCalledMultipleTimes_UpdatesTimestampEachTime()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);

            store.Unpublish();
            var firstUpdateTime = store.UpdatedAtUtc;

            // Small delay to ensure different timestamps
            System.Threading.Thread.Sleep(10);

            // Act
            store.Unpublish();

            // Assert
            Assert.IsFalse(store.IsPublished);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc > firstUpdateTime);
        }

        /// <summary>
        /// Tests that Create successfully creates a store with valid required parameters and null optional parameters.
        /// </summary>
        [TestMethod]
        public void Create_WithValidParametersAndNullOptionalFields_ReturnsStoreWithCorrectProperties()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            DateTime beforeCreation = DateTime.UtcNow;

            // Act
            Store store = Store.Create(tenantId, name, slug);
            DateTime afterCreation = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(store);
            Assert.AreNotEqual(Guid.Empty, store.Id);
            Assert.AreEqual(tenantId, store.TenantId);
            Assert.AreEqual(name, store.Name);
            Assert.AreEqual(slug, store.Slug);
            Assert.IsNull(store.Description);
            Assert.IsNull(store.LogoUrl);
            Assert.AreEqual(StoreStatus.PendingPayment, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsTrue(store.CreatedAtUtc >= beforeCreation && store.CreatedAtUtc <= afterCreation);
            Assert.IsNull(store.UpdatedAtUtc);
        }

        /// <summary>
        /// Tests that Create successfully creates a store with all parameters provided including optional ones.
        /// </summary>
        [TestMethod]
        public void Create_WithAllValidParameters_ReturnsStoreWithCorrectProperties()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            string description = "Test Description";
            string logoUrl = "https://example.com/logo.png";
            DateTime beforeCreation = DateTime.UtcNow;

            // Act
            Store store = Store.Create(tenantId, name, slug, description, logoUrl);
            DateTime afterCreation = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(store);
            Assert.AreNotEqual(Guid.Empty, store.Id);
            Assert.AreEqual(tenantId, store.TenantId);
            Assert.AreEqual(name, store.Name);
            Assert.AreEqual(slug, store.Slug);
            Assert.AreEqual(description, store.Description);
            Assert.AreEqual(logoUrl, store.LogoUrl);
            Assert.AreEqual(StoreStatus.PendingPayment, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsTrue(store.CreatedAtUtc >= beforeCreation && store.CreatedAtUtc <= afterCreation);
            Assert.IsNull(store.UpdatedAtUtc);
        }

        /// <summary>
        /// Tests that Create trims leading and trailing whitespace from the name parameter.
        /// </summary>
        [TestMethod]
        public void Create_WithWhitespaceInName_TrimsName()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string nameWithWhitespace = "  Test Store  ";
            string expectedName = "Test Store";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, nameWithWhitespace, slug);

            // Assert
            Assert.AreEqual(expectedName, store.Name);
        }

        /// <summary>
        /// Tests that Create trims leading and trailing whitespace from the description parameter.
        /// </summary>
        [TestMethod]
        public void Create_WithWhitespaceInDescription_TrimsDescription()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            string descriptionWithWhitespace = "  Test Description  ";
            string expectedDescription = "Test Description";

            // Act
            Store store = Store.Create(tenantId, name, slug, descriptionWithWhitespace);

            // Assert
            Assert.AreEqual(expectedDescription, store.Description);
        }

        /// <summary>
        /// Tests that Create trims leading and trailing whitespace from the logoUrl parameter.
        /// </summary>
        [TestMethod]
        public void Create_WithWhitespaceInLogoUrl_TrimsLogoUrl()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            string logoUrlWithWhitespace = "  https://example.com/logo.png  ";
            string expectedLogoUrl = "https://example.com/logo.png";

            // Act
            Store store = Store.Create(tenantId, name, slug, null, logoUrlWithWhitespace);

            // Assert
            Assert.AreEqual(expectedLogoUrl, store.LogoUrl);
        }

        /// <summary>
        /// Tests that Create generates a unique Id for each store instance.
        /// </summary>
        [TestMethod]
        public void Create_WithValidParameters_GeneratesUniqueId()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store1 = Store.Create(tenantId, name, slug);
            Store store2 = Store.Create(tenantId, name, slug);

            // Assert
            Assert.AreNotEqual(store1.Id, store2.Id);
            Assert.AreNotEqual(Guid.Empty, store1.Id);
            Assert.AreNotEqual(Guid.Empty, store2.Id);
        }

        /// <summary>
        /// Tests that Create sets Status to PendingPayment by default.
        /// </summary>
        [TestMethod]
        public void Create_WithValidParameters_SetsStatusToPendingPayment()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, name, slug);

            // Assert
            Assert.AreEqual(StoreStatus.PendingPayment, store.Status);
        }

        /// <summary>
        /// Tests that Create sets IsPublished to false by default.
        /// </summary>
        [TestMethod]
        public void Create_WithValidParameters_SetsIsPublishedToFalse()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, name, slug);

            // Assert
            Assert.IsFalse(store.IsPublished);
        }

        /// <summary>
        /// Tests that Create sets UpdatedAtUtc to null initially.
        /// </summary>
        [TestMethod]
        public void Create_WithValidParameters_SetsUpdatedAtUtcToNull()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, name, slug);

            // Assert
            Assert.IsNull(store.UpdatedAtUtc);
        }

        /// <summary>
        /// Tests that Create with empty string description sets Description to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void Create_WithEmptyStringDescription_SetsDescriptionToEmptyString()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            string emptyDescription = string.Empty;

            // Act
            Store store = Store.Create(tenantId, name, slug, emptyDescription);

            // Assert
            Assert.AreEqual(string.Empty, store.Description);
        }

        /// <summary>
        /// Tests that Create with empty string logoUrl sets LogoUrl to empty string after trimming.
        /// </summary>
        [TestMethod]
        public void Create_WithEmptyStringLogoUrl_SetsLogoUrlToEmptyString()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string name = "Test Store";
            Slug slug = Slug.Create("test-store");
            string emptyLogoUrl = string.Empty;

            // Act
            Store store = Store.Create(tenantId, name, slug, null, emptyLogoUrl);

            // Assert
            Assert.AreEqual(string.Empty, store.LogoUrl);
        }

        /// <summary>
        /// Tests that Create with very long name successfully creates store.
        /// </summary>
        [TestMethod]
        public void Create_WithVeryLongName_CreatesStoreSuccessfully()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string veryLongName = new string('A', 10000);
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, veryLongName, slug);

            // Assert
            Assert.AreEqual(veryLongName, store.Name);
        }

        /// <summary>
        /// Tests that Create with special characters in name successfully creates store.
        /// </summary>
        [TestMethod]
        public void Create_WithSpecialCharactersInName_CreatesStoreSuccessfully()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string nameWithSpecialChars = "Test Store @#$%^&*()!";
            Slug slug = Slug.Create("test-store");

            // Act
            Store store = Store.Create(tenantId, nameWithSpecialChars, slug);

            // Assert
            Assert.AreEqual(nameWithSpecialChars, store.Name);
        }

        /// <summary>
        /// Tests that Suspend sets Status to Suspended when store is in Active state.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsActive_ShouldSetStatusToSuspended()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);

            // Act
            store.Suspend();

            // Assert
            Assert.AreEqual(StoreStatus.Suspended, store.Status);
        }

        /// <summary>
        /// Tests that Suspend sets IsPublished to false when store is in Active state.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsActive_ShouldSetIsPublishedToFalse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);

            // Act
            store.Suspend();

            // Assert
            Assert.IsFalse(store.IsPublished);
        }

        /// <summary>
        /// Tests that Suspend updates UpdatedAtUtc to a recent timestamp when store is in Active state.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsActive_ShouldUpdateUpdatedAtUtc()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            var beforeSuspend = DateTime.UtcNow;

            // Act
            store.Suspend();
            var afterSuspend = DateTime.UtcNow;

            // Assert
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeSuspend);
            Assert.IsTrue(store.UpdatedAtUtc <= afterSuspend);
        }

        /// <summary>
        /// Tests that Suspend sets IsPublished to false even when store was published before suspension.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsPublished_ShouldUnpublishStore()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            store.Activate();
            store.Publish();

            // Act
            store.Suspend();

            // Assert
            Assert.IsFalse(store.IsPublished);
            Assert.AreEqual(StoreStatus.Suspended, store.Status);
        }

        /// <summary>
        /// Tests that Suspend can be called on an already suspended store and updates the timestamp.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsAlreadySuspended_ShouldUpdateTimestamp()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            store.Suspend();
            var firstSuspendTime = store.UpdatedAtUtc;
            System.Threading.Thread.Sleep(10);

            // Act
            store.Suspend();

            // Assert
            Assert.AreEqual(StoreStatus.Suspended, store.Status);
            Assert.IsTrue(store.UpdatedAtUtc > firstSuspendTime);
        }

        /// <summary>
        /// Tests that Suspend does not change Status when store is Archived and exception is thrown.
        /// </summary>
        [TestMethod]
        public void Suspend_WhenStoreIsArchived_ShouldNotChangeStatus()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, "Test Store", slug);
            store.Archive();

            // Act
            try
            {
                store.Suspend();
            }
            catch (ArchivedStoreException)
            {
                // Expected exception
            }

            // Assert
            Assert.AreEqual(StoreStatus.Archived, store.Status);
        }

        /// <summary>
        /// Tests that Activate successfully changes status from Suspended to Active and updates the timestamp.
        /// </summary>
        [TestMethod]
        public void Activate_WhenStatusIsSuspended_ShouldSetStatusToActiveAndUpdateTimestamp()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = Store.Create(tenantId, name, slug);
            store.Suspend();
            var beforeActivation = DateTime.UtcNow;

            // Act
            store.Activate();

            // Assert
            Assert.AreEqual(StoreStatus.Active, store.Status);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeActivation);
            Assert.IsTrue(store.UpdatedAtUtc <= DateTime.UtcNow.AddSeconds(1));
        }

        /// <summary>
        /// Tests that Activate successfully updates timestamp when status is already Active.
        /// </summary>
        [TestMethod]
        public void Activate_WhenStatusIsAlreadyActive_ShouldUpdateTimestamp()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = Store.Create(tenantId, name, slug);
            var initialUpdatedAt = store.UpdatedAtUtc;
            var beforeActivation = DateTime.UtcNow;

            // Act
            store.Activate();

            // Assert
            Assert.AreEqual(StoreStatus.Active, store.Status);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeActivation);
            Assert.IsTrue(store.UpdatedAtUtc <= DateTime.UtcNow.AddSeconds(1));
        }

        /// <summary>
        /// Tests that Archive method sets the Status property to Archived.
        /// </summary>
        [TestMethod]
        public void Archive_WhenCalled_SetsStatusToArchived()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);

            // Act
            store.Archive();

            // Assert
            Assert.AreEqual(StoreStatus.Archived, store.Status);
        }

        /// <summary>
        /// Tests that Archive method sets IsPublished property to false.
        /// </summary>
        [TestMethod]
        public void Archive_WhenCalled_SetsIsPublishedToFalse()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = global::Store.Domain.ValueObjects.Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);

            // Act
            store.Archive();

            // Assert
            Assert.IsFalse(store.IsPublished);
        }

        /// <summary>
        /// Tests that Archive method updates the UpdatedAtUtc property to current UTC time.
        /// </summary>
        [TestMethod]
        public void Archive_WhenCalled_UpdatesUpdatedAtUtc()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);
            var beforeArchive = DateTime.UtcNow;

            // Act
            store.Archive();

            // Assert
            var afterArchive = DateTime.UtcNow;
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeArchive);
            Assert.IsTrue(store.UpdatedAtUtc <= afterArchive);
        }

        /// <summary>
        /// Tests that Archive method can be called multiple times (idempotency).
        /// Verifies that Status remains Archived and IsPublished remains false.
        /// </summary>
        [TestMethod]
        public void Archive_WhenCalledMultipleTimes_RemainsArchived()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);

            // Act
            store.Archive();
            var firstUpdatedAt = store.UpdatedAtUtc;
            store.Archive();

            // Assert
            Assert.AreEqual(StoreStatus.Archived, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= firstUpdatedAt);
        }

        /// <summary>
        /// Tests that Archive method works correctly when called on a newly created store (Active status).
        /// Verifies all three state changes: Status to Archived, IsPublished to false, and UpdatedAtUtc is set.
        /// </summary>
        [TestMethod]
        public void Archive_WhenStoreIsActive_ChangesAllPropertiesCorrectly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = global::Store.Domain.Stores.Store.Create(tenantId, name, slug);
            var beforeArchive = DateTime.UtcNow;

            // Verify initial state
            Assert.AreEqual(StoreStatus.PendingPayment, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsNull(store.UpdatedAtUtc);

            // Act
            store.Archive();

            // Assert
            var afterArchive = DateTime.UtcNow;
            Assert.AreEqual(StoreStatus.Archived, store.Status);
            Assert.IsFalse(store.IsPublished);
            Assert.IsNotNull(store.UpdatedAtUtc);
            Assert.IsTrue(store.UpdatedAtUtc >= beforeArchive);
            Assert.IsTrue(store.UpdatedAtUtc <= afterArchive);
        }

        /// <summary>
        /// Tests that Publish successfully publishes an active store by setting IsPublished to true
        /// and updating the UpdatedAtUtc timestamp.
        /// </summary>
        [TestMethod]
        public void Publish_ActiveStore_SetsIsPublishedTrueAndUpdatesTimestamp()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = Store.Create(tenantId, name, slug);
            store.Activate();
            var beforePublish = DateTime.UtcNow.AddSeconds(-1);

            // Act
            store.Publish();

            // Assert
            Assert.IsTrue(store.IsPublished, "Store should be published after calling Publish.");
            Assert.IsNotNull(store.UpdatedAtUtc, "UpdatedAtUtc should be set after publishing.");
            Assert.IsTrue(store.UpdatedAtUtc >= beforePublish, "UpdatedAtUtc should be set to a recent timestamp.");
            Assert.IsTrue(store.UpdatedAtUtc <= DateTime.UtcNow.AddSeconds(1), "UpdatedAtUtc should not be in the future.");
        }

        /// <summary>
        /// Tests that Publish does not change IsPublished or UpdatedAtUtc when called on
        /// a suspended store and an exception is thrown.
        /// </summary>
        [TestMethod]
        public void Publish_SuspendedStore_DoesNotModifyState()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = Store.Create(tenantId, name, slug);
            store.Suspend();
            var originalIsPublished = store.IsPublished;
            var originalUpdatedAtUtc = store.UpdatedAtUtc;

            // Act
            try
            {
                store.Publish();
            }
            catch (InvalidStoreStatusException)
            {
                // Expected exception
            }

            // Assert
            Assert.AreEqual(originalIsPublished, store.IsPublished, "IsPublished should not be modified when publish fails.");
            Assert.AreEqual(originalUpdatedAtUtc, store.UpdatedAtUtc, "UpdatedAtUtc should not be modified when publish fails.");
        }

        /// <summary>
        /// Tests that Publish does not change IsPublished or UpdatedAtUtc when called on
        /// an archived store and an exception is thrown.
        /// </summary>
        [TestMethod]
        public void Publish_ArchivedStore_DoesNotModifyState()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Store";
            var slug = Slug.Create("test-store");
            var store = Store.Create(tenantId, name, slug);
            store.Archive();
            var originalIsPublished = store.IsPublished;
            var originalUpdatedAtUtc = store.UpdatedAtUtc;

            // Act
            try
            {
                store.Publish();
            }
            catch (ArchivedStoreException)
            {
                // Expected exception
            }

            // Assert
            Assert.AreEqual(originalIsPublished, store.IsPublished, "IsPublished should not be modified when publish fails.");
            Assert.AreEqual(originalUpdatedAtUtc, store.UpdatedAtUtc, "UpdatedAtUtc should not be modified when publish fails.");
        }
    }
}