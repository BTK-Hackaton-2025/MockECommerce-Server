using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MockECommerce.WebAPI.Filters;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Endpoint'in Authorize attribute'u var mı kontrol et
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .Any() ?? false;

        // AllowAnonymous attribute'u var mı kontrol et
        var hasAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AllowAnonymousAttribute>()
            .Any() ?? false;

        // Eğer [Authorize] var ve [AllowAnonymous] yoksa security requirement ekle
        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }
                    ] = new string[] { }
                }
            };

            // Eğer external API endpoint'i ise API Key de ekle
            if (context.ApiDescription.RelativePath?.Contains("external") == true)
            {
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        }
                    ] = new string[] { }
                });
            }
        }
    }
}
