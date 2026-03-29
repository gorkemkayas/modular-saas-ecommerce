namespace Catalog.Application.Exceptions
{
    public sealed class DuplicateAttributeCodeException : ApplicationException
    {
        public DuplicateAttributeCodeException(string code)
            : base($"Attribute code '{code}' is already in use.")
        {
            Code = code;
        }

        public string Code { get; }
    }
}
