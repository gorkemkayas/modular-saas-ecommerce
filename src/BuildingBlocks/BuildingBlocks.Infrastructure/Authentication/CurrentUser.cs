using BuildingBlocks.Application.Abstractions.Authentication;

namespace BuildingBlocks.Infrastructure.Authentication
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly UserRequestContext _userRequestContext;

        public CurrentUser(UserRequestContext userRequestContext)
        {
            _userRequestContext = userRequestContext;
        }

        public Guid? UserId => _userRequestContext.UserId;

        public string? Email => _userRequestContext.Email;

        public string? Name => _userRequestContext.Name;

        public string? TokenType => _userRequestContext.TokenType;

        public bool IsAuthenticated => _userRequestContext.IsAuthenticated;
    }
}
