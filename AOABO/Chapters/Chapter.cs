using AOABO.Config;
using AOABO.Omnibus;

namespace AOABO.Chapters
{
    public class Chapter
    {
        public List<string> OriginalFilenames { get; set; } = new List<string>();
        public string ChapterName { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public string Season { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;

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
                "0501" or "0502" => $"{Configuration.FolderNames["PartFive"]}\\03-Story",
                "FB1" or "FB2" or "FB3" => $"11-Fanbooks",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }
        protected virtual string GetYearsSubFolder()
        {
            return Volume switch
            {
                "FB1" or "FB2" or "FB3" => $"06-Fanbooks",
                _ => $"03-Story\\[Year]\\{GetSeason()}"
            };
        }

        protected virtual string GetVolumeSubFolder()
        {
            return Volume switch
            {
                "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\{Volume}-{getVolumeName()}",
                "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\{Volume}-{getVolumeName()}",
                "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\{Volume}-{getVolumeName()}",
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\{Volume}-{getVolumeName()}",
                "0501" or "0502" => $"{Configuration.FolderNames["PartFive"]}\\{Volume}-{getVolumeName()}",
                "FB1" or "FB2" or "FB3" => $"06-Fanbooks",
                _ => throw new Exception($"GetVolumeSubFolder - {ChapterName}")
            };
        }

        protected string getVolumeName()
        {
            return Volume switch
            {
                "0101" or "0201" or "0301" or "0401" or "0501" => "Volume 1",
                "0102" or "0202" or "0302" or "0402" or "0502" => "Volume 2",
                "0103" or "0203" or "0303" or "0403" => "Volume 3",
                "0204" or "0304" or "0404" => "Volume 4",
                "0305" or "0405" => "Volume 5",
                "0406" => "Volume 6",
                "0407" => "Volume 7",
                "0408" => "Volume 8",
                "0409" => "Volume 9",
                _ => throw new NotImplementedException($"GetVolumeName - {Volume}")
            };
        }

        protected virtual string GetFlatSubFolder()
        {
            return Volume switch
            {
                "FB1" or "FB2" or "FB3" => $"06-Fanbooks",
                _ => $"03-Story"
            };
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
                default:
                    return "00-Unknown";
            }
        }
        public bool ProcessedInPartOne { get; set; } = false;
        public bool ProcessedInPartTwo { get; set; } = false;
        public bool ProcessedInPartThree { get; set; } = false;
        public bool ProcessedInPartFour { get; set; } = false;
        public bool ProcessedInPartFive { get; set; } = false;
        public bool ProcessedInFanbooks { get; set; } = false;

        public void RemoveInserts()
        {
            OriginalFilenames.RemoveAll(x => x.StartsWith("insert", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}