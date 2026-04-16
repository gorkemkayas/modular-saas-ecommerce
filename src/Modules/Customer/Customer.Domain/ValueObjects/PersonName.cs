using Customer.Domain.Common;

namespace Customer.Domain.ValueObjects;

public sealed class PersonName : ValueObject
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}".Trim();

    private PersonName()
    {
    }

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static PersonName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        var normalizedFirstName = Normalize(firstName, nameof(firstName));
        var normalizedLastName = Normalize(lastName, nameof(lastName));

        return new PersonName(normalizedFirstName, normalizedLastName);
    }

    private static string Normalize(string value, string parameterName)
    {
        var normalized = value.Trim();

        if (normalized.Length > 100)
            throw new ArgumentException("Name value is too long.", parameterName);

        return normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
