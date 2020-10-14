using GPIOInterfaces.Contracts;
using System.Threading.Tasks;

namespace GPIOInterfaces
{
    public interface IProjectRunner
    {
        public RunnerResult CreateProjectInstance(string projectName);

        public Task GetRunningProject();
    }
}
