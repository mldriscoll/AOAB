namespace AOABO.Config
{
    public class BonusChapter : MoveableChapter
    {
        protected override string GetFlatSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            return base.GetFlatSubFolder();
        }

        protected override string GetPartSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.BonusChapterSetting == BonusChapterSetting.EndOfBook)
                return $"{base.GetPartSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
            else
                return base.GetPartSubFolder();
        }

        protected override string GetVolumeSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.BonusChapterSetting == BonusChapterSetting.EndOfBook)
                return $"{base.GetVolumeSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
            else
                return base.GetVolumeSubFolder();
        }

        protected override string GetYearsSubFolder()
        {
            SortOrder = IsEarly() ? EarlySortOrder : LateSortOrder;

            if (Configuration.Options.BonusChapterSetting == BonusChapterSetting.EndOfBook)
                return $"{base.GetYearsSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
            else
                return base.GetYearsSubFolder();
        }

        protected override bool IsEarly()
        {
            return Configuration.Options.BonusChapterSetting == BonusChapterSetting.Chronological;
        }
    }

}