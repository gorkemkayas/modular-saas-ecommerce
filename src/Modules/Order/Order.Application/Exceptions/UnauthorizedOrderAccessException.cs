namespace Order.Application.Exceptions;

public sealed class UnauthorizedOrderAccessException : ApplicationException
{
    public UnauthorizedOrderAccessException()
        : base("You are not authorized to access this order.")
    {
    }
}
