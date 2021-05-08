using GPIOInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using GPIOModels.Configuration;
using System.Text.Json;
using GPIOInterfaces.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog;
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
                    (RunnerResult result, string name) = serviceProvider.GetService<IRunner>().CreateProjectInstance(project.Name);

                    HandleResult(name, result);
                }
            }

            while (true)
            {
                Console.WriteLine("Enter the name of the project to run or 'Q' to quit.");
                Console.WriteLine($"Known projects: {JsonSerializer.Serialize(projects.Select(project => project.Name))}");

                string input = Console.ReadLine();

                if (input.ToLower() == "q")
                    break;

                (RunnerResult result, string name) = serviceProvider.GetService<IRunner>().CreateProjectInstance(input);

                HandleResult(name, result);
            }
        }

        private static void HandleResult(string name, RunnerResult result)
        {
            switch (result)
            {
                case RunnerResult.Success:
                    Console.WriteLine($"Running project {name}" + Environment.NewLine);
                    break;

                case RunnerResult.AnotherProjectRunning:
                    Console.WriteLine($"Cannot run {name}. Another project is already running." + Environment.NewLine);
                    break;

                case RunnerResult.ProjectNotRunnable:
                    Console.WriteLine($"{name} is not currently setup and cannot be run." + Environment.NewLine);
                    break;

                case RunnerResult.UnknownProject:
                    Console.WriteLine($"{name} is not a recognised project." + Environment.NewLine);
                    break;

                case RunnerResult.ClassNotFound:
                    Console.WriteLine($"Could not get type for: {name}" + Environment.NewLine);
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

            services.AddSharedConfiguration(configuration);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddNLog(configuration);
            });

            DisableConsoleLogging();

            // Register event handler for when configuration is reloaded
            LogManager.ConfigurationReloaded += (sender, e) =>
            {
                DisableConsoleLogging();
            };

            services.AddSharedEssentials();
            services.AddProjects();

            return services;
        }

        private static void DisableConsoleLogging()
        {
            var configuration = LogManager.Configuration;
            configuration.RemoveTarget("Console");
        }
    }
}
