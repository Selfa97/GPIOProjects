using GPIOInterfaces.Contracts;

namespace GPIOInterfaces
{
    public interface IProjectRunner
    {
        public RunnerResult CreateProjectInstance(string projectName);

        public bool IsProjectRunning();
    }
}
