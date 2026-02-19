using System.Text.Json;

namespace DynQoS.Utility
{
    internal class Utilities
    {
        public static HashSet<string> DownloadExecutablesFromDiscord()
        {
            using var httpClient = new HttpClient();
            HashSet<string> discordExecutables = [];
            var discordJson = httpClient.GetStringAsync("https://discord.com/api/v10/applications/detectable").GetAwaiter().GetResult();
            using var discordJsonDocument = JsonDocument.Parse(discordJson);

            foreach (var app in discordJsonDocument.RootElement.EnumerateArray())
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
                                discordExecutables.Add(exeName);
                            }
                        }
                    }
                }
            }

            return discordExecutables;
        }

        public static HashSet<string> ReadExecutablesFromFile(string executablesPath)
        {
            HashSet<string> executablesList = [];

            if (File.Exists(executablesPath))
            {
                executablesList = File.ReadAllLines(executablesPath)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Select(name => Path.GetFileNameWithoutExtension(name))
                    .Select(name => name.Trim().ToLowerInvariant())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToHashSet();
            }

            return executablesList;
        }
    }
}