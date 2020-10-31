using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;

namespace GPIOProjects.LED
{
    public class LEDBlink : BaseProject
    {
        private readonly int _pin;
        private readonly List<int> _pins;
        private const int _sleepTime = 200;

        public override List<int> Pins => _pins;

        public LEDBlink(GpioController controller, ILogger<LEDBlink> logger) : base(controller, logger) 
        {
            _pin = 18;
            _pins = new List<int> { _pin };
        }

        protected override void Run()
        {
            _logger.LogTrace("Running LEDBlink.cs");

            var cancellationTokenSource = new CancellationTokenSource(new TimeSpan(hours: 0, minutes: 0, seconds: 10));
            do
            {
                _logger.LogTrace("LED On for {0}ms.", _sleepTime);
                _controller.Write(_pin, PinValue.High);

                Thread.Sleep(_sleepTime);

                _logger.LogTrace("LED Off for {0}ms.", _sleepTime);
                _controller.Write(_pin, PinValue.Low);

                Thread.Sleep(_sleepTime);
            } while (!cancellationTokenSource.Token.IsCancellationRequested); 
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
