using AOABO.Omnibus;
using System.Text.Json.Serialization;

namespace AOABO.Config
{
    public class VolumeOptions
    {
        public VolumeOptions()
        {
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
                    BonusChapterSetting = oldSetting ? Config.BonusChapterSetting.Chronological : Config.BonusChapterSetting.EndOfBook;
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

        public void Upgrade()
        {
            if (IncludeImagesInChapters.HasValue)
            {
                Image.IncludeImagesInChapters = IncludeImagesInChapters.Value;
                IncludeImagesInChapters = null;
            }
            if (SplashImages.HasValue)
            {
                Image.SplashImages = SplashImages.Value;
                SplashImages = null;
            }
            if (ChapterImages.HasValue)
            {
                Image.ChapterImages = ChapterImages.Value;
                ChapterImages = null;
            }
            if (IncludeRegularChapters.HasValue)
            {
                Chapter.IncludeRegularChapters = IncludeRegularChapters.Value;
                IncludeRegularChapters = null;
            }
            if (BonusChapterSetting.HasValue)
            {
                Chapter.BonusChapter = BonusChapterSetting.Value;
                BonusChapterSetting = null;
            }
            if (MangaChapters.HasValue)
            {
                Chapter.MangaChapters = MangaChapters.Value;
                MangaChapters = null;
            }
            if (UpdateChapterNames.HasValue)
            {
                Chapter.UpdateChapterNames = UpdateChapterNames.Value;
                UpdateChapterNames = null;
            }
            if (ComfyLifeChapters.HasValue)
            {
                Extras.ComfyLifeChapters = ComfyLifeChapters.Value;
                ComfyLifeChapters = null;
            }
            if (CharacterSheets.HasValue)
            {
                Extras.CharacterSheets = CharacterSheets.Value;
                CharacterSheets = null;
            }
            if (Maps.HasValue)
            {
                Extras.Maps = Maps.Value;
                Maps = null;
            }
            if (AfterwordSetting.HasValue)
            {
                Extras.Afterword = AfterwordSetting.Value;
                AfterwordSetting = null;
            }
            if (Polls.HasValue)
            {
                Extras.Polls = Polls.Value;
                Polls = null;
            }
        }

        public bool? UpdateChapterNames { get; set; }
        public BonusChapterSetting? BonusChapterSetting { get; set; }
        public OutputStructure OutputStructure { get; set; } = OutputStructure.Volumes;
        [JsonIgnore]
        public string OutputStructureSetting { get
            {
                switch (OutputStructure)
                {
                    case OutputStructure.Volumes:
                        return "by Part/Volume";
                    case OutputStructure.Parts:
                        return "by Part";
                    case OutputStructure.Seasons:
                        return "by Year/Season";
                }
                return "Flat";
            } }
        public int StartYear { get; set; } = 5;
        public int OutputYearFormat { get; set; } = 0;
        public AfterwordSetting? AfterwordSetting { get; set; }
        public bool? IncludeRegularChapters { get; set; }
        public bool? IncludeImagesInChapters { get; set; }
        public bool UseHumanReadableFileStructure { get; set; } = false;
        public BonusChapterSetting? MangaChapters { get; set; }
        public ComfyLifeSetting? ComfyLifeChapters { get; set; }
        public CharacterSheets? CharacterSheets { get; set; }
        public GallerySetting? SplashImages { get; set; }
        public GallerySetting? ChapterImages { get; set; }
        public bool? Maps { get; set; }
        public bool? Polls { get; set; }
        public Collections Collection { get; set; } = new Collections();
        public Images Image { get; set; } = new Images();
        public Chapters Chapter { get; set; } = new Chapters();
        public ExtraContent Extras { get; set; } = new ExtraContent();

        public class Collections
        {
            public bool POVChapterCollection { get; set; } = true;
            public bool POVChapterOrdering { get; set; } = false;
            [JsonIgnore]
            public string POVChapterOrderingSetting { get
                {
                    if (!POVChapterCollection) return string.Empty;
                    return POVChapterOrdering ? "in Character Order" : "in Chronological Order";
                } }
        }

        public class ExtraContent
        {
            public ComfyLifeSetting ComfyLifeChapters { get; set; } = ComfyLifeSetting.VolumeEnd;
            [JsonIgnore]
            public string ComfyLifeChaptersSetting { get { return BonusChapterSettingText(ComfyLifeChapters); } }
            public CharacterSheets CharacterSheets { get; set; } = CharacterSheets.PerPart;
            [JsonIgnore]
            public string CharacterSheetsSetting
            {
                get
                {
                    switch (CharacterSheets)
                    {
                        case CharacterSheets.PerPart:
                            return "one per Part";
                        case CharacterSheets.All:
                            return "all of them";
                    }
                    return "none of them";
                }
            }

            private string BonusChapterSettingText(ComfyLifeSetting setting)
            {
                switch (setting)
                {
                    case Config.ComfyLifeSetting.VolumeEnd:
                        return "placed after the relevant volume";
                    case Config.ComfyLifeSetting.OmnibusEnd:
                        return "placed in a section after the story content";
                }

                return "left out";
            }
            public bool Maps { get; set; } = true;
            public AfterwordSetting Afterword { get; set; } = Config.AfterwordSetting.OmnibusEnd;
            [JsonIgnore]
            public string AfterwordSetting
            {
                get
                {
                    switch (Afterword)
                    {
                        case Config.AfterwordSetting.VolumeEnd:
                            return "at the end of each volume";
                        case Config.AfterwordSetting.OmnibusEnd:
                            return "at the end of the Omnibus";
                    }
                    return "leave out";
                }
            }

            public bool Polls { get; set; } = true;
        }

        public class Chapters
        {
            public bool IncludeRegularChapters { get; set; } = true;
            public BonusChapterSetting BonusChapter { get; set; } = Config.BonusChapterSetting.Chronological;
            [JsonIgnore]
            public string BonusChapterSetting { get { return BonusChapterSettingText(BonusChapter); } }
            public BonusChapterSetting MangaChapters { get; set; } = Config.BonusChapterSetting.Chronological;
            [JsonIgnore]
            public string MangaChapterSetting { get { return BonusChapterSettingText(MangaChapters); } }

            private string BonusChapterSettingText(BonusChapterSetting setting)
            {
                switch (setting)
                {
                    case Config.BonusChapterSetting.Chronological:
                        return "placed after the last overlapping chapter";
                    case Config.BonusChapterSetting.EndOfBook:
                        return "placed after the last overlapping volume";
                }

                return "left out";
            }

            public bool UpdateChapterNames { get; set; } = false;
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