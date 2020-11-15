using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using System.Threading;

namespace GPIOProjects.LED
{
    public class LEDBreatheSoftware : BaseProject
    {
        private const int _pin = 18;
        private const int _frequency = 1024; 
        private readonly List<int> _pins;
        private SoftwarePwmChannel _pwmChannel;

        public override List<int> Pins => _pins;

        public LEDBreatheSoftware(GpioController controller, ILogger<LEDBreatheSoftware> logger) : base(controller, logger)
        {
            _pins = new List<int>() { _pin };
        }

        protected override void Run()
        {
            _logger.LogTrace("Running LEDBreatheSoftware.cs");

            using (_pwmChannel)
            {
                _pwmChannel.Start();

                var cancellationTokenSource = new CancellationTokenSource(new TimeSpan(hours: 0, minutes: 0, seconds: 10));
                do
                {
                    _logger.LogTrace("Increasing brightness...");
                    for (int i = 0; i <= _frequency; i++)
                    {
                        _pwmChannel.DutyCycle = (double)i / _frequency;
                        Thread.Sleep(2);
                    }

                    _logger.LogTrace("Decreasing brightness...");
                    for (int i = _frequency; i >= 0; i--)
                    {
                        _pwmChannel.DutyCycle = (double)i / _frequency;
                        Thread.Sleep(2);
                    }

                } while (!cancellationTokenSource.Token.IsCancellationRequested);

                _pwmChannel.Stop();
            }
        }

        protected override void Startup()
        {
            _logger.LogTrace("Opening pin {0} as an output.", _pin);

            if (!_controller.IsPinOpen(_pin))
                _controller.OpenPin(_pin, PinMode.Output);
            else if (_controller.GetPinMode(_pin) != PinMode.Output)
                _controller.SetPinMode(_pin, PinMode.Output);

            _logger.LogTrace("Setting up software PWM channel.");
            _pwmChannel = new SoftwarePwmChannel(_pin, _frequency, dutyCycle: 0.0, usePrecisionTimer: true);
        }
    }
}
