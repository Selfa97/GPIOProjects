using GPIOInterfaces;
using GPIOProjects;
using GPIOProjects.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using GPIOModels.Configuration;
using System.Text.Json;
#if DEBUG
using System.Diagnostics;
using System.Threading;
#endif

namespace GPIORunner
{
    class Program
    {
        private static List<ProjectConfig> _projects = new List<ProjectConfig>();

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

            foreach (var project in _projects)
            {
                if (project.RunOnStart)
                {
                    if (project.Runnable)
                    {
                        var assemblyTypes = typeof(BaseProject).Assembly.GetTypes();
                        var projectType = assemblyTypes.FirstOrDefault(type => type.Name == project.Name);
                        if (projectType != null)
                        {
                            IProject projectService = (IProject)serviceProvider.GetService(projectType);
                            projectService.RunProject();
                        }
                        else
                            Console.WriteLine($"Could not get type for: {project}");
                    }
                    else
                        Console.WriteLine($"{project.Name} is not currently setup and cannot be run.");
                }
            }

            while (true)
            {
                Console.WriteLine("Enter the name of the project to run or 'Q' to quit.");
                Console.WriteLine($"Known projects: {JsonSerializer.Serialize(_projects.Select(project => project.Name))}");
                var input = Console.ReadLine().ToLower();

                if (input == "q")
                    break;

                var project = _projects.FirstOrDefault(project => project.Name.ToLower() == input);

                if (project != null)
                {
                    if (project.Runnable)
                    {
                        var assemblyTypes = typeof(BaseProject).Assembly.GetTypes();
                        var projectType = assemblyTypes.FirstOrDefault(type => type.Name == project.Name);
                        if (projectType != null)
                        {
                            IProject projectService = (IProject)serviceProvider.GetService(projectType);
                            projectService.RunProject();
                        }
                        else
                            Console.WriteLine($"Could not get type for: {project}");
                    }
                    else
                        Console.WriteLine($"{project.Name} is not currently setup and cannot be run.");
                }
                else
                    Console.WriteLine($"{input} is not a recognised project.");
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            configuration.GetSection("Projects").Bind(_projects);

            services.AddSingleton<GpioController>();
            services.AddSingleton(_projects);

            services.AddTransient<LEDBlink>();
            services.AddTransient<LEDSwitch>();

            return services;
        }
    }
}
