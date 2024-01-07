using System.IO;
using System.Text.Json;

namespace OBB_WPF
{
    public class Configuration
    {
        public string SourceFolder { get; set; } = "D:\\JNC\\Raw";
        public string DefaultOutputFolder { get; set; } = null;
        public bool IncludeNormalChapters { get; set; } = true;
        public bool IncludeExtraChapters { get; set; } = true;
        public bool IncludeNonStoryChapters { get; set; } = true;
        public bool CombineMangaSplashPages { get; set; } = true;
        public bool UpdateChapterTitles { get; set; } = false;
        public int? MaxImageWidth { get; set; } = null;
        public int? MaxImageHeight { get; set; } = null;
        public int ResizedImageQuality { get; set; } = 90;

        public async Task Save()
        {
            await JSON.Save("Settings.json", this);
        }
    }

    public static class JSON
    {
        public async static Task Save<T>(string file, T obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

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
