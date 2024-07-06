using System.Reflection;

namespace piconavx.ui
{
    public static class EmbeddedResource
    {
        private static Assembly assembly;
        private static readonly string[] names;
        public static string[] Names { get { return names; } }
        public static string Namespace => typeof(EmbeddedResource).Namespace ?? string.Empty;

        static EmbeddedResource()
        {
            assembly = Assembly.GetExecutingAssembly();
            names = assembly.GetManifestResourceNames();
        }

        public static string GetName(string relativePath)
        {
            return Namespace + "." + relativePath.Replace('/', '.').Replace('\\', '.');
        }

        public static Stream? GetResourceWithName(string name)
        {
            return assembly.GetManifestResourceStream(name);
        }

        public static Stream? GetResource(string relativePath)
        {
            return GetResourceWithName(GetName(relativePath));
        }

        public static string? ReadAllText(string relativePath)
        {
            Stream? stream = GetResource(relativePath);
            if (stream == null)
                return null;

            using StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            stream.Dispose();

            return text;
        }

        public static byte[]? ReadAllBytes(string relativePath)
        {
            Stream? stream = GetResource(relativePath);
            if (stream == null)
                return null;

            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            stream.Dispose();

            return bytes;
        }
    }
}
