using GPIOProjects.Base;
using System;
using System.Device.Gpio;

namespace GPIOProjects
{
    public class LEDSwitch : BaseProject
    {
        private const int _pin = 18;

        public LEDSwitch(GpioController controller) : base(controller) { }

        protected override void Run()
        {
            // Console.WriteLine("Running LEDSwitch.cs");

            if (_controller.Read(_pin) == PinValue.High)
            {
                // Console.WriteLine("Switching off LED...");
                _controller.Write(_pin, PinValue.Low);
            }
            else if (_controller.Read(_pin) == PinValue.Low)
            {
                // Console.WriteLine("Switching on LED...");
                _controller.Write(_pin, PinValue.High);
            }
            else
                throw new ApplicationException("Unknown Pin Value.");
        }

        protected override void Startup()
        {
            if (!_controller.IsPinOpen(_pin))
                _controller.OpenPin(_pin, PinMode.Output);
            else if (_controller.GetPinMode(_pin) != PinMode.Output)
                _controller.SetPinMode(_pin, PinMode.Output);
        }
    }
}
