using Microsoft.PowerShell;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DynQoS.Utility
{
    internal class PowerShellHelper
    {
        private InitialSessionState ISS;
        private Runspace RS;
        private PowerShell PS;
        internal PowerShellHelper() {
            ISS = InitialSessionState.CreateDefault();
            ISS.ExecutionPolicy = ExecutionPolicy.Bypass;
            ISS.ImportPSModule(["NetQos"]);

            RS = RunspaceFactory.CreateRunspace(ISS);
            RS.Open();

            PS = System.Management.Automation.PowerShell.Create();
            PS.Runspace = RS;
        }

        public PowerShell GetPowerShell()
        {
            return PS;
        }
    }
}