using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading;

namespace GPIOProjects.LED
{
    public class LEDBreathe : BaseProject
    {
        // ---------------------------------------------------- DEFAULT PIN ----------------------------------------------------
        // I have setup my RPi's boot configuration to enable a hardware PWM channel on pin 12 at start up.
        // If PWM pin's mode is overwritten by a project, hardware PWM will no longer work and the RPi will need to be rebooted.
        // See: https://github.com/dotnet/iot/blob/master/Documentation/raspi-pwm.md
        // ---------------------------------------------------------------------------------------------------------------------

        private const int _frequency = 1024;
        private readonly List<int> _pins;
        private PwmChannel _pwmChannel;

        public override List<int> Pins => _pins;

        public LEDBreathe(GpioController controller, ILogger<LEDBreathe> logger) : base(controller, logger) 
        {
            _pins = new List<int> { 12 };
        }

        protected override void Run()
        {
            _logger.LogTrace("Running LEDBreathe.cs");

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
            _logger.LogTrace("Setting up default PWM channel.");
            _pwmChannel = PwmChannel.Create(chip: 0, channel: 0, _frequency, dutyCyclePercentage: 0.0);
        }
    }
}
