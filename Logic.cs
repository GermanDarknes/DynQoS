using DynQoS.UI;
using DynQoS.Utility;
using System.Diagnostics;
using System.Management.Automation;

namespace DynQoS
{
    internal class Logic
    {
        private const string TrayIconHoverText = "Dynamic QoS Injector";
        private readonly TrayIconContext TrayContext= new(TrayIconHoverText);

        HashSet<string> DiscordExecutables = [];
        HashSet<string> IncludeExectuables = [];
        HashSet<string> ExcludeExecutables = [];
        HashSet<string> QoSExecutables = [];
        HashSet<string> ProcessedExecutables = [];

        private static string ExeDirectory => Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
        private readonly string DiscordExecutablesPath = Path.Combine(ExeDirectory, "dynqos_discord.txt");
        private readonly string IncludeExectuablePath = Path.Combine(ExeDirectory, "dynqos_include.txt");
        private readonly string ExcludeExectuablePath = Path.Combine(ExeDirectory, "dynqos_exclude.txt");

        private System.Timers.Timer ProcessCheckTimer;

        private PowerShell PS;

        private int ProcessCheckTimeInSeconds = 5;

        internal Logic()
        {
            TrayContext.ClearMenu();
            TrayContext.AddElement("Initializing...", null);
            TrayContext.UpdateMenu();

            PS = new PowerShellHelper().GetPowerShell();

            ReloadExectuablesLists();
            ClearQoSPolicies();

            ProcessCheckTimer = new System.Timers.Timer(ProcessCheckTimeInSeconds * 1000);
            ProcessCheckTimer.Elapsed += ProcessCheck;
            ProcessCheckTimer.AutoReset = true;
            ProcessCheckTimer.Enabled = true;
        }

        internal void CombineExectuableLists()
        {
            QoSExecutables = new HashSet<string>();
            QoSExecutables.UnionWith(DiscordExecutables);
            QoSExecutables.UnionWith(IncludeExectuables);
            QoSExecutables.ExceptWith(ExcludeExecutables);
            BuildContextMenu();
        }

        internal void ReloadExectuablesLists(object? Sender = null, EventArgs? E = null)
        {
            LoadDiscordExecutables();
            LoadIncludeExecutables();
            LoadExcludeExecutables();
        }

        internal void LoadDiscordExecutables(object? Sender = null, EventArgs? E = null)
        {
            try
            {
                DiscordExecutables = Utilities.DownloadExecutablesFromDiscord();
                File.WriteAllLines(DiscordExecutablesPath, DiscordExecutables);
            }
            catch (Exception)
            {
                DiscordExecutables = Utilities.ReadExectuablesFromFile(DiscordExecutablesPath);
            }
            CombineExectuableLists();
        }

        internal void LoadIncludeExecutables(object? Sender = null, EventArgs? E = null)
        {
            IncludeExectuables = Utilities.ReadExectuablesFromFile(IncludeExectuablePath);
            CombineExectuableLists();
        }

        internal void LoadExcludeExecutables(object? Sender = null, EventArgs? E = null)
        {
            ExcludeExecutables = Utilities.ReadExectuablesFromFile(ExcludeExectuablePath);
            RemoveExcludeQoSPolicies();
            CombineExectuableLists();
        }

        internal void NewNetQosPolicy(String QoSProcess)
        {
            if (!ProcessedExecutables.Contains(QoSProcess))
            {
                PS.Commands.Clear();
                PS.AddCommand("New-NetQosPolicy")
                  .AddParameter("Name", $"Dynamic QoS - {QoSProcess}")
                  .AddParameter("AppPathNameMatchCondition", $"{QoSProcess}.exe")
                  .AddParameter("DSCPAction", 1)
                  .AddParameter("IPProtocolMatchCondition", "Both");
                PS.Invoke();

                ProcessedExecutables.Add(QoSProcess);
            }
        }

        internal void RemoveNetQosPolicy(String QoSProcess)
        {
            PS.Commands.Clear();
            PS.AddScript($"Remove-NetQosPolicy -Name \"Dynamic QoS - {QoSProcess}\" -Confirm:$false -ErrorAction SilentlyContinue");
            PS.Invoke();

            ProcessedExecutables.Remove(QoSProcess);
        }

        internal void ClearQoSPolicies(object? Sender = null, EventArgs? E = null)
        {
            PS.Commands.Clear();
            PS.AddScript("Get-NetQosPolicy | Where-Object Name -like \"Dynamic QoS *\" | Remove-NetQosPolicy -Confirm:$false -ErrorAction SilentlyContinue");
            PS.Invoke();

            ProcessedExecutables.Clear();
            ProcessCheck();
        }

        internal void RemoveExcludeQoSPolicies()
        {
            foreach (var QoSProcess in ExcludeExecutables)
            {
                RemoveNetQosPolicy(QoSProcess);
            }
        }

        internal void ProcessCheck(object? Sender = null, EventArgs? E = null)
        {
            var RunningQosProcesses = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .Where(p => QoSExecutables.Contains(p.ProcessName.ToLowerInvariant()))
                .Where(p => !ProcessedExecutables.Contains(p.ProcessName.ToLowerInvariant()))
                .Select(p => p.ProcessName.ToLowerInvariant())
                .Distinct()
                .ToHashSet();

            foreach (var QoSProcess in RunningQosProcesses)
            {
                NewNetQosPolicy(QoSProcess);
            }
        }


        internal void BuildContextMenu(object? Sender = null, EventArgs? E = null)
        {
            TrayContext.ClearMenu();
            TrayContext.AddElement($"Reload All ({QoSExecutables.Count})", MenuFunction: new EventHandler(ReloadExectuablesLists));
            TrayContext.AddElement($"Reload Discord ({DiscordExecutables.Count})", MenuFunction: new EventHandler(LoadDiscordExecutables));
            TrayContext.AddElement($"Reload Include ({IncludeExectuables.Count})", MenuFunction: new EventHandler(LoadIncludeExecutables));
            TrayContext.AddElement($"Reload Exclude ({ExcludeExecutables.Count})", MenuFunction: new EventHandler(LoadExcludeExecutables));

            TrayContext.AddStrip();
            TrayContext.AddElement("Clear QoS Policies", MenuFunction: new EventHandler(ClearQoSPolicies));

            TrayContext.AddStrip();
            TrayContext.AddElement("Close", MenuFunction: new EventHandler(TrayContext.Close));

            TrayContext.UpdateMenu();
        }

        internal TrayIconContext GetContext()
        {
            return TrayContext;
        }
    }
}