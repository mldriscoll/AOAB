using AOABO.Config;

namespace AOABO.Chapters
{
    public class CollectionChapter : Chapter
    {
        public string SubFolder { get; set; } = string.Empty;
        public CollectionEnum Gallery { get; set; }
        public enum CollectionEnum
        {
            POVGallery
        };

        protected override string GetVolumeSubFolder()
        {
            return GetFlatSubFolder();
        }

        protected override string GetPartSubFolder()
        {
            return GetFlatSubFolder();
        }

        protected override string GetYearsSubFolder()
        {
            return GetFlatSubFolder();
        }

        protected override string GetFlatSubFolder()
        {
            switch (Gallery)
            {
                case CollectionEnum.POVGallery:
                    if (string.IsNullOrWhiteSpace(SubFolder))
                    {
                        return Configuration.FolderNames["POVGallery"];
                    }
                    return $"{Configuration.FolderNames["POVGallery"]}\\{SubFolder}";
                default:
                    throw new Exception($"GalleryChapter Unknown Gallery Type {Gallery}");
            }
        }
    }
}