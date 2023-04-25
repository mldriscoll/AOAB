namespace AOABO.Chapters
{
    public abstract class MoveableChapter : Chapter
    {
        public string OverrideName { get; set; } = string.Empty;
        public string POV { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public void ApplyPOVToTitle()
        {
            if (!string.IsNullOrWhiteSpace(POV))
            {
                ChapterName = $"{ChapterName} [{POV}]";
            }
        }
        public string EarlySortOrder { get; set; } = string.Empty;
        public string LateSortOrder { get; set; } = string.Empty;
        public int EarlyYear { get; set; }
        public int LateYear { get; set; }
        public string EarlySeason { get; set; } = string.Empty;
        public string LateSeason { get; set; } = string.Empty;

        protected abstract bool IsEarly();
        public abstract CollectionChapter GetCollectionChapter();

        protected override string GetYearsSubFolder()
        {
            if (IsEarly())
            {
                Year = EarlyYear;
                Season = EarlySeason;
            }
            else
            {
                Year = LateYear;
                Season = LateSeason;
            }
            return base.GetYearsSubFolder();
        }
    }
}