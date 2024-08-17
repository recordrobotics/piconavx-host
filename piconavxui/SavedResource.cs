using piconavx.ui.graphics.ui;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace piconavx.ui
{
    public static class SavedResource
    {
        public class Settings
        {
            private static Settings current;
            public static Settings Current => current;

            static Settings()
            {
                current = ReadSettings();
            }

            public static Settings Default => new Settings()
            {
                Theme = null,
                Address = "0.0.0.0",
                Port = 65432,
                Timeout = 1000,
                HighTimeout = 10000
            };

            [JsonPropertyName("address")]
            public string? Address { get; set; }
            [JsonPropertyName("port")]
            public int Port { get; set; }
            [JsonPropertyName("timeout")]
            public int Timeout { get; set; }
            [JsonPropertyName("high_timeout")]
            public int HighTimeout { get; set; }
            [JsonPropertyName("theme")]
            public string? Theme { get; set; }
        }

        private static string? savePath;

        private static bool isReadOnly;
        public static bool ReadOnly => isReadOnly;

        public static string? SavePath => savePath;

        static SavedResource()
        {
            try
            {
                // Test directory
                savePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "piconavx");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
            }
            catch
            {
                savePath = null;
                isReadOnly = true;
                return;
            }

            try
            {
                // Test file
                File.Open(Path.Join(savePath, "LOCK"), FileMode.Create, FileAccess.ReadWrite).Close();
                File.Delete(Path.Join(savePath, "LOCK"));

                isReadOnly = false;
            }
            catch
            {
                isReadOnly = true;
            }
        }

        public static Stream? GetResource(string name)
        {
            Stream? fs;

            if (!isReadOnly && savePath != null && File.Exists(Path.Join(savePath, name)))
            {
                try
                {
                    fs = File.Open(Path.Join(savePath, name), FileMode.Open, FileAccess.Read);
                }
                catch
                {
                    fs = null;
                }
            }
            else
            {
                fs = EmbeddedResource.GetResource(Path.Join("assets/config", name));
            }

            return fs;
        }

        public static Settings ReadSettings()
        {
            Stream? fs = GetResource("settings.json");

            if (fs != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<Settings>(fs, SourceGenerationContext.Default.Settings) ?? Settings.Default;
                }
                finally
                {
                    fs.Dispose();
                }
            }
            else
            {
                return Settings.Default;
            }
        }

        internal static Theme.ThemeFile ReadTheme(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return Theme.ThemeFile.Default;

            Stream? fs = GetResource(Path.Join("themes", name));

            if (fs != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<Theme.ThemeFile>(fs, SourceGenerationContext.Default.ThemeFile) ?? Theme.ThemeFile.Default;
                }
                finally
                {
                    fs.Dispose();
                }
            }
            else
            {
                return Theme.ThemeFile.Default;
            }
        }

        public static bool WriteSettings(Settings settings)
        {
            if (isReadOnly || savePath == null) return false;

            try
            {
                Stream fs = File.Open(Path.Join(savePath, "settings.json"), FileMode.Create, FileAccess.Write);
                try
                {
                    JsonSerializer.Serialize<Settings>(fs, settings, SourceGenerationContext.Default.Settings);
                    return true;
                }
                finally
                {
                    fs.Dispose();
                }
            }
            catch { return false; }
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SavedResource.Settings))]
    [JsonSerializable(typeof(Theme.ThemeFile))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
