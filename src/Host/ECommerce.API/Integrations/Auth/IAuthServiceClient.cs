namespace ECommerce.API.Integrations.Auth;

public interface IAuthServiceClient
{
    Task<AuthServiceRegisterOutcome> RegisterAsync(
        AuthServiceRegisterCommand command,
        CancellationToken cancellationToken = default);

    Task<AuthServiceLoginOutcome> LoginAsync(
        AuthServiceLoginCommand command,
        CancellationToken cancellationToken = default);
}
