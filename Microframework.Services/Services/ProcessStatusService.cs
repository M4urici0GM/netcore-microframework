using System.Runtime.InteropServices;
using Microframework.Core.Enums;
using Microframework.Core.Interfaces;

namespace Microframework.Services.Services
{
    public class ProcessStatusService : IProcessStatusService
    {
        public ProcessPlatform CurrentOsArchitecture { get; }

        public ProcessStatusService()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            CurrentOsArchitecture = isWindows ? ProcessPlatform.Windows : ProcessPlatform.Unix;
        }
    }
}