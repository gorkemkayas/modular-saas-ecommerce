namespace Catalog.Application.Products.Commands.AddVariant
{
    public sealed record VariantAttributeValueInput(
        Guid AttributeDefinitionId,
        string Value);
}
