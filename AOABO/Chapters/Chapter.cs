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

        public string Set { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;

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
                "0501" or "0502" or "0503" or "0504" or "0505" or "0506" or "0507" or "0508" or "0509" or "0510" or "0511" or "0512" => $"{Configuration.FolderNames["PartFive"]}\\03-Story",
                "0601" => $"{Configuration.FolderNames["Hannelore"]}\\03-Story",
                "FB1" or "FB2" or "FB3" or "FB4" or "FB5" => $"11-Fanbooks",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }
        public string GetVolumeName()
        {
            return Volume switch
            {
                "0101" or "0102" or "0103" => $"P1V{Volume[3]}",
                "0201" or "0202" or "0203" or "0204" => $"P2V{Volume[3]}",
                "0301" or "0302" or "0303" or "0304" or "0305" => $"P3V{Volume[3]}",
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"P4V{Volume[3]}",
                "0501" or "0502" or "0503" or "0504" or "0505" or "0506" or "0507" or "0508" or "0509" => $"P5V{Volume[3]}",
                "0510" or "0511" or "0512" => $"P5V{Volume[2]}{Volume[3]}",
                "0601" => $"H5V{Volume[3]}",
                "FB1" or "FB2" or "FB3" or "FB4" or "FB5" => $"11-Fanbooks",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }
        protected virtual string GetYearsSubFolder()
        {
            return Volume switch
            {
                "FB1" or "FB2" or "FB3" or "FB4" or "FB5" => $"06-Fanbooks",
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
                "0501" or "0502" or "0503" or "0504" or "0505" or "0506" or "0507" or "0508" or "0509" or "0510" or "0511" or "0512" => $"{Configuration.FolderNames["PartFive"]}\\{Volume}-{getVolumeName()}",
                "0601" => $"{Configuration.FolderNames["Hannelore"]}\\{Volume}-{getVolumeName()}",
                "FB1" or "FB2" or "FB3" or "FB4" or "FB5" => $"08-Fanbooks",
                _ => throw new Exception($"GetVolumeSubFolder - {ChapterName}")
            };
        }

        protected string getVolumeName()
        {
            return Volume switch
            {
                "0101" or "0201" or "0301" or "0401" or "0501" or "0601" => "Volume 1",
                "0102" or "0202" or "0302" or "0402" or "0502" => "Volume 2",
                "0103" or "0203" or "0303" or "0403" or "0503" => "Volume 3",
                "0204" or "0304" or "0404" or "0504" => "Volume 4",
                "0305" or "0405" or "0505" => "Volume 5",
                "0406" or "0506" => "Volume 6",
                "0407" or "0507" => "Volume 7",
                "0408" or "0508" => "Volume 8",
                "0409" or "0509" => "Volume 9",
                "0510" => "Volume 10",
                "0511" => "Volume 11",
                "0512" => "Volume 12",
                _ => throw new NotImplementedException($"GetVolumeName - {Volume}")
            };
        }

        protected virtual string GetFlatSubFolder()
        {
            return Volume switch
            {
                "FB1" or "FB2" or "FB3" or "FB4" or "FB5" => $"06-Fanbooks",
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

        public bool ProcessedInHannelore { get; set; } = false;
        public string? StartLine { get; set; }
        public string? EndLine { get; set; }

        public void RemoveInserts()
        {
            OriginalFilenames.RemoveAll(x => x.StartsWith("insert", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}