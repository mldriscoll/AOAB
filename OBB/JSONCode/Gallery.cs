namespace OBB.JSONCode
{
    public class Gallery : Chapter
    {
        public List<string> SplashImages { get; set; } = new List<string>();
        public List<string> ChapterImages { get; set; } = new List<string>();
        public string? LateSubFolder { get; set; }

        public Chapter SetFiles(bool includeSplashImages, bool includeChapterImages, bool early)
        {
            var chapter = new Chapter
            {
                ChapterName = "Gallery",
                SubFolder = early ? SubFolder : string.IsNullOrWhiteSpace(LateSubFolder) ? SubFolder : LateSubFolder,
                SortOrder = early ? string.Empty : "99"
            };

            if (includeSplashImages)
            {
                chapter.OriginalFilenames.AddRange(SplashImages);
            }

            if (includeChapterImages)
            {
                chapter.OriginalFilenames.AddRange(ChapterImages);
            }

            return chapter;
        }
    }
}
