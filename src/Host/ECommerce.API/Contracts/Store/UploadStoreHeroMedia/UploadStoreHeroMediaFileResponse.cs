namespace ECommerce.API.Contracts.Store.UploadStoreHeroMedia
{
    public sealed record UploadStoreHeroMediaFileResponse(
        string Url,
        string MediaType,
        string OriginalFileName);
}
