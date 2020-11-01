using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GPIOWebRunner.Middleware
{
    public class LogSeperatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogSeperatorMiddleware> _logger;

        public LogSeperatorMiddleware(RequestDelegate next, ILogger<LogSeperatorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("------------------------------------------");

            await _next(context);
        }
    }
}
