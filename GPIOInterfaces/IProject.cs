using System.Collections.Generic;

namespace GPIOInterfaces
{
    public interface IProject
    {
        public List<int> Pins { get; }

        public void RunProject();
    }
}
