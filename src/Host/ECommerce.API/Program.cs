using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Infrastructure.Extensions.Authentication;
using BuildingBlocks.Infrastructure.Extensions.Middleware;
using ECommerce.API.ExceptionHandlers;
using ECommerce.API.Extensions;
using Notification.Infrastructure.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

Log.Logger = SerilogExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting ECommerce.API application");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u ASP.NET Core'a entegre et
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRequestContexts();
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
