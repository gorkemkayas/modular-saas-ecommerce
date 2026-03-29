namespace Catalog.Application.Exceptions
{
    public sealed class BrandNotFoundException : ApplicationException
    {
        public BrandNotFoundException(Guid brandId)
            : base($"Brand with id '{brandId}' was not found.")
        {
            BrandId = brandId;
        }

        public Guid BrandId { get; }
    }
}
