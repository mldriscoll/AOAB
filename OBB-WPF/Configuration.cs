namespace OBB_WPF
{
    public class Configuration
    {
        public string SourceFolder { get; set; } = null;
        public string DefaultOutputFolder { get; set; } = null;
        public bool IncludeNormalChapters { get; set; } = true;
        public bool IncludeExtraChapters { get; set; } = true;
        public bool IncludeNonStoryChapters { get; set; } = true;
        public bool CombineMangaSplashPages { get; set; } = true;
        public bool UpdateChapterTitles { get; set; } = false;
        public int? MaxImageWidth { get; set; } = null;
        public int? MaxImageHeight { get; set; } = null;
        public int ResizedImageQuality { get; set; } = 90;
        public string? EditorName { get; set; }

        public async Task Save()
        {
            await JSON.Save("Settings.json", this);
        }
    }
}
