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
                Test = 69
            };

            [JsonPropertyName("test")]
            public int Test { get; set; }
        }

        private static string? savePath;

        private static bool isReadOnly;
        public static bool ReadOnly => isReadOnly;

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
                File.OpenWrite(Path.Join(savePath, "LOCK")).Close();
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
                    fs = File.OpenRead(Path.Join(savePath, name));
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

        public static bool WriteSettings(Settings settings)
        {
            if (isReadOnly || savePath == null) return false;

            try
            {
                Stream fs = File.OpenWrite(Path.Join(savePath, "settings.json"));
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
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
