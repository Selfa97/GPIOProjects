using GPIOInterfaces;
using GPIOInterfaces.Contracts;
using GPIOModels.Configuration;
using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GPIORunner
{
    public class Runner : IRunner
    {
        private readonly List<ProjectConfig> _projects;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Runner> _logger;

        private readonly object _activePinsLock = new object();
        private static List<int> _activePins = new List<int>();

        public Runner(IOptions<List<ProjectConfig>> options, 
                             IServiceProvider serviceProvider,
                             ILogger<Runner> logger)
        {
            _projects = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public bool IsProjectRunning()
        {
            return _activePins.Any();
        }

        public (RunnerResult, string) CreateProjectInstance(string projectName)
        {
            var project = _projects.FirstOrDefault(project => project.Name.ToLower() == projectName.ToLower());
            projectName = project?.Name ?? projectName;

            try
            {
                if (project != null)
                {
                    if (project.Runnable)
                    {
                        var assemblyTypes = typeof(BaseProject).Assembly.GetTypes();
                        var projectType = assemblyTypes.FirstOrDefault(type => type.Name == projectName);
                        if (projectType != null)
                        {
                            IProject projectService = (IProject)_serviceProvider.GetService(projectType);

                            if (IsProjectRunning() && _activePins.Intersect(projectService.Pins).Any())
                            {
                                _logger.LogInformation("Another running project is currently using the required pins. Cannot start {0}.", projectName);
                                return (RunnerResult.AnotherProjectRunning, projectName);
                            }

                            _logger.LogTrace("Adding pins {0} to active pins.", JsonSerializer.Serialize(projectService.Pins));
                            lock (_activePinsLock)
                            {
                                _activePins.AddRange(projectService.Pins);
                            }

                            Task.Run(() => projectService.RunProject())
                                           .ContinueWith(task => {
                                               lock (_activePinsLock)
                                               {
                                                   _activePins = _activePins.Except(projectService.Pins).ToList();
                                               }
                                               _logger.LogTrace("Removing pins {0} from active pins.", JsonSerializer.Serialize(projectService.Pins));
                                           });

                            _logger.LogInformation("Successfully started project {0}.", projectName);
                            return (RunnerResult.Success, projectName);
                        }
                        else
                        {
                            _logger.LogInformation("Could not find project class for {0}.", projectName);
                            return (RunnerResult.ClassNotFound, projectName);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Project {0} is not setup/runnable.", projectName);
                        return (RunnerResult.ProjectNotRunnable, projectName);
                    }
                }
                else
                {
                    _logger.LogInformation("{0} is not a recognised project.", projectName);
                    return (RunnerResult.UnknownProject, projectName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create instance of project {0}.", projectName);
                throw ex;
            }
        }
    }
}
