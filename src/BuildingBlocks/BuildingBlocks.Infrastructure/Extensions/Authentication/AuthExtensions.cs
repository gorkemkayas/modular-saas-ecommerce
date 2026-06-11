using BuildingBlocks.Infrastructure.Extensions.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.Infrastructure.Extensions.Authentication
{
    public static partial class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = "tenant-api",

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),

                    NameClaimType = "sub",
                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is not ClaimsIdentity identity)
                            return Task.CompletedTask;

                        var existingRoleValues = identity.Claims
                            .Where(c =>
                                c.Type == ClaimTypes.Role ||
                                c.Type == "role" ||
                                c.Type == "roles" ||
                                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                            .Select(c => c.Value)
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        foreach (var roleValue in existingRoleValues)
                        {
                            if (!identity.HasClaim(ClaimTypes.Role, roleValue))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(context.Token))
                            return Task.CompletedTask;

                        if (context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicies.TenantAdmin, policy =>
                policy.RequireRole(AppRoles.TenantAdmin));

                options.AddPolicy(AppPolicies.SuperAdmin, policy =>
                    policy.RequireRole(AppRoles.SuperAdmin));

                options.AddPolicy(AppPolicies.TenantOrSuperAdmin, policy =>
                    policy.RequireRole(AppRoles.TenantAdmin, AppRoles.SuperAdmin));
            }
                );

            return services;
        }
    }
}
