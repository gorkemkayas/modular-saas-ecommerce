using Catalog.Domain.Enums;

namespace Catalog.Domain.Entities
{
    public sealed class ProductMedia
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public Guid? ProductVariantId { get; private set; }
        public MediaType MediaType { get; private set; }
        public string Url { get; private set; } = default!;
        public string? AltText { get; private set; }
        public bool IsMain { get; private set; }
        public int SortOrder { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private ProductMedia()
        {
        }

        private ProductMedia(
            Guid id,
            Guid productId,
            MediaType mediaType,
            string url,
            string? altText,
            bool isMain,
            int sortOrder,
            Guid? productVariantId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Media url cannot be empty.", nameof(url));

            Id = id;
            ProductId = productId;
            ProductVariantId = productVariantId;
            MediaType = mediaType;
            Url = url.Trim();
            AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim();
            IsMain = isMain;
            SortOrder = sortOrder;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public static ProductMedia Create(
            Guid productId,
            MediaType mediaType,
            string url,
            string? altText = null,
            bool isMain = false,
            int sortOrder = 0,
            Guid? productVariantId = null)
        {
            return new ProductMedia(
                Guid.NewGuid(),
                productId,
                mediaType,
                url,
                altText,
                isMain,
                sortOrder,
                productVariantId);
        }

        public void ChangeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Media url cannot be empty.", nameof(url));

            Url = url.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeAltText(string? altText)
        {
            AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim();
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void MarkAsMain()
        {
            IsMain = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void UnmarkAsMain()
        {
            IsMain = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
