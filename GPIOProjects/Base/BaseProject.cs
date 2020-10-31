using GPIOInterfaces;
using Microsoft.Extensions.Logging;
using System.Device.Gpio;

namespace GPIOProjects.Base
{
    public abstract class BaseProject : IProject
    {
        protected readonly GpioController _controller;

        protected readonly ILogger<IProject> _logger;

        protected BaseProject(GpioController controller, ILogger<IProject> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public void RunProject()
        {
            _logger.LogTrace("------------------------------------------");

            Startup();

            Run();
        }

        protected abstract void Startup();

        protected abstract void Run();
    }
}