namespace BuildingBlocks.Infrastructure.Authentication
{
    public sealed class UserRequestContext
    {
        public bool IsAuthenticated { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? TokenType { get; set; }
    }
}
