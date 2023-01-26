using AOABO.Omnibus;

namespace AOABO.Config
{
    public class Chapter
    {
        public List<string> OriginalFilenames { get; set; } = new List<string>();
        public string ChapterName { get; set; }

        public string AltName { get; set; }
        public string SortOrder { get; set; }
        public string OriginalOrder { get; set; }
        public string FlatSubfolder { get; set; }
        public string PartsSubfolder { get; set; }
        public string VolumeSubfolder { get; set; }
        public string YearsSubfolder { get; set; }
        public int? Year { get; set; }
        public string Season { get; set; }

        public string Volume { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public string GetSubFolder(OutputStructure structure)
        {
            switch (structure)
            {
                case OutputStructure.Parts:
                    return GetPartSubFolder();
                case OutputStructure.Seasons:
                    return GetYearsSubFolder();
                case OutputStructure.Volumes:
                    return GetVolumeSubFolder();
                case OutputStructure.Flat:
                    return GetFlatSubFolder();
                default:
                    break;
            }
            return string.Empty;
        }

        protected virtual string GetPartSubFolder()
        {
            return Volume switch
            {
                "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\03-Story",
                "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\03-Story",
                "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\03-Story",
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\03-Story",
                "0501" => $"{Configuration.FolderNames["PartFive"]}\\03-Story",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }
        protected virtual string GetYearsSubFolder()
        {
            return $"03-Story\\[Year]\\{GetSeason}";
        }

        protected virtual string GetVolumeSubFolder()
        {
            return Volume switch
            {
                "0101" => $"{Configuration.FolderNames["PartOne"]}\\0101-Volume 1",
                "0102" => $"{Configuration.FolderNames["PartOne"]}\\0102-Volume 2",
                "0103" => $"{Configuration.FolderNames["PartOne"]}\\0103-Volume 3",
                "0201" => $"{Configuration.FolderNames["PartTwo"]}\\0201-Volume 1",
                "0202" => $"{Configuration.FolderNames["PartTwo"]}\\0201-Volume 2",
                "0203" => $"{Configuration.FolderNames["PartTwo"]}\\0201-Volume 3",
                "0204" => $"{Configuration.FolderNames["PartTwo"]}\\0201-Volume 4",
                "0301" => $"{Configuration.FolderNames["PartThree"]}\\0301-Volume 1",
                "0302" => $"{Configuration.FolderNames["PartThree"]}\\0302-Volume 2",
                "0303" => $"{Configuration.FolderNames["PartThree"]}\\0303-Volume 3",
                "0304" => $"{Configuration.FolderNames["PartThree"]}\\0304-Volume 4",
                "0305" => $"{Configuration.FolderNames["PartThree"]}\\0305-Volume 5",
                "0401" => $"{Configuration.FolderNames["PartFour"]}\\0401-Volume 1",
                "0402" => $"{Configuration.FolderNames["PartFour"]}\\0402-Volume 2",
                "0403" => $"{Configuration.FolderNames["PartFour"]}\\0403-Volume 3",
                "0404" => $"{Configuration.FolderNames["PartFour"]}\\0404-Volume 4",
                "0405" => $"{Configuration.FolderNames["PartFour"]}\\0405-Volume 5",
                "0406" => $"{Configuration.FolderNames["PartFour"]}\\0406-Volume 6",
                "0407" => $"{Configuration.FolderNames["PartFour"]}\\0407-Volume 7",
                "0408" => $"{Configuration.FolderNames["PartFour"]}\\0408-Volume 8",
                "0409" => $"{Configuration.FolderNames["PartFour"]}\\0409-Volume 9",
                "0501" => $"{Configuration.FolderNames["PartFive"]}\\0501-Volume 1",
                _ => throw new Exception($"GetVolumeSubFolder - {ChapterName}")
            };
        }

        protected virtual string GetFlatSubFolder()
        {
            return "03-Story";
        }

        private string GetSeason()
        {
            switch (Season)
            {
                case "Spring":
                    return "04-Spring";
                case "Summer":
                    return "01-Summer";
                case "Autumn":
                    return "02-Autumn";
                case "Winter":
                    return "03-Winter";
            }
            return string.Empty;
        }
        public bool ProcessedInPartOne { get; set; } = false;
        public bool ProcessedInPartTwo { get; set; } = false;
        public bool ProcessedInPartThree { get; set; } = false;
        public bool ProcessedInPartFour { get; set; } = false;
        public bool ProcessedInPartFive { get; set; } = false;
        public OCRSettings OCR { get; set; }

        public Chapter Copy()
        {
            return new Chapter
            {
                OriginalFilenames = OriginalFilenames.ToList(),
                AltName = AltName,
                ChapterName = ChapterName,
                FlatSubfolder = FlatSubfolder,
                OCR = OCR,
                OriginalOrder = OriginalOrder,
                PartsSubfolder = PartsSubfolder,
                ProcessedInPartFive = ProcessedInPartFive,
                ProcessedInPartFour = ProcessedInPartFour,
                ProcessedInPartOne = ProcessedInPartOne,
                ProcessedInPartThree = ProcessedInPartThree,
                ProcessedInPartTwo = ProcessedInPartTwo,
                SortOrder = SortOrder,
                VolumeSubfolder = VolumeSubfolder,
                Year = Year,
                YearsSubfolder = YearsSubfolder
            };
        }

        public void RemoveInserts()
        {
            OriginalFilenames.RemoveAll(x => x.StartsWith("insert", StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public class OCRSettings {
        public List<Correction> Corrections { get; set; } = new List<Correction>();
        public List<Italics> Italics { get; set; } = new List<Italics>();
        public string Header { get; set; }
    }

    public class Correction
    {
        public string Original { get; set; }
        public string Replacement { get; set; }
    }

    public class Italics
    {
        public string Start { get; set; }
        public string End { get; set; }
    }
}