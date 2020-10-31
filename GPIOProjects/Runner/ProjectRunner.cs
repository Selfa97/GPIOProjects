using GPIOInterfaces;
using GPIOInterfaces.Contracts;
using GPIOModels.Configuration;
using GPIOProjects.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPIOProjects.Runner
{
    public class ProjectRunner : IProjectRunner
    {
        private readonly List<ProjectConfig> _projects;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProjectRunner> _logger;

        private static List<Task> _activeProjects = new List<Task>();

        public ProjectRunner(IOptions<List<ProjectConfig>> options, 
                             IServiceProvider serviceProvider,
                             ILogger<ProjectRunner> logger)
        {
            _projects = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public bool IsProjectRunning()
        {
            return _activeProjects.Any();
        }

        public (RunnerResult, string) CreateProjectInstance(string projectName)
        {
            var project = _projects.FirstOrDefault(project => project.Name.ToLower() == projectName.ToLower());
            projectName = project?.Name ?? projectName;

            try
            {
                if (IsProjectRunning())
                {
                    _logger.LogInformation("Another project is already running. Cannot start {0}.", projectName);
                    return (RunnerResult.AnotherProjectRunning, projectName);
                }

                if (project != null)
                {
                    if (project.Runnable)
                    {
                        var assemblyTypes = typeof(BaseProject).Assembly.GetTypes();
                        var projectType = assemblyTypes.FirstOrDefault(type => type.Name == projectName);
                        if (projectType != null)
                        {
                            IProject projectService = (IProject)_serviceProvider.GetService(projectType);
                            var runningProject = Task.Run(() => projectService.RunProject());
                            runningProject.ContinueWith(finishedProject => _activeProjects.Remove(finishedProject));
                            _activeProjects.Add(runningProject);

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
