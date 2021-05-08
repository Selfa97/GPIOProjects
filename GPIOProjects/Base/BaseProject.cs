using GPIOInterfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace GPIOProjects.Base
{
    public abstract class BaseProject : IProject
    {
        protected readonly GpioController _controller;
        protected readonly ILogger<IProject> _logger;

        public abstract List<int> Pins { get; }

        protected BaseProject(GpioController controller, ILogger<IProject> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public void RunProject()
        {
            _logger.LogTrace("------------------------------------------");

            try
            {
                Startup();

                Run();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running project.");
            }
        }

        protected abstract void Startup();

        protected abstract void Run();
    }
}