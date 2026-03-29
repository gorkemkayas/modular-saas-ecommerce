namespace ECommerce.API.Contracts.Catalog.Brands
{
    public sealed record SearchBrandsRequest(string? SearchTerm, bool ActiveOnly = false);

    public sealed record CreateBrandRequest(
        string Name,
        string Slug,
        string? Description);

    public sealed record UpdateBrandRequest(
        string Name,
        string Slug,
        string? Description);
}
