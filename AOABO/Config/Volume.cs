namespace AOABO.Config
{
    public class Volume
    {
        public string InternalName { get; set; }

        public override string ToString()
        {
            return InternalName ?? string.Empty;
        }

        public bool ProcessedInPartOne
        {
            get {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInPartOne)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInPartOne)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInPartOne)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInPartOne) return true;
                if (Afterword != null && Afterword.ProcessedInPartOne) return true;
                if (Gallery != null && Gallery.ProcessedInPartOne) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInPartOne) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInPartOne) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInPartOne)) return true;

                return false;
            }
        }
        public bool ProcessedInPartTwo
        {
            get
            {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInPartTwo)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInPartTwo)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInPartTwo)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInPartTwo) return true;
                if (Afterword != null && Afterword.ProcessedInPartTwo) return true;
                if (Gallery != null && Gallery.ProcessedInPartTwo) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInPartTwo) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInPartTwo) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInPartTwo)) return true;

                return false;
            }
        }
        public bool ProcessedInPartThree
        {
            get
            {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInPartThree)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInPartThree)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInPartThree)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInPartThree) return true;
                if (Afterword != null && Afterword.ProcessedInPartThree) return true;
                if (Gallery != null && Gallery.ProcessedInPartThree) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInPartThree) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInPartThree) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInPartThree)) return true;

                return false;
            }
        }
        public bool ProcessedInPartFour
        {
            get
            {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInPartFour)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInPartFour)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInPartFour)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInPartFour) return true;
                if (Afterword != null && Afterword.ProcessedInPartFour) return true;
                if (Gallery != null && Gallery.ProcessedInPartFour) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInPartFour) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInPartFour) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInPartFour)) return true;

                return false;
            }
        }
        public bool ProcessedInPartFive
        {
            get
            {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInPartFive)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInPartFive)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInPartFive)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInPartFive) return true;
                if (Afterword != null && Afterword.ProcessedInPartFive) return true;
                if (Gallery != null && Gallery.ProcessedInPartFive) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInPartFive) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInPartFive) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInPartFive)) return true;

                return false;
            }
        }
        public bool ProcessedInFanbooks
        {
            get
            {
                if (Chapters != null && Chapters.Any(x => x.ProcessedInFanbooks)) return true;
                if (MangaChapters != null && MangaChapters.Any(x => x.ProcessedInFanbooks)) return true;
                if (BonusChapters != null && BonusChapters.Any(x => x.ProcessedInFanbooks)) return true;
                if (ComfyLifeChapter != null && ComfyLifeChapter.ProcessedInFanbooks) return true;
                if (Afterword != null && Afterword.ProcessedInFanbooks) return true;
                if (Gallery != null && Gallery.ProcessedInFanbooks) return true;
                if (CharacterSheet != null && CharacterSheet.ProcessedInFanbooks) return true;
                if (CharacterPoll != null && CharacterPoll.ProcessedInFanbooks) return true;
                if (Maps != null && Maps.Any(x => x.ProcessedInFanbooks)) return true;

                return false;
            }
        }

        public bool OCR { get; set; } = false;

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public List<BonusChapter> MangaChapters { get; set; } = new List<BonusChapter>();

        public List<BonusChapter> BonusChapters { get; set; } = new List<BonusChapter>();

        public ComfyLifeChapter ComfyLifeChapter { get; set; }

        public Afterword Afterword { get; set; }

        public Gallery Gallery { get; set; }

        public CharacterSheetChapter CharacterSheet { get; set; }

        public List<Map> Maps { get; set; } = new List<Map>();

        public CharacterPoll CharacterPoll { get; set; }
    }

    public class CharacterPoll : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return "CP-Character Polls";
        }

        protected override string GetPartSubFolder()
        {
            return "CP-Character Polls";
        }

        protected override string GetVolumeSubFolder()
        {
            return "CP-Character Polls";
        }

        protected override string GetYearsSubFolder()
        {
            return "CP-Character Polls";
        }
    }

    public class CharacterSheetChapter : Chapter
    {
        public bool PartSheet { get; set; } = false;


        protected override string GetFlatSubFolder()
        {
            if (Configuration.Options.CharacterSheets == CharacterSheets.All)
            {
                return base.GetFlatSubFolder();
            }
            return "02-Characters";
        }

        protected override string GetYearsSubFolder()
        {
            if (Configuration.Options.CharacterSheets == CharacterSheets.All)
            {
                return base.GetYearsSubFolder();
            }
            return "02-Characters";
        }

        protected override string GetPartSubFolder()
        {
            if (Configuration.Options.CharacterSheets == CharacterSheets.All)
            {
                return Volume switch
                {
                    "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\03-Story",
                    "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\03-Story",
                    "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\03-Story",
                    "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\03-Story",
                    "0501" or "0502" => $"{Configuration.FolderNames["PartFive"]}\\03-Story",
                    _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
                };
            }
            return Volume switch
            {
                "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}",
                "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}",
                "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}",
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}",
                "0501" or "0502" => $"{Configuration.FolderNames["PartFive"]}",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }

        protected override string GetVolumeSubFolder()
        {
            if (Configuration.Options.CharacterSheets == CharacterSheets.All)
            {
                return Volume switch
                {
                    "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\{Volume}-{getVolumeName()}",
                    "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\{Volume}-{getVolumeName()}",
                    "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\{Volume}-{getVolumeName()}",
                    "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\{Volume}-{getVolumeName()}",
                    "0501" or "0502" => $"{Configuration.FolderNames["PartFive"]}\\{Volume}-{getVolumeName()}",
                    _ => throw new Exception($"GetVolumeSubFolder - {ChapterName}")
                };
            }

            return GetPartSubFolder();
        }
    }

    public class Afterword : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return "09-Afterwords";
        }

        protected override string GetPartSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetPartSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
        }

        protected override string GetVolumeSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetVolumeSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
        }

        protected override string GetYearsSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetYearsSubFolder()}\\{Volume}xx-{getVolumeName()} Bonus";
        }
    }

}