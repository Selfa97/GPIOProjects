using GPIOInterfaces.Contracts;

namespace GPIOInterfaces
{
    public interface IProjectRunner
    {
        public (RunnerResult, string) CreateProjectInstance(string projectName);

        public bool IsProjectRunning();
    }
}
