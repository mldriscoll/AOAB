using AOABO.Chapters;

namespace AOABO.Config
{
    public class CharacterSheetChapter : Chapter
    {
        public bool PartSheet { get; set; } = false;


        protected override string GetFlatSubFolder()
        {
            if (Configuration.Options.Extras.CharacterSheets == CharacterSheets.All)
            {
                return base.GetFlatSubFolder();
            }
            SortOrder = SortOrder.Substring(0, 2);
            return Configuration.FolderNames["CharacterSheets"];
        }

        protected override string GetYearsSubFolder()
        {
            if (Configuration.Options.Extras.CharacterSheets == CharacterSheets.All)
            {
                return base.GetYearsSubFolder();
            }
            SortOrder = SortOrder.Substring(0, 2);
            return Configuration.FolderNames["CharacterSheets"];
        }

        protected override string GetPartSubFolder()
        {
            if (Configuration.Options.Extras.CharacterSheets == CharacterSheets.All)
            {
                return Volume switch
                {
                    "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\03-Story",
                    "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\03-Story",
                    "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\03-Story",
                    "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\03-Story",
                    "0501" or "0502" or "0503" => $"{Configuration.FolderNames["PartFive"]}\\03-Story",
                    _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
                };
            }
            SortOrder = SortOrder.Substring(0, 2);
            return Volume switch
            {
                "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}",
                "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}",
                "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}",
                "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}",
                "0501" or "0502" or "0503" => $"{Configuration.FolderNames["PartFive"]}",
                _ => throw new Exception($"GetPartSubFolder - {ChapterName}"),
            };
        }

        protected override string GetVolumeSubFolder()
        {
            if (Configuration.Options.Extras.CharacterSheets == CharacterSheets.All)
            {
                return Volume switch
                {
                    "0101" or "0102" or "0103" => $"{Configuration.FolderNames["PartOne"]}\\{Volume}-{getVolumeName()}",
                    "0201" or "0202" or "0203" or "0204" => $"{Configuration.FolderNames["PartTwo"]}\\{Volume}-{getVolumeName()}",
                    "0301" or "0302" or "0303" or "0304" or "0305" => $"{Configuration.FolderNames["PartThree"]}\\{Volume}-{getVolumeName()}",
                    "0401" or "0402" or "0403" or "0404" or "0405" or "0406" or "0407" or "0408" or "0409" => $"{Configuration.FolderNames["PartFour"]}\\{Volume}-{getVolumeName()}",
                    "0501" or "0502" or "0503" => $"{Configuration.FolderNames["PartFive"]}\\{Volume}-{getVolumeName()}",
                    _ => throw new Exception($"GetVolumeSubFolder - {ChapterName}")
                };
            }

            SortOrder = SortOrder.Substring(0, 2);
            return GetPartSubFolder();
        }
    }

}