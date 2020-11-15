using GPIOInterfaces;
using GPIOModels.Configuration;
using GPIOProjects.LED;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Device.Gpio;

namespace GPIORunner
{
    public static class SharedDependencyInjector
    {
        public static void AddSharedConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<List<ProjectConfig>>(section => configuration.GetSection("Projects").Bind(section));
        }

        public static void AddSharedEssentials(this IServiceCollection services)
        {
            services.AddSingleton<GpioController>();
            services.AddSingleton<IRunner, Runner>();
        }

        public static void AddProjects(this IServiceCollection services)
        {
            // LED Projects
            services.AddTransient<LEDBlink>();
            services.AddTransient<LEDBreathe>();
            services.AddTransient<LEDBreatheSoftware>();
            services.AddTransient<LEDSwitch>();
            services.AddTransient<RGBRandom>();
        }
    }
}
