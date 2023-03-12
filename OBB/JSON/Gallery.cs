namespace OBB.JSON
{
    public class Gallery : Chapter
    {
        public List<string> SplashImages { get; set; } = new List<string>();
        public List<string> ChapterImages { get; set; } = new List<string>();

        public Chapter SetFiles(bool includeSplashImages, bool includeChapterImages, bool early)
        {
            var chapter = new Chapter
            {
                ChapterName = "Gallery",
                SubFolder = SubFolder,
                SortOrder = early ? String.Empty : "99"
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
