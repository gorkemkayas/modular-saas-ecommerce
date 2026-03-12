namespace Store.Application.Exceptions
{
    public sealed class StoreNotFoundByIdException : ApplicationException
    {
        public Guid StoreId { get; }

        public StoreNotFoundByIdException(Guid storeId)
            : base($"Store with ID {storeId} not found.")
        {
            StoreId = storeId;
        }
    }
}
