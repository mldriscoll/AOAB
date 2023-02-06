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
                AltName = AltName,
                ChapterName = ChapterName,
                OriginalFilenames = OriginalFilenames,
                Season = Season,
                SortOrder = SortOrder,
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
    }
}