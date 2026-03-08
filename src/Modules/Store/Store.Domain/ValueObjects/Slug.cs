using Store.Domain.Abstractions;
using Store.Domain.Rules;
using System.Text.RegularExpressions;

namespace Store.Domain.ValueObjects
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
                throw new ArgumentException("Slug cannot be empty.");

            var normalized = value.Trim().ToLowerInvariant();

            if (normalized.StartsWith('-') || normalized.EndsWith('-'))
                throw new ArgumentException("Slug cannot start or end with '-'.");

            if (!Regex.IsMatch(normalized, "^[a-z0-9]+(-[a-z0-9]+)*$"))
                throw new ArgumentException("Slug format is invalid.");


            if (SlugReservedRules.ReservedSlugs.Contains(normalized))
                throw new ArgumentException("This slug is reserved.");

            return new Slug(normalized);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
