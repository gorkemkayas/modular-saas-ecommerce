using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Infrastructure.Extensions.Authentication;
using BuildingBlocks.Infrastructure.Extensions.Middleware;
using ECommerce.API.ExceptionHandlers;
using ECommerce.API.Extensions;
using ECommerce.API.Options;
using Notification.Infrastructure.DependencyInjection;
using Order.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using System.Data;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

Log.Logger = SerilogExtensions.CreateBootstrapLogger();

static async Task EnsureOrderDevelopmentSchemaAsync(IServiceProvider services, CancellationToken cancellationToken = default)
{
    await using var scope = services.CreateAsyncScope();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("OrderSchemaBootstrap");

    var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    await orderDbContext.Database.MigrateAsync(cancellationToken);

    if (await OrderTableExistsAsync(orderDbContext, cancellationToken))
        return;

    logger.LogWarning(
        "Order migrations completed but the Orders table is still missing. Falling back to direct table creation for the current model.");

    var databaseCreator = orderDbContext.GetService<IRelationalDatabaseCreator>();
    await databaseCreator.CreateTablesAsync(cancellationToken);
}

static async Task<bool> OrderTableExistsAsync(OrderDbContext context, CancellationToken cancellationToken)
{
    var connection = context.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;

    if (shouldClose)
        await connection.OpenAsync(cancellationToken);

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            select exists (
                select 1
                from information_schema.tables
                where table_schema = current_schema()
                  and table_name = 'Orders'
            );
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true || (result is bool boolResult && boolResult);
    }
    finally
    {
        if (shouldClose)
            await connection.CloseAsync();
    }
}

try
{
    Log.Information("Starting ECommerce.API application");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u ASP.NET Core'a entegre et
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("Frontend"));
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRequestContexts();
    builder.Services.AddAuthServiceIntegration(builder.Configuration);
    builder.Services.AddMediaStorage(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddCustomerModule(builder.Configuration);
    builder.Services.AddInventoryModule(builder.Configuration);
    builder.Services.AddNotificationModule(builder.Configuration);
    builder.Services.AddOrderModule(builder.Configuration);
    builder.Services.AddPaymentModule(builder.Configuration);
    builder.Services.AddPricingModule(builder.Configuration);
    builder.Services.AddShipmentModule(builder.Configuration);
    builder.Services.AddStoreModule(builder.Configuration);
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Catalog.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Customer.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Inventory.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Notification.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Order.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Payment.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Pricing.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Shipment.Application.AssemblyReference).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(Store.Application.AssemblyReference).Assembly);
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    var allowedCorsOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];
    var frontendBaseUrl = builder.Configuration["Frontend:BaseUrl"]?.Trim().TrimEnd('/');
    if (!string.IsNullOrWhiteSpace(frontendBaseUrl) &&
        !allowedCorsOrigins.Contains(frontendBaseUrl, StringComparer.OrdinalIgnoreCase))
    {
        allowedCorsOrigins = [.. allowedCorsOrigins, frontendBaseUrl];
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            if (allowedCorsOrigins is { Length: > 0 })
            {
                policy.WithOrigins(allowedCorsOrigins)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });

    builder.Services.AddOpenApi();
    
    builder.Services.AddExceptionHandler<CustomerExceptionHandler>();
    builder.Services.AddExceptionHandler<InventoryExceptionHandler>();
    builder.Services.AddExceptionHandler<NotificationExceptionHandler>();
    builder.Services.AddExceptionHandler<OrderExceptionHandler>();
    builder.Services.AddExceptionHandler<PaymentExceptionHandler>();
    builder.Services.AddExceptionHandler<PricingExceptionHandler>();
    builder.Services.AddExceptionHandler<ShipmentExceptionHandler>();
    builder.Services.AddExceptionHandler<StoreExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await EnsureOrderDevelopmentSchemaAsync(app.Services);
    }

    await app.Services.SeedNotificationDefaultsAsync();

    // Serilog HTTP Request Logging
    app.UseEnrichedSerilogRequestLogging();

    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseRequestContext();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
