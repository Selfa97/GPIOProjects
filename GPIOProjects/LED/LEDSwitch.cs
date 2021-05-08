using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace GPIOProjects.LED
{
    public class LEDSwitch : BaseProject
    {
        private readonly int _pin;
        private readonly List<int> _pins;
        public override List<int> Pins => _pins;

        public LEDSwitch(GpioController controller, ILogger<LEDSwitch> logger) : base(controller, logger)
        {
            _pin = 18;
            _pins = new List<int> { _pin };
        }

        protected override void Run()
        {
            _logger.LogTrace("Running LEDSwitch.cs");

            if (_controller.Read(_pin) == PinValue.High)
            {
                _logger.LogTrace("Switching off LED...");
                _controller.Write(_pin, PinValue.Low);
            }
            else if (_controller.Read(_pin) == PinValue.Low)
            {
                _logger.LogTrace("Switching on LED...");
                _controller.Write(_pin, PinValue.High);
            }
        }

        protected override void Startup()
        {
            _logger.LogTrace("Opening pin {0} as an output.", _pin);

            if (!_controller.IsPinOpen(_pin))
                _controller.OpenPin(_pin, PinMode.Output);
            else if (_controller.GetPinMode(_pin) != PinMode.Output)
                _controller.SetPinMode(_pin, PinMode.Output);
        }
    }
}
