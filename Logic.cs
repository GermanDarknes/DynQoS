using DynQoS.UI;
using DynQoS.Utility;
using System.Diagnostics;
using System.Management.Automation;

namespace DynQoS
{
    internal class Logic
    {
        private const string TrayIconHoverText = "Dynamic QoS Injector";
        private readonly TrayIconContext _trayContext = new(TrayIconHoverText);

        private HashSet<string> _discordExecutables = [];
        private HashSet<string> _includeExecutables = [];
        private HashSet<string> _excludeExecutables = [];
        private HashSet<string> _qosExecutables = [];
        private HashSet<string> _processedExecutables = [];

        private static string ExeDirectory => Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        private readonly string _discordExecutablesPath = Path.Combine(ExeDirectory, "dynqos_discord.txt");
        private readonly string _includeExecutablePath = Path.Combine(ExeDirectory, "dynqos_include.txt");
        private readonly string _excludeExecutablePath = Path.Combine(ExeDirectory, "dynqos_exclude.txt");

        private System.Timers.Timer _processCheckTimer;

        private PowerShell _powerShell;

        private const int ProcessCheckTimeInSeconds = 5;

        internal Logic()
        {
            _trayContext.ClearMenu();
            _trayContext.AddElement("Initializing...");
            _trayContext.UpdateMenu();

            _powerShell = new PowerShellHelper().GetPowerShell();

            ReloadExecutableLists();
            ClearQoSPolicies();

            _processCheckTimer = new System.Timers.Timer(ProcessCheckTimeInSeconds * 1000);
            _processCheckTimer.Elapsed += ProcessCheck;
            _processCheckTimer.AutoReset = true;
            _processCheckTimer.Enabled = true;
        }

        internal void CombineExecutableLists()
        {
            _qosExecutables.Clear();
            _qosExecutables.UnionWith(_discordExecutables);
            _qosExecutables.UnionWith(_includeExecutables);
            _qosExecutables.ExceptWith(_excludeExecutables);
            BuildContextMenu();
        }

        internal void ReloadExecutableLists(object? sender = null, EventArgs? e = null)
        {
            LoadDiscordExecutables();
            LoadIncludeExecutables();
            LoadExcludeExecutables();
        }

        internal void LoadDiscordExecutables(object? sender = null, EventArgs? e = null)
        {
            try
            {
                _discordExecutables = Utilities.DownloadExecutablesFromDiscord();
                File.WriteAllLines(_discordExecutablesPath, _discordExecutables);
            }
            catch (Exception)
            {
                _discordExecutables = Utilities.ReadExecutablesFromFile(_discordExecutablesPath);
            }
            CombineExecutableLists();
        }

        internal void LoadIncludeExecutables(object? sender = null, EventArgs? e = null)
        {
            _includeExecutables = Utilities.ReadExecutablesFromFile(_includeExecutablePath);
            CombineExecutableLists();
        }

        internal void LoadExcludeExecutables(object? sender = null, EventArgs? e = null)
        {
            _excludeExecutables = Utilities.ReadExecutablesFromFile(_excludeExecutablePath);
            RemoveExcludeQoSPolicies();
            CombineExecutableLists();
        }

        internal void NewNetQosPolicy(string qosProcess)
        {
            if (!_processedExecutables.Contains(qosProcess))
            {
                _powerShell.Commands.Clear();
                _powerShell.AddCommand("New-NetQosPolicy")
                  .AddParameter("Name", $"Dynamic QoS - {qosProcess}")
                  .AddParameter("AppPathNameMatchCondition", $"{qosProcess}.exe")
                  .AddParameter("DSCPAction", 1)
                  .AddParameter("IPProtocolMatchCondition", "Both");
                _powerShell.Invoke();

                _processedExecutables.Add(qosProcess);
            }
        }

        internal void RemoveNetQosPolicy(string qosProcess)
        {
            _powerShell.Commands.Clear();
            _powerShell.AddScript($"Remove-NetQosPolicy -Name \"Dynamic QoS - {qosProcess}\" -Confirm:$false -ErrorAction SilentlyContinue");
            _powerShell.Invoke();

            _processedExecutables.Remove(qosProcess);
        }

        internal void ClearQoSPolicies(object? sender = null, EventArgs? e = null)
        {
            _powerShell.Commands.Clear();
            _powerShell.AddScript("Remove-NetQosPolicy -Name \"Dynamic QoS - *\" -Confirm:$false -ErrorAction SilentlyContinue");
            _powerShell.Invoke();

            _processedExecutables.Clear();
            ProcessCheck();
        }

        internal void RemoveExcludeQoSPolicies()
        {
            foreach (var qosProcess in _excludeExecutables)
            {
                RemoveNetQosPolicy(qosProcess);
            }
        }

        internal void ProcessCheck(object? sender = null, EventArgs? e = null)
        {
            var runningQosProcesses = Process.GetProcesses()
                .Where(process => !string.IsNullOrEmpty(process.ProcessName))
                .Where(process => _qosExecutables.Contains(process.ProcessName.ToLowerInvariant()))
                .Where(process => !_processedExecutables.Contains(process.ProcessName.ToLowerInvariant()))
                .Select(process => process.ProcessName.ToLowerInvariant())
                .Distinct()
                .ToHashSet();

            foreach (var qosProcess in runningQosProcesses)
            {
                NewNetQosPolicy(qosProcess);
            }
        }


        internal void BuildContextMenu(object? sender = null, EventArgs? e = null)
        {
            _trayContext.ClearMenu();
            _trayContext.AddElement($"Reload All ({_qosExecutables.Count})", menuFunction: new EventHandler(ReloadExecutableLists));
            _trayContext.AddElement($"Reload Discord ({_discordExecutables.Count})", menuFunction: new EventHandler(LoadDiscordExecutables));
            _trayContext.AddElement($"Reload Include ({_includeExecutables.Count})", menuFunction: new EventHandler(LoadIncludeExecutables));
            _trayContext.AddElement($"Reload Exclude ({_excludeExecutables.Count})", menuFunction: new EventHandler(LoadExcludeExecutables));

            _trayContext.AddStrip();
            _trayContext.AddElement("Clear QoS Policies", menuFunction: new EventHandler(ClearQoSPolicies));

            _trayContext.AddStrip();
            _trayContext.AddElement("Close", menuFunction: new EventHandler(_trayContext.Close));

            _trayContext.UpdateMenu();
        }

        internal TrayIconContext GetContext()
        {
            return _trayContext;
        }
    }
}