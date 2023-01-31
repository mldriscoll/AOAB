namespace AOABO.Config
{
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