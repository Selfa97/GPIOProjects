using GPIOInterfaces.Contracts;

namespace GPIOInterfaces
{
    public interface IRunner
    {
        public (RunnerResult, string) CreateProjectInstance(string projectName);

        public bool IsProjectRunning();
    }
}
