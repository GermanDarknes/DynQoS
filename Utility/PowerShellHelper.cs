using Microsoft.PowerShell;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DynQoS.Utility
{
    internal class PowerShellHelper
    {
        private InitialSessionState _initialSessionState;
        private Runspace _runspace;
        private PowerShell _powerShell;
        internal PowerShellHelper() {
            _initialSessionState = InitialSessionState.CreateDefault();
            _initialSessionState.ExecutionPolicy = ExecutionPolicy.Bypass;
            _initialSessionState.ImportPSModule(["NetQos"]);

            _runspace = RunspaceFactory.CreateRunspace(_initialSessionState);
            _runspace.Open();

            _powerShell = System.Management.Automation.PowerShell.Create();
            _powerShell.Runspace = _runspace;
        }

        public PowerShell GetPowerShell()
        {
            return _powerShell;
        }
    }
}