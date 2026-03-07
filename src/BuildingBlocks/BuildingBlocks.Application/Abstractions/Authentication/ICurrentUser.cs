namespace BuildingBlocks.Application.Abstractions.Authentication
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? Name { get; }
        string? TokenType { get; }
        bool IsAuthenticated { get; }
    }
}
