using MediatR;

namespace Catalog.Application.Products.Commands.AddVariant
{
    public sealed record AddVariantCommand(
        Guid StoreId,
        Guid ProductId,
        string Sku,
        string? Name,
        int SortOrder,
        IReadOnlyCollection<VariantAttributeValueInput> AttributeValues) : IRequest<Guid>;
}
