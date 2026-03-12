using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Infrastructure.Extensions.Authentication;
using BuildingBlocks.Infrastructure.Extensions.Middleware;
using ECommerce.API.ExceptionHandlers;
using ECommerce.API.Extensions;
using Serilog;
using System.Text.Json.Serialization;

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
    builder.Services.AddStoreModule(builder.Configuration);
    builder.Services.AddMediatR(cfg =>
    {
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
    builder.Services.AddExceptionHandler<StoreExceptionHandler>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Serilog HTTP Request Logging
    app.UseEnrichedSerilogRequestLogging();

    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
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
