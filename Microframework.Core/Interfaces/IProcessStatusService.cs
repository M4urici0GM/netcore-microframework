using Microframework.Core.Enums;

namespace Microframework.Core.Interfaces
{
    public interface IProcessStatusService
    {
        public ProcessPlatform CurrentOsArchitecture { get;  }
        
    }
}