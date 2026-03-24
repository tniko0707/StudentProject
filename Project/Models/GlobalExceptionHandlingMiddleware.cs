using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    /// <summary>
    /// middleware для глобальной обработки исключений
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. Method={Method}, Path={Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var error = new ProblemDetails
            {
                Status = statusCode,
                Detail = ex.Message
            };

            await httpContext.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex)
            => ex switch
            {
                ValidationException ve => StatusCodes.Status400BadRequest,
                ArgumentNullException knfe => StatusCodes.Status404NotFound,
                InvalidOperationException ioe => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}
