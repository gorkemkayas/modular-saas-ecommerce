using MediatR;

namespace Catalog.Application.Products.Commands.SetProductAttributes
{
    public sealed record SetProductAttributesCommand(
        Guid StoreId,
        Guid ProductId,
        IReadOnlyCollection<ProductAttributeValueInput> AttributeValues) : IRequest;
}
