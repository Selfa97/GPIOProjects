using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using System.Threading;

namespace GPIOProjects.LED
{
    public class RGBRandom : BaseProject
    {
        private const int _redPin = 25;
        private const int _greenPin = 24;
        private const int _bluePin = 23;
        private const int _multiplier = 4;
        private const int _frequency = _multiplier * 255;

        private SoftwarePwmChannel _redPwmChannel;
        private SoftwarePwmChannel _greenPwmChannel;
        private SoftwarePwmChannel _bluePwmChannel;

        private readonly List<int> _pins;

        public override List<int> Pins => _pins;

        public RGBRandom(GpioController controller, ILogger<RGBRandom> logger) : base(controller, logger)
        {
            _pins = new List<int>() { _redPin, _greenPin, _bluePin };
        }

        protected override void Run()
        {
            _logger.LogTrace("Running RGBRandom.cs");

            var random = new Random(Guid.NewGuid().GetHashCode());

            _redPwmChannel.Start();
            _greenPwmChannel.Start();
            _bluePwmChannel.Start();

            _redPwmChannel.DutyCycle = 0.0;
            Thread.Sleep(500);
            _redPwmChannel.DutyCycle = 1.0;

            _greenPwmChannel.DutyCycle = 0.0;
            Thread.Sleep(500);
            _greenPwmChannel.DutyCycle = 1.0;

            _bluePwmChannel.DutyCycle = 0.0;
            Thread.Sleep(500);
            _bluePwmChannel.DutyCycle = 1.0;

            var cancellationTokenSource = new CancellationTokenSource(new TimeSpan(hours: 0, minutes: 0, seconds: 10));
            do
            {
                int red = random.Next(0, 255);
                int green = random.Next(0, 255);
                int blue = random.Next(0, 255);

                _logger.LogTrace("Changing the colour to R:{0} G:{1} B:{2}.", red, green, blue);
                _redPwmChannel.DutyCycle = (double)((255 - red) * _multiplier) / _frequency;
                _greenPwmChannel.DutyCycle = (double)((255 - green) * _multiplier) / _frequency;
                _bluePwmChannel.DutyCycle = (double)((255 - blue) * _multiplier) / _frequency;

                Thread.Sleep(500);
            } while (!cancellationTokenSource.IsCancellationRequested);

            _redPwmChannel.Stop();
            _greenPwmChannel.Stop();
            _bluePwmChannel.Stop();

            _redPwmChannel.Dispose();
            _greenPwmChannel.Dispose();
            _bluePwmChannel.Dispose();
        }

        protected override void Startup()
        {
            foreach (int pin in _pins)
            {
                _logger.LogTrace("Opening pin {0} as an output.", pin);

                if (!_controller.IsPinOpen(pin))
                    _controller.OpenPin(pin, PinMode.Output);
                else if (_controller.GetPinMode(pin) != PinMode.Output)
                    _controller.SetPinMode(pin, PinMode.Output);
            }

            _logger.LogTrace("Setting up software PWMs.");
            _redPwmChannel = new SoftwarePwmChannel(_redPin, _frequency, dutyCycle: 1.0, usePrecisionTimer: true);
            _greenPwmChannel = new SoftwarePwmChannel(_greenPin, _frequency, dutyCycle: 1.0, usePrecisionTimer: true);
            _bluePwmChannel = new SoftwarePwmChannel(_bluePin, _frequency, dutyCycle: 1.0, usePrecisionTimer: true);
        }
    }
}
