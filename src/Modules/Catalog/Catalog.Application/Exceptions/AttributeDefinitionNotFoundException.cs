namespace Catalog.Application.Exceptions
{
    public sealed class AttributeDefinitionNotFoundException : ApplicationException
    {
        public AttributeDefinitionNotFoundException(Guid attributeDefinitionId)
            : base($"Attribute definition with id '{attributeDefinitionId}' was not found.")
        {
            AttributeDefinitionId = attributeDefinitionId;
        }

        public Guid AttributeDefinitionId { get; }
    }
}
