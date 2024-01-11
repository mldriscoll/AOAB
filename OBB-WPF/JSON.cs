using System.IO;
using System.Text.Json;

namespace OBB_WPF
{
    public static class JSON
    {
        public async static Task Save<T>(string file, T obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            if (File.Exists(file)) File.Delete(file);

            using (var stream = File.OpenWrite(file))
            {
                await JsonSerializer.SerializeAsync<T>(stream, obj, options: options);
            }
        }

        public async static Task<T> Load<T>(string file)
        {
            if (!File.Exists(file)) return default(T);

            using (var stream = File.OpenRead(file))
            {
                return await JsonSerializer.DeserializeAsync<T>(stream);
            }
        }
    }
}
