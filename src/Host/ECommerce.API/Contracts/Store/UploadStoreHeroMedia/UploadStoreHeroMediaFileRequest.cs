using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Contracts.Store.UploadStoreHeroMedia
{
    public sealed record UploadStoreHeroMediaFileRequest(IFormFile? File);
}
