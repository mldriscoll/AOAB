namespace AOABO.Config
{
    public abstract class MoveableChapter : Chapter
    {
        public string EarlySortOrder { get; set; }
        public string LateSortOrder { get; set; }
        public int EarlyYear { get; set; }
        public int LateYear { get; set; }
        public string EarlySeason { get; set; }
        public string LateSeason { get; set; }

        protected abstract bool IsEarly();

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