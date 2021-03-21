using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace GPIOWebRunner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : Controller
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
            IActionResult response = new UnauthorizedResult();
            ClaimsPrincipal currentUser = HttpContext.User;

            if (currentUser != null)
            {
                var username = currentUser.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (username == "Admin")
                {
                    _logger.LogInformation("Shutting down GPIOWebRunnner");

                    _applicationLifetime.StopApplication();

                    response = new OkResult();
                }
            }

            return response;
        }
    }
}
