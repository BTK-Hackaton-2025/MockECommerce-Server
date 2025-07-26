using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Exceptions;

namespace MockECommerce.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // normal flow
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/problem+json";

        ProblemDetails pd =
            ex switch
            {
                NotFoundException nf =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Kaynak Bulunamadı",
                        Detail = nf.Message,
                        Type = "https://httpstatuses.com/404"
                    },

                BusinessException be =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Title = "İş Kuralı İhlali",
                        Detail = be.Message,
                        Type = "https://httpstatuses.com/422",
                        Extensions = { { "errorCode", be.ErrorCode } }
                    },

                _ =>
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Beklenmeyen Hata",
                        Detail = ex.Message,
                        Type = "https://httpstatuses.com/500"
                    }
            };

        ctx.Response.StatusCode = pd.Status ?? 500;
        var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(pd, opts));
    }
}