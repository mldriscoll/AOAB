namespace AOABO.Config
{
    public class Gallery : Chapter
    {
        public List<string> SplashImages { get; set; }
        public List<string> ChapterImages { get; set; }
        public string EndOfBookSortOrder { get; set; }
        public string StartOfBookSortOrder { get; set; }

        private bool startOfBook = false;

        public Gallery GetChapter(bool startOfBook, bool splashImages, bool chapterImages)
        {
            if (!splashImages && !chapterImages)
            {
                return null;
            }

            //var chapter = Copy();

            if (startOfBook)
            {
                SortOrder = StartOfBookSortOrder;
                startOfBook = true;
            }
            else
            {
                startOfBook = false;
                SortOrder = EndOfBookSortOrder;
                //ChapterName = $"{ChapterName} Gallery";
                //AltName = $"{AltName} Gallery";
            }

            if (splashImages)
            {
                OriginalFilenames.AddRange(SplashImages);
            }

            if (chapterImages)
            {
                OriginalFilenames.AddRange(ChapterImages);
            }
            return this;
        }

        protected override string GetPartSubFolder()
        {
            return Volume switch
            {
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\01-Inserts",
                _ => throw new Exception($"GetPartSubFolder (Gallery) - {ChapterName}"),
            };
        }

        protected override string GetVolumeSubFolder()
        {
            return Volume switch
            {
                "0403"  => startOfBook ? $"{Configuration.FolderNames["PartFour"]}\\0403-Volume 3" : $"{Configuration.FolderNames["PartFour"]}\\0403-Volume 3\\x-Extras",
                _ => throw new Exception($"GetVolumeSubFolder (Gallery) - {ChapterName}")
            };
        }
    }
}