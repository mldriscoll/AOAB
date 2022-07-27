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
    }
}