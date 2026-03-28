using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Application.Products.Commands.AddProductMedia
{
    public sealed record AddProductMediaCommand(
        Guid StoreId,
        Guid ProductId,
        MediaType MediaType,
        string Url,
        string? AltText,
        bool IsMain,
        int SortOrder,
        Guid? ProductVariantId) : IRequest;
}
