using AOABO.Config;

namespace AOABO.Chapters
{
    public class POVChapter : Chapter
    {
        public string POV { get; set; } = string.Empty;
        public CollectionChapter GetCollectionChapter()
        {
            return new CollectionChapter
            {
                ChapterName = ChapterName,
                OriginalFilenames = OriginalFilenames,
                Season = Season,
                SortOrder = SortOrder,
                SubFolder = Configuration.Options.Collection.POVChapterOrdering ? POV : string.Empty,
                Volume = Volume,
                Year = Year,
                Gallery = CollectionChapter.CollectionEnum.POVGallery,
                ProcessedInFanbooks = ProcessedInFanbooks,
                ProcessedInPartFive = ProcessedInPartFive,
                ProcessedInPartFour = ProcessedInPartFour,
                ProcessedInPartOne = ProcessedInPartOne,
                ProcessedInPartThree = ProcessedInPartThree,
                ProcessedInPartTwo = ProcessedInPartTwo,
                StartLine = StartLine,
                EndLine = EndLine,
            };
        }

        protected override string GetYearsSubFolder()
        {
            return base.GetYearsSubFolder();
        }
        public void ApplyPOVToTitle()
        {
            if (!string.IsNullOrWhiteSpace(POV))
            {
                ChapterName = $"{ChapterName} [{POV}]";
            }
        }
    }
}