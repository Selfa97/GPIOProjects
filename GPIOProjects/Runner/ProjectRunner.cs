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
            return _activeProjects.Any();
        }

        public (RunnerResult, string) CreateProjectInstance(string projectName)
        {
            var project = _projects.FirstOrDefault(project => project.Name.ToLower() == projectName.ToLower());
            
            if (IsProjectRunning())
                return (RunnerResult.AnotherProjectRunning, project?.Name ?? projectName);

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

                        return (RunnerResult.Success, project.Name);
                    }
                    else
                        return (RunnerResult.ClassNotFound, project.Name);
                }
                else
                    return (RunnerResult.ProjectNotRunnable, project.Name);
            }
            else
                return (RunnerResult.UnknownProject, projectName);
        }

        private void RemoveCompletedProject(Task finishedProject)
        {
            _activeProjects.Remove(finishedProject);
        }
    }
}
