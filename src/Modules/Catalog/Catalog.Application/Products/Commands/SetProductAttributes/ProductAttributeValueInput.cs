namespace Catalog.Application.Products.Commands.SetProductAttributes
{
    public sealed record ProductAttributeValueInput(
        Guid AttributeDefinitionId,
        string Value);
}
