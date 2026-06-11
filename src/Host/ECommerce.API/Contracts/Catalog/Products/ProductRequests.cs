using Catalog.Domain.Enums;

namespace ECommerce.API.Contracts.Catalog.Products
{
    public sealed record StoreProductSearchRequest(
        string? SearchTerm,
        ProductStatus? Status,
        ProductType? ProductType,
        bool? IsPublished,
        Guid? CategoryId,
        Guid? BrandId,
        int PageNumber = 1,
        int PageSize = 20);

    public sealed record CreateSimpleProductRequest(
        string Name,
        string Slug,
        string Sku,
        string? ShortDescription,
        string? Description,
        Guid? BrandId,
        IReadOnlyCollection<Guid>? CategoryIds);

    public sealed record CreateVariantProductRequest(
        string Name,
        string Slug,
        string? ShortDescription,
        string? Description,
        Guid? BrandId,
        IReadOnlyCollection<Guid>? CategoryIds);

    public sealed record UpdateProductDetailsRequest(
        string Name,
        string? ShortDescription,
        string? Description,
        Guid? BrandId);

    public sealed record ChangeProductSlugRequest(string Slug);

    public sealed record AssignProductCategoriesRequest(IReadOnlyCollection<Guid> CategoryIds);

    public sealed record ProductAttributeValueRequest(Guid AttributeDefinitionId, string Value);

    public sealed record SetProductAttributesRequest(IReadOnlyCollection<ProductAttributeValueRequest> AttributeValues);

    public sealed record VariantAttributeValueRequest(Guid AttributeDefinitionId, string Value);

    public sealed record AddVariantRequest(
        string Sku,
        string? Name,
        int SortOrder,
        IReadOnlyCollection<VariantAttributeValueRequest> AttributeValues);

    public sealed record AddProductMediaRequest(
        MediaType MediaType,
        string Url,
        string? AltText,
        bool IsMain,
        int SortOrder,
        Guid? ProductVariantId);

    public sealed class UploadProductMediaFileRequest
    {
        public IFormFile File { get; init; } = default!;
    }

    public sealed record UploadProductMediaFileResponse(
        string Url,
        MediaType MediaType,
        string OriginalFileName);
}
