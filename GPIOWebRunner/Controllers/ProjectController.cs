using GPIOInterfaces;
using GPIOInterfaces.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace GPIOWebRunner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly IRunner _runner; 

        public ProjectController(ILogger<ProjectController> logger, IRunner runner)
        {
            _logger = logger;
            _runner = runner;
        }

        [HttpGet("run/{projectName}")]
        public IActionResult RunProject(string projectName)
        {
            _logger.LogInformation("Starting RunProject() for '{0}'", projectName);

            JsonResult jsonResult;

            try
            {
                (RunnerResult result, string name) = _runner.CreateProjectInstance(projectName);
                jsonResult = HandleResult(name, result);
            }
            catch (Exception ex)
            {
                jsonResult = new JsonResult(ex.Message);
                jsonResult.StatusCode = StatusCodes.Status500InternalServerError;
            }

            _logger.LogInformation("Response: {0}", JsonSerializer.Serialize(jsonResult));
            return jsonResult;
        }

        private JsonResult HandleResult(string name, RunnerResult result)
        {
            JsonResult jsonResult;

            switch (result)
            {
                case RunnerResult.Success:
                    jsonResult = new JsonResult($"Running project {name}");
                    jsonResult.StatusCode = StatusCodes.Status200OK;
                    break;

                case RunnerResult.AnotherProjectRunning:
                    jsonResult = new JsonResult($"Cannot run {name}. Another project is already running.");
                    jsonResult.StatusCode = StatusCodes.Status202Accepted;
                    break;

                case RunnerResult.ProjectNotRunnable:
                    jsonResult = new JsonResult($"{name} is not currently setup and cannot be run.");
                    jsonResult.StatusCode = StatusCodes.Status409Conflict;
                    break;

                case RunnerResult.UnknownProject:
                    jsonResult = new JsonResult($"{name} is not a recognised project.");
                    jsonResult.StatusCode = StatusCodes.Status404NotFound;
                    break;

                case RunnerResult.ClassNotFound:
                    jsonResult = new JsonResult($"Could not get type for: {name}");
                    jsonResult.StatusCode = StatusCodes.Status500InternalServerError;
                    break;

                default:
                    jsonResult = new JsonResult($"Runner returned unrecognised result for project {name}");
                    jsonResult.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            return jsonResult;
        }
    }
}
