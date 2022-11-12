using AOABO.Omnibus;

namespace AOABO.Config
{
    public class Chapter
    {
        public List<string> OriginalFilenames { get; set; } = new List<string>();
        public string ChapterName { get; set; }

        public string AltName { get; set; }
        public string SortOrder { get; set; }
        public string FlatSubfolder { get; set; }
        public string PartsSubfolder { get; set; }
        public string VolumeSubfolder { get; set; }
        public string YearsSubfolder { get; set; }
        public int? Year { get; set; }

        public string GetSubFolder(OutputStructure structure)
        {
            switch (structure)
            {
                case OutputStructure.Parts:
                    return PartsSubfolder;
                case OutputStructure.Seasons:
                    return YearsSubfolder;
                case OutputStructure.Volumes:
                    return VolumeSubfolder;
                case OutputStructure.Flat:
                    return FlatSubfolder;
                default:
                    break;
            }
            return string.Empty;
        }
        public bool ProcessedInPartOne { get; set; } = false;
        public bool ProcessedInPartTwo { get; set; } = false;
        public bool ProcessedInPartThree { get; set; } = false;
        public bool ProcessedInPartFour { get; set; } = false;
        public bool ProcessedInPartFive { get; set; } = false;
        public OCRSettings OCR { get; set; }
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