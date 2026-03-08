using Store.Domain.Stores;

namespace Store.Application.DTOs
{
    public sealed record StoreDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    StoreStatus Status,
    bool IsPublished);
}
