using Catalog.Domain.Common;

namespace Catalog.Domain.ValueObjects
{
    public sealed class Sku : ValueObject
    {
        public string Value { get; }

        private Sku(string value)
        {
            Value = value;
        }

        public static Sku Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("SKU cannot be empty.", nameof(value));

            var normalized = value.Trim().ToUpperInvariant();

            if (normalized.Length > 100)
                throw new ArgumentException("SKU is too long.", nameof(value));

            return new Sku(normalized);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
