namespace AOABO.Config
{
    public class ComfyLifeChapter : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return "05-A Comfy Life with my Family";
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