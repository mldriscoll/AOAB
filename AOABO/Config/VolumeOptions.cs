using AOABO.Omnibus;
using System.Text.Json.Serialization;

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
            if (split.Length > 9)
            {
                MangaChapters = EnumParse<BonusChapterSetting>(split[9]);
            }
            if (split.Length > 10)
            {
                ComfyLifeChapters = EnumParse<ComfyLifeSetting>(split[10]);
            }
            if (split.Length > 11)
            {
                CharacterSheets = EnumParse<CharacterSheets>(split[11]);
            }
            if (split.Length > 12)
            {
                Maps = bool.Parse(split[12]);
            }
            if (split.Length > 13)
            {
                SplashImages = EnumParse<GallerySetting>(split[13]);
            }
            if (split.Length > 14)
            {
                ChapterImages = EnumParse<GallerySetting>(split[14]);
            }
            if (split.Length > 15)
            {
                Polls = bool.Parse(split[15]);
            }
        }

        private T EnumParse<T>(string str) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public bool UpdateChapterNames { get; set; } = false;
        public BonusChapterSetting BonusChapterSetting { get; set; } = BonusChapterSetting.Chronological;
        public OutputStructure OutputStructure { get; set; } = OutputStructure.Volumes;
        public int StartYear { get; set; } = 5;
        public int OutputYearFormat { get; set; } = 0;
        public AfterwordSetting AfterwordSetting { get; set; } = AfterwordSetting.OmnibusEnd;
        public bool IncludeRegularChapters { get; set; } = true;
        [JsonIgnore]
        public bool IncludeImagesInChapters { set { Image.IncludeImagesInChapters = value; } }
        public bool UseHumanReadableFileStructure { get; set; } = false;
        public BonusChapterSetting MangaChapters { get; set; } = BonusChapterSetting.Chronological;
        public ComfyLifeSetting ComfyLifeChapters { get; set; } = ComfyLifeSetting.VolumeEnd;
        public CharacterSheets CharacterSheets { get; set; } = CharacterSheets.PerPart;
        [JsonIgnore]
        public GallerySetting SplashImages { set { Image.SplashImages = value; } }
        [JsonIgnore]
        public GallerySetting ChapterImages { set { Image.ChapterImages = value; } }
        public bool Maps { get; set; } = true;
        public bool Polls { get; set; } = true;
        public Collections Collection { get; set; } = new Collections();
        public Images Image { get; set; } = new Images();

        public class Collections
        {
            public bool POVChapterCollection { get; set; } = true;
        }

        public class Images
        {
            public int? MaxHeight { get; set; }
            [JsonIgnore]
            public string MaxHeightSetting
            {
                get
                {
                    if (MaxHeight.HasValue)
                    {
                        return $"{MaxHeight.Value} pixels";
                    }
                    return "No limit set";
                }
            }
            public int? MaxWidth { get; set; }
            [JsonIgnore]
            public string MaxWidthSetting
            {
                get
                {
                    if (MaxWidth.HasValue)
                    {
                        return $"{MaxWidth.Value} pixels";
                    }
                    return "No limit set";
                }
            }
            public bool IncludeImagesInChapters { get; set; } = true;
            [JsonIgnore]
            public string IncludeImagesInChaptersSetting { get
                {
                    return IncludeImagesInChapters ? "Included" : "Excluded";
                } }
            public int Quality { get; set; } = 90;
            public GallerySetting SplashImages { get; set; } = GallerySetting.Start;
            [JsonIgnore]
            public string SplashImagesSetting { get
                {
                    switch (SplashImages)
                    {
                        case GallerySetting.Start:
                            return "at start of each volume";
                        case GallerySetting.End:
                            return "at end of each volume";
                    }

                    return "no Gallery";
                } }
            public GallerySetting ChapterImages { get; set; } = GallerySetting.None;
            [JsonIgnore]
            public string ChapterImagesSetting
            {
                get
                {
                    switch (ChapterImages)
                    {
                        case GallerySetting.Start:
                            return "at start of each volume";
                        case GallerySetting.End:
                            return "at end of each volume";
                    }

                    return "no Gallery";
                }
            }
        }
    }
}