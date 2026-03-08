using BuildingBlocks.Application.Abstractions.Authentication;
using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Infrastructure.Extensions.Authentication;
using BuildingBlocks.Infrastructure.Extensions.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add authentication extension
builder.Services.AddJwtAuthentication(builder.Configuration);
// Add user context extension
builder.Services.AddRequestContexts();

// For return enums as string, not int
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use the custom request context middleware to populate user and tenant information
app.UseRequestContext();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("API is running"));
app.MapGet("/secure", (HttpContext httpContext) =>
{
    var user = httpContext.User;

    var sub = user.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
    var tenantId = user.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value;
    var email = user.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
    var tokenType = user.Claims.FirstOrDefault(x => x.Type == "token_type")?.Value;

    return Results.Ok(new
    {
        sub,
        tenantId,
        email,
        tokenType
    });
}).RequireAuthorization();
app.MapGet("/claims", (HttpContext httpContext) =>
{
    return Results.Ok(
        httpContext.User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
}).RequireAuthorization();

app.MapGet("/me", (ICurrentUser currentUser, ITenantContext tenantContext) =>
{
    return Results.Ok(new
    {
        currentUser.IsAuthenticated,
        currentUser.UserId,
        currentUser.Email,
        currentUser.Name,
        currentUser.TokenType,
        tenantContext.TenantId,
        tenantContext.HasTenant
    });
}).RequireAuthorization();
app.Run();
