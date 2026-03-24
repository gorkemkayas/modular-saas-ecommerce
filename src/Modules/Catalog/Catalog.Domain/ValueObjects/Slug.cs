using Catalog.Domain.Common;
using System.Text.RegularExpressions;

namespace Catalog.Domain.ValueObjects
{
    public sealed class Slug : ValueObject
    {
        public string Value { get; }

        private Slug(string value)
        {
            Value = value;
        }

        public static Slug Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Slug cannot be empty.", nameof(value));

            var normalized = value.Trim().ToLowerInvariant();

            if (!Regex.IsMatch(normalized, "^[a-z0-9]+(-[a-z0-9]+)*$"))
                throw new ArgumentException("Slug format is invalid.", nameof(value));

            return new Slug(normalized);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
