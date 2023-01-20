namespace AOABO.Config
{
    public class Gallery : Chapter
    {
        public List<string> SplashImages { get; set; }
        public List<string> ChapterImages { get; set; }
        public string EndOfBookSortOrder { get; set; }

        public Chapter GetChapter(bool startOfBook, bool splashImages, bool chapterImages)
        {
            if (!splashImages && !chapterImages)
            {
                return null;
            }

            var chapter = Copy();

            if (!startOfBook)
            {
                chapter.SortOrder = EndOfBookSortOrder;
                chapter.ChapterName = $"{ChapterName} Gallery";
                chapter.AltName = $"{AltName} Gallery";
            }

            if (splashImages)
            {
                chapter.OriginalFilenames.AddRange(SplashImages);
            }

            if (chapterImages)
            {
                chapter.OriginalFilenames.AddRange(ChapterImages);
            }
            return chapter;
        }
    }
}