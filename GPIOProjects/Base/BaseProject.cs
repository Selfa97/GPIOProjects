using GPIOInterfaces;
using System.Device.Gpio;

namespace GPIOProjects.Base
{
    public abstract class BaseProject : IProject
    {
        protected GpioController _controller;

        protected BaseProject(GpioController controller)
        {
            _controller = controller;
        }

        public void RunProject()
        {
            Startup();

            Run();
        }

        protected abstract void Startup();

        protected abstract void Run();
    }
}