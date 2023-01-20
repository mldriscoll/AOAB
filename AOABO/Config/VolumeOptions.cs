using AOABO.Omnibus;

namespace AOABO.Config
{
    public class VolumeOptions
    {
        public VolumeOptions()
        {
            UpdateChapterNames = false;
            BonusChapterSetting = BonusChapterSetting.Chronological;
            StartYear = 5;
            OutputStructure = OutputStructure.Volumes;
            OutputYearFormat = 0;
            AfterwordSetting = AfterwordSetting.None;
            IncludeRegularChapters = true;
        }

        public VolumeOptions(string str)
        {
            var split = str.Split("\r\n");
            if (split.Length > 0)
            {
                UpdateChapterNames = bool.Parse(split[0]);
            }
            if (split.Length > 1)
            {
                try
                {
                    BonusChapterSetting = EnumParse<BonusChapterSetting>(split[1]);
                }
                catch
                {
                    var oldSetting = bool.Parse(split[1]);
                    BonusChapterSetting = oldSetting ? BonusChapterSetting.Chronological : BonusChapterSetting.EndOfBook;
                }
            }
            if (split.Length > 2)
            {
                OutputStructure = EnumParse<OutputStructure>(split[2]);
            }
            if (split.Length > 3)
            {
                StartYear = int.Parse(split[3]);
            }
            if (split.Length > 4)
            {
                OutputYearFormat = int.Parse(split[4]);
            }
            if (split.Length > 5)
            {
                AfterwordSetting = EnumParse<AfterwordSetting>(split[5]);
            }
            if (split.Length > 6)
            {
                IncludeRegularChapters = bool.Parse(split[6]);
            }
            if (split.Length > 7)
            {
                IncludeImagesInChapters = bool.Parse(split[7]);
            }
            if (split.Length > 8)
            {
                UseHumanReadableFileStructure = bool.Parse(split[8]);
            }
        }

        private T EnumParse<T>(string str) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public override string ToString()
        {
            return $"{UpdateChapterNames}\r\n{BonusChapterSetting}\r\n{OutputStructure}\r\n{StartYear}\r\n{OutputYearFormat}" +
                $"\r\n{AfterwordSetting}\r\n{IncludeRegularChapters}\r\n{IncludeImagesInChapters}\r\n{UseHumanReadableFileStructure}";
        }

        public bool UpdateChapterNames { get; set; } = false;
        public BonusChapterSetting BonusChapterSetting { get; set; } = BonusChapterSetting.Chronological;
        public OutputStructure OutputStructure { get; set; } = OutputStructure.Volumes;
        public int StartYear { get; set; } = 5;
        public int OutputYearFormat { get; set; } = 0;
        public AfterwordSetting AfterwordSetting { get; set; } = AfterwordSetting.None;
        public bool IncludeRegularChapters { get; set; } = true;
        public bool IncludeImagesInChapters { get; set; } = true;
        public bool UseHumanReadableFileStructure { get; set; } = false;
    }
    public enum BonusChapterSetting
    {
        Chronological = 0,
        EndOfBook = 1,
        LeaveOut = 2
    }
    public enum AfterwordSetting
    {
        None,
        VolumeEnd,
        OmnibusEnd
    }
}