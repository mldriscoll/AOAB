namespace AOABO.Config
{
    public class Volume
    {
        public string InternalName { get; set; }

        public override string ToString()
        {
            return InternalName ?? string.Empty;
        }

        public bool ProcessedInPartOne { get; set; } = false;
        public bool ProcessedInPartTwo { get; set; } = false;
        public bool ProcessedInPartThree { get; set; } = false;
        public bool ProcessedInPartFour { get; set; } = false;
        public bool ProcessedInPartFive { get; set; } = false;

        public bool OCR { get; set; } = false;

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();

        public List<BonusChapter> BonusChapters { get; set; } = new List<BonusChapter>();

        public Afterword Afterword { get; set; }

        public Gallery Gallery { get; set; }
    }

    public class BonusChapter : Chapter
    {
        public string AlternateSortOrder { get; set; }

        public void UseAlternateSortOrder()
        {
            SortOrder = AlternateSortOrder;
        }
    }

    public class Afterword : Chapter
    {
        public void EndOfOmnibus()
        {
            FlatSubfolder = "09-Afterwords";
            PartsSubfolder = "09-Afterwords";
            YearsSubfolder = "09-Afterwords";
            VolumeSubfolder = "09-Afterwords";
        }
    }

}