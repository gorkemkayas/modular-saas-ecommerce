using Catalog.Domain.Enums;

namespace ECommerce.API.Integrations.Media;

public interface IProductMediaStorageService
{
    Task<StoredProductMediaFile> UploadAsync(
        Guid storeId,
        IFormFile file,
        CancellationToken cancellationToken = default);
}
