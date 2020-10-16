using GPIOInterfaces;
using GPIOInterfaces.Contracts;
using GPIOModels.Configuration;
using GPIOProjects.Base;
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

        private static List<Task> _activeProjects = new List<Task>();

        public ProjectRunner(IOptions<List<ProjectConfig>> options, IServiceProvider serviceProvider)
        {
            _projects = options.Value;
            _serviceProvider = serviceProvider;
        }

        public bool IsProjectRunning()
        {
            return _activeProjects.FirstOrDefault() != null;
        }

        public RunnerResult CreateProjectInstance(string projectName)
        {
            if (IsProjectRunning())
                return RunnerResult.AnotherProjectRunning;

            var project = _projects.FirstOrDefault(project => project.Name.ToLower() == projectName.ToLower());

            if (project != null)
            {
                if (project.Runnable)
                {
                    var assemblyTypes = typeof(BaseProject).Assembly.GetTypes();
                    var projectType = assemblyTypes.FirstOrDefault(type => type.Name == project.Name);
                    if (projectType != null)
                    {
                        IProject projectService = (IProject)_serviceProvider.GetService(projectType);
                        var runningProject = Task.Run(() => projectService.RunProject());
                        runningProject.ContinueWith(finishedProject => RemoveCompletedProject(finishedProject));
                        _activeProjects.Add(runningProject);

                        return RunnerResult.Success;
                    }
                    else
                        return RunnerResult.ClassNotFound;
                }
                else
                    return RunnerResult.ProjectNotRunnable;
            }
            else
                return RunnerResult.UnknownProject;
        }

        private void RemoveCompletedProject(Task finishedProject)
        {
            _activeProjects.Remove(finishedProject);
        }
    }
}
