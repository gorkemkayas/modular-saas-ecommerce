using Catalog.Domain.Common;
using System.Text.RegularExpressions;

namespace Catalog.Domain.ValueObjects
{
    public sealed class AttributeCode : ValueObject
    {
        public string Value { get; }

        private AttributeCode(string value)
        {
            Value = value;
        }

        public static AttributeCode Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Attribute code cannot be empty.", nameof(value));

            var normalized = Normalize(value);

            if (!IsValid(normalized))
                throw new ArgumentException("Attribute code format is invalid.", nameof(value));

            return new AttributeCode(normalized);
        }

        private static string Normalize(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"\s+", "-");
            normalized = Regex.Replace(normalized, @"-+", "-");
            return normalized;
        }

        private static bool IsValid(string value)
        {
            return Regex.IsMatch(value, "^[a-z0-9]+(-[a-z0-9]+)*$");
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
