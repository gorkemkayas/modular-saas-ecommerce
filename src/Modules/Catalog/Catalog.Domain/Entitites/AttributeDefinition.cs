using Catalog.Domain.Common;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entitites
{
    public sealed class AttributeDefinition : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid StoreId { get; private set; }
        public string Name { get; private set; } = default!;
        public AttributeCode Code { get; private set; } = default!;
        public AttributeDataType DataType { get; private set; }
        public bool IsRequired { get; private set; }
        public bool IsFilterable { get; private set; }
        public bool IsVariantDefining { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private AttributeDefinition()
        {
        }

        private AttributeDefinition(
            Guid id,
            Guid storeId,
            string name,
            AttributeCode code,
            AttributeDataType dataType,
            bool isRequired,
            bool isFilterable,
            bool isVariantDefining)
        {
            if (storeId == Guid.Empty)
                throw new ArgumentException("StoreId cannot be empty.", nameof(storeId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be empty.", nameof(name));

            Id = id;
            StoreId = storeId;
            Name = name.Trim();
            Code = code;
            DataType = dataType;
            IsRequired = isRequired;
            IsFilterable = isFilterable;
            IsVariantDefining = isVariantDefining;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static AttributeDefinition Create(
            Guid storeId,
            string name,
            AttributeCode code,
            AttributeDataType dataType,
            bool isRequired = false,
            bool isFilterable = false,
            bool isVariantDefining = false)
        {
            return new AttributeDefinition(
                Guid.NewGuid(),
                storeId,
                name,
                code,
                dataType,
                isRequired,
                isFilterable,
                isVariantDefining);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be empty.", nameof(name));

            Name = name.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeCode(AttributeCode code)
        {
            Code = code;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeDataType(AttributeDataType dataType)
        {
            DataType = dataType;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetRequired(bool isRequired)
        {
            IsRequired = isRequired;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetFilterable(bool isFilterable)
        {
            IsFilterable = isFilterable;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetVariantDefining(bool isVariantDefining)
        {
            IsVariantDefining = isVariantDefining;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
