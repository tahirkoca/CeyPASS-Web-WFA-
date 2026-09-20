using CeyPASS.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CeyPASS.Api.Infrastructure
{
    /// <summary>Yakalanmamış hataları <see cref="ApiResult"/> JSON olarak 500 döner.</summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Global Hata Yakalandı: {Message}", exception.Message);

            var apiResult = ApiResult.Failure($"Sunucu hatası: {exception.Message}", (int)HttpStatusCode.InternalServerError);

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(apiResult, cancellationToken);

            return true;
        }
    }
}
