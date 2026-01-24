using AOABO.Omnibus;
using Core.Downloads;
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
        public BonusChapterSetting? BonusChapterSetting
        {
            get
            {
                return null;
            }
            set
            {
            }
        }
        public OutputStructure OutputStructure { get; set; } = OutputStructure.Volumes;
        [JsonIgnore]
        public string OutputStructureSetting
        {
            get
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
            }
        }
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

        public Folders Folder { get; set; } = new Folders();
        public class Collections
        {
            public bool POVChapterCollection { get; set; } = true;
            public bool POVChapterOrdering { get; set; } = false;

            public string Prefix { get; set; } = "p";

            [JsonIgnore]
            public string POVChapterOrderingSetting { get
                {
                    if (!POVChapterCollection) return "Not Included";
                    return POVChapterOrdering ? $"in Character Order (position {Prefix})" : $"in Chronological Order (position {Prefix})";
                } }
        }

        public class ChapterSetting
        {
            public enum PositionEnum
            {
                Original,
                Volume,
                Part,
                Omnibus,
            }
            public PositionEnum Position { get; set; } = PositionEnum.Original;
            public bool Included { get; set; } = false;
            public string PositionPrefix { get; set; } = "M";

            public string Name { get; set; } = string.Empty;

            [JsonIgnore]
            public string Summary
            {
                get
                {
                    if (!Included) return "Not Included";

                    switch (Position)
                    {
                        case PositionEnum.Original:
                            return "In Original Position";
                        case PositionEnum.Volume:
                            return $"In Each Volume (position {PositionPrefix})";
                        case PositionEnum.Part:
                            return $"In Each Section (position {PositionPrefix})";
                        case PositionEnum.Omnibus:
                            return $"In the Omnibus (position {PositionPrefix})";
                    }

                    return "Not set";
                }
            }
        }

        public class ExtraContent
        {
            public ChapterSetting CoverSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Volume, PositionPrefix = "a", Name = "Cover" };
            public ChapterSetting MapSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Part, PositionPrefix = "b", Name = "Maps" };
            public ChapterSetting CharacterSheetSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Part, PositionPrefix = "b", Name = "Character Sheets" };
            public ChapterSetting ComfyLife { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Part, PositionPrefix = "o", Name = "Comfy Life Manga"};
            public ChapterSetting DramaCDSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Omnibus, PositionPrefix = "v", Name = "Drama CD Writeups" };
            public ChapterSetting PollSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Omnibus, PositionPrefix = "w", Name = "Character Polls" };
            public ChapterSetting QNASetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Omnibus, PositionPrefix = "x", Name = "Q&As" };
            public ChapterSetting FanbookMiscSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Omnibus, PositionPrefix = "y", Name = "Misc Fanbook Content" };
            public ChapterSetting AfterwordSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Omnibus, PositionPrefix = "z", Name = "Afterwords" };

            public ChapterSetting BonusSetting { get; set; } = new ChapterSetting { Included = true, Position = ChapterSetting.PositionEnum.Original, PositionPrefix = "n", Name = "Bonus Chapters" };

            [Obsolete]
            public ComfyLifeSetting? ComfyLifeChapters {
                get
                {
                    return null;
                }
                set
                {
                    if (value != default)
                    {
                        if (value != ComfyLifeSetting.None) ComfyLife.Included = true;

                        if (value == ComfyLifeSetting.VolumeEnd) ComfyLife.Position = ChapterSetting.PositionEnum.Part;

                        if (value == ComfyLifeSetting.OmnibusEnd) ComfyLife.Position = ChapterSetting.PositionEnum.Omnibus;
                    }
                }
            }

            [Obsolete]
            public CharacterSheets? CharacterSheets
            {
                get { return null; }
                set
                {
                    switch (value)
                    {
                        case Config.CharacterSheets.PerPart:
                            CharacterSheetSetting.Position = ChapterSetting.PositionEnum.Part;
                            break;
                        case Config.CharacterSheets.All:
                            CharacterSheetSetting.Position = ChapterSetting.PositionEnum.Original;
                            break;
                        case Config.CharacterSheets.None:
                            CharacterSheetSetting.Included = false;
                            break;
                    }
                }
            }

            [Obsolete]
            public bool? Maps
            {
                get
                {
                    return null;
                }
                set
                {
                    if (value != null)
                        MapSetting.Included = value.Value;
                }
            }

            [Obsolete]
            public AfterwordSetting? Afterword
            {
                get
                {
                    return null;
                }
                set
                {
                    switch (value)
                    {
                        case Config.AfterwordSetting.None:
                            AfterwordSetting.Included = false;
                            break;
                        case Config.AfterwordSetting.VolumeEnd:
                            AfterwordSetting.Position = ChapterSetting.PositionEnum.Original;
                            break;
                        case Config.AfterwordSetting.OmnibusEnd:
                            AfterwordSetting.Position = ChapterSetting.PositionEnum.Omnibus;
                            break;
                        case null:
                            break;
                    }
                }
            }

            [Obsolete]
            public bool? Polls
            {
                get { return null; }
                set { PollSetting.Included = value ?? false; }
            }

        }

        public class Chapters
        {
            public bool IncludeRegularChapters { get; set; } = true;
            public BonusChapterSetting? BonusChapter
            {
                get
                {
                    return null;
                }
                set
                {
                    switch (value)
                    {
                        case Config.BonusChapterSetting.Chronological:
                        case Config.BonusChapterSetting.SubChapter:
                            Configuration.Options.Extras.BonusSetting.Position = ChapterSetting.PositionEnum.Original;
                            break;
                        case Config.BonusChapterSetting.EndOfBook:
                            Configuration.Options.Extras.BonusSetting.Position = ChapterSetting.PositionEnum.Volume;
                            break;
                        case Config.BonusChapterSetting.LeaveOut:
                            Configuration.Options.Extras.BonusSetting.Included = false;
                            break;
                    }
                }
            }
            
            [JsonIgnore]
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
            public GallerySetting? SplashImages
            {
                get { return null; }
                set
                {
                    if (value == GallerySetting.None) Configuration.Options.Extras.CoverSetting.Included = false;
                    else Configuration.Options.Extras.CoverSetting.Included = true;
                }
            }

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

            public MangaQuality MangaQuality { get; set; } = MangaQuality.Desktop;

            [JsonIgnore]
            public string MangaQualitySetting
            {
                get
                {
                    return MangaQuality switch
                    {
                        MangaQuality.Mobile => "Mobile",
                        MangaQuality.Desktop => "Desktop",
                        MangaQuality.FourK => "4k",
                        _ => "not set",
                    };
                }
            }
        }

        public class Folders
        {
            public string InputFolder { get; set; } = string.Empty;
            [JsonIgnore]
            public string InputFolderSetting
            {
                get
                {
                    return FolderDisplay(InputFolder);
                }
            }

            public string OutputFolder { get; set; } = string.Empty;
            [JsonIgnore]
            public string OutputFolderSetting { get { return FolderDisplay(OutputFolder); } }

            private string FolderDisplay(string folder)
            {
                if (string.IsNullOrWhiteSpace(folder))
                    return "[current folder]";

                if (folder.Length > 1 && folder[1].Equals(':'))
                {
                    return folder;
                }

                return $"[current folder]\\{folder}";
            }

            public bool DeleteTempFolder { get; set; } = true;
        }
    }
}