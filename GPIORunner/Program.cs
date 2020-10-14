using GPIOInterfaces;
using GPIOProjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using GPIOModels.Configuration;
using System.Text.Json;
using GPIOInterfaces.Contracts;
using GPIOProjects.Runner;
using Microsoft.Extensions.Options;
#if DEBUG
using System.Diagnostics;
using System.Threading;
#endif

namespace GPIORunner
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            do
            {
                Console.WriteLine("Waiting for Debugger to attach...");
                Thread.Sleep(5000);
            } while (!Debugger.IsAttached);
#endif

            var serviceProvider = ConfigureServices().BuildServiceProvider();
            var projects = serviceProvider.GetService<IOptions<List<ProjectConfig>>>().Value;

            foreach (ProjectConfig project in projects)
            {
                if (project.RunOnStart)
                {
                    RunnerResult result = serviceProvider.GetService<IProjectRunner>().CreateProjectInstance(project.Name);

                    HandelResult(project.Name, result);
                }
            }

            while (true)
            {
                Console.WriteLine("Enter the name of the project to run or 'Q' to quit.");
                Console.WriteLine($"Known projects: {JsonSerializer.Serialize(projects.Select(project => project.Name))}");

                string input = Console.ReadLine().ToLower();

                if (input == "q")
                    break;

                RunnerResult result = serviceProvider.GetService<IProjectRunner>().CreateProjectInstance(input);

                HandelResult(input, result);
            }
        }

        private static void HandelResult(string input, RunnerResult result)
        {
            switch (result)
            {
                case RunnerResult.ProjectNotRunnable:
                    Console.WriteLine($"{input} is not currently setup and cannot be run.");
                    break;

                case RunnerResult.UnknownProject:
                    Console.WriteLine($"{input} is not a recognised project.");
                    break;

                case RunnerResult.ClassNotFound:
                    Console.WriteLine($"Could not get type for: {input}");
                    break;

                default:
                    break;
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.Configure<List<ProjectConfig>>(options => configuration.GetSection("Projects").Bind(options));

            services.AddSingleton<GpioController>();
            services.AddSingleton<IProjectRunner, ProjectRunner>();

            services.AddTransient<LEDBlink>();
            services.AddTransient<LEDSwitch>();

            return services;
        }
    }
}
