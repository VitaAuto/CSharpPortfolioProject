using System.IO;
using System.Text.Json;

namespace PlaywrightProject.Config
{
    public static class ConfigReader
    {
        public static CommonSettings LoadSettings(string path = "appsettings.json")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file '{path}' not found.");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<CommonSettings>(json)!;
        }
    }
}