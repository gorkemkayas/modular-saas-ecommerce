using ECommerce.API.ExceptionHandlers;
using Payment.Infrastructure.DependencyInjection;

namespace ECommerce.API.Extensions;

public static class PaymentModuleRegistration
{
    public static IServiceCollection AddPaymentModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<PaymentExceptionHandler>();

        return services.AddPaymentInfrastructure(configuration);
    }
}
