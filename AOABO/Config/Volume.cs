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
        public List<BonusChapter> MangaChapters { get; set; } = new List<BonusChapter>();

        public List<BonusChapter> BonusChapters { get; set; } = new List<BonusChapter>();

        public ComfyLifeChapter ComfyLifeChapter { get; set; }

        public Afterword Afterword { get; set; }

        public Gallery Gallery { get; set; }

        public CharacterSheetChapter CharacterSheet { get; set; }

        public List<Chapter> Maps { get; set; } = new List<Chapter>();
    }

    public class BonusChapter : Chapter
    {
        public string AlternateSortOrder { get; set; }

        public void UseAlternateSortOrder()
        {
            SortOrder = AlternateSortOrder;
        }
    }

    public class CharacterSheetChapter : Chapter
    {
        public bool PartOnly { get; set; } = false;
        public string PartOnlySubfolder { get; set; }

        public List<Chapter> PartOnlyChapter()
        {
            if (!PartOnly)
            {
                return new List<Chapter>();
            }

            var chapter = Copy();

            chapter.VolumeSubfolder = PartOnlySubfolder;

            return new List<Chapter> { chapter };
        }
    }

    public class ComfyLifeChapter : Chapter
    {
        public string PartEndSortOrder { get; set; }
        public string PartEndVolumeFolder { get; set; }
        public string PartEndYearFolder { get; set; }
        public string OmnibusEndSortOrder { get; set; }
        public string OmnibusEndFolder { get; set; }
        public int PartEndYear { get; set; }
        public int OmnibusEndYear { get; set; }
        public void SetSortOrder(ComfyLifeSetting setting)
        {
            switch (setting)
            {
                case ComfyLifeSetting.OmnibusEnd:
                    SortOrder = OmnibusEndSortOrder;
                    YearsSubfolder = OmnibusEndFolder;
                    VolumeSubfolder = OmnibusEndFolder;
                    PartsSubfolder = OmnibusEndFolder;
                    FlatSubfolder = OmnibusEndFolder;
                    Year = OmnibusEndYear;
                    break;
                case ComfyLifeSetting.PartEnd:
                    SortOrder = PartEndSortOrder;
                    YearsSubfolder = PartEndYearFolder;
                    VolumeSubfolder = PartEndVolumeFolder;
                    Year = PartEndYear;
                    break;
            }
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