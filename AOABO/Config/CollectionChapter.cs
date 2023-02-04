namespace AOABO.Config
{
    public class CollectionChapter : Chapter
    {
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
                    return "05-POV Chapters";
                default:
                    throw new Exception($"GalleryChapter Unknown Gallery Type {Gallery}");
            }
        }
    }
}