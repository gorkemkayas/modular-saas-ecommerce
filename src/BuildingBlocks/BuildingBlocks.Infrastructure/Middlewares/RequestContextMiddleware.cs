using BuildingBlocks.Infrastructure.Authentication;
using BuildingBlocks.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.Infrastructure.Middlewares
{
    public sealed class RequestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserRequestContext userRequestContext,
            TenantRequestContext tenantRequestContext)
        {
            var user = context.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                userRequestContext.IsAuthenticated = true;

                var sub = user.FindFirstValue("sub");
                if (Guid.TryParse(sub, out var userId))
                {
                    userRequestContext.UserId = userId;
                }

                userRequestContext.Email = user.FindFirstValue("email");
                userRequestContext.Name = user.FindFirstValue("name");
                userRequestContext.TokenType = user.FindFirstValue("token_type");

                var tenantIdValue = user.FindFirstValue("tenantId");
                if (int.TryParse(tenantIdValue, out var tenantId))
                {
                    tenantRequestContext.TenantId = tenantId;
                }
            }

            await _next(context);
        }
    }
    }
