using AOABO.Config;

namespace AOABO.Chapters
{
    public class Map : Chapter
    {
        protected override string GetFlatSubFolder()
        {
            return $"05-{Configuration.FolderNames["Maps"]}";
        }

        protected override string GetPartSubFolder()
        {
            return $"10-{Configuration.FolderNames["Maps"]}";
        }

        protected override string GetVolumeSubFolder()
        {
            return $"07-{Configuration.FolderNames["Maps"]}";
        }

        protected override string GetYearsSubFolder()
        {
            return $"05-{Configuration.FolderNames["Maps"]}";
        }
    }

}