using AOABO.Config;

namespace AOABO.Chapters
{
    public class ComfyLifeChapter : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return Configuration.FolderNames["ComfyLife"];
        }

        protected override string GetPartSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetPartSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
        }

        protected override string GetVolumeSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetVolumeSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
        }

        protected override string GetYearsSubFolder()
        {
            if (Configuration.Options.ComfyLifeChapters == ComfyLifeSetting.OmnibusEnd)
                return GetFlatSubFolder();

            return $"{base.GetYearsSubFolder()}\\{Volume}xx-{getVolumeName()} {Configuration.FolderNames["VolumeBonusChapters"]}";
        }
    }

}