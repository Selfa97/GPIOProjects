using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GPIOWebRunner.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHostApplicationLifetime applicationLifetime, ILogger<AdminController> logger)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        [HttpGet("shutdown")]
        public IActionResult Shutdown()
        {
            _logger.LogInformation("Shutting down GPIOWebRunnner");

            _applicationLifetime.StopApplication();

            return new OkResult();
        }
    }
}
