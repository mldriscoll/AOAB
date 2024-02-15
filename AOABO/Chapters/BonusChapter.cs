using AOABO.Config;

namespace AOABO.Chapters
{
    public class BonusChapter : MoveableChapter
    {
        public OCRSettings? OCR { get; set; }
        public override CollectionChapter GetCollectionChapter()
        {
            return new CollectionChapter
            {
                ChapterName = ChapterName,
                OverrideName = OverrideName,
                OriginalFilenames = OriginalFilenames,
                Season = Season,
                SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder,
                Volume = Volume,
                Year = Year,
                SubFolder = Configuration.Options.Collection.POVChapterOrdering ? POV : string.Empty,
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

        protected override string GetFlatSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            return base.GetFlatSubFolder();
        }

        protected override string GetPartSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.Chapter.BonusChapter == BonusChapterSetting.EndOfBook)
                return $"{base.GetPartSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
            else
                return base.GetPartSubFolder();
        }

        protected override string GetVolumeSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.Chapter.BonusChapter == BonusChapterSetting.EndOfBook)
                return $"{base.GetVolumeSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
            else
                return base.GetVolumeSubFolder();
        }

        protected override string GetYearsSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.Chapter.BonusChapter == BonusChapterSetting.EndOfBook)
                return $"{base.GetYearsSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
            else
                return base.GetYearsSubFolder();
        }

        protected override bool IsEarly()
        {
            return Configuration.Options.Chapter.BonusChapter == BonusChapterSetting.Chronological;
        }
    }

}