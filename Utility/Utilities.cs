using System.Text.Json;

namespace DynQoS.Utility
{
    internal class Utilities
    {
        public static HashSet<string> DownloadExecutablesFromDiscord()
        {
            using var HttpClient = new HttpClient();
            HashSet<string> DiscordExecutables = [];
            var DiscordJson = HttpClient.GetStringAsync("https://discord.com/api/v10/applications/detectable").GetAwaiter().GetResult();
            using var DiscordJsonDocument = JsonDocument.Parse(DiscordJson);

            foreach (var app in DiscordJsonDocument.RootElement.EnumerateArray())
            {
                if (app.TryGetProperty("executables", out var executables) && executables.ValueKind == JsonValueKind.Array)
                {
                    foreach (var exe in executables.EnumerateArray())
                    {
                        if (exe.TryGetProperty("os", out var os) && os.GetString() == "win32" &&
                            exe.TryGetProperty("is_launcher", out var is_launcher) && !is_launcher.GetBoolean() &&
                            exe.TryGetProperty("name", out var name))
                        {
                            var exeName = Path.GetFileNameWithoutExtension(name.GetString() ?? "").ToLowerInvariant();
                            if (!string.IsNullOrEmpty(exeName))
                            {
                                DiscordExecutables.Add(exeName);
                            }
                        }
                    }
                }
            }

            return DiscordExecutables;
        }

        public static HashSet<string> ReadExectuablesFromFile(String ExecutablesPath)
        {
            HashSet<string> ExecutablesList = [];

            if (File.Exists(ExecutablesPath))
            {
                ExecutablesList = File.ReadAllLines(ExecutablesPath)
                    .Where(Name => !string.IsNullOrEmpty(Name))
                    .Select(Name => Path.GetFileNameWithoutExtension(Name))
                    .Select(Name => Name.Trim().ToLowerInvariant())
                    .Where(Name => !string.IsNullOrEmpty(Name))
                    .Distinct()
                    .ToHashSet();
            }

            return ExecutablesList;
        }
    }
}