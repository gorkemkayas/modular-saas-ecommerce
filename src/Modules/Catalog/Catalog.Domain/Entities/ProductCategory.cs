using Catalog.Domain.Common;

namespace Catalog.Domain.Entities
{
    public sealed class ProductCategory
    {
        public Guid ProductId { get; private set; }
        public Guid CategoryId { get; private set; }
        public bool IsPrimary { get; private set; }
        public int SortOrder { get; private set; }

        private ProductCategory()
        {
        }

        private ProductCategory(Guid productId, Guid categoryId, bool isPrimary, int sortOrder)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId cannot be empty.", nameof(productId));

            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));

            ProductId = productId;
            CategoryId = categoryId;
            IsPrimary = isPrimary;
            SortOrder = sortOrder;
        }

        public static ProductCategory Create(Guid productId, Guid categoryId, bool isPrimary = false, int sortOrder = 0)
        {
            return new ProductCategory(productId, categoryId, isPrimary, sortOrder);
        }

        public void SetPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
        }

        public void ChangeSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
        }
    }
}
