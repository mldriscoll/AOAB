using AOABO.Config;

namespace AOABO.Chapters
{
    public class POVChapter : MoveableChapter
    {
        protected override bool IsEarly()
        {
            return true;
        }

        public override CollectionChapter GetCollectionChapter()
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
            };
        }

        protected override string GetYearsSubFolder()
        {
            EarlySeason = Season;
            EarlyYear = Year;
            return base.GetYearsSubFolder();
        }
    }
}