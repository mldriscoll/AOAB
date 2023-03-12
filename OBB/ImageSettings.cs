namespace OBB
{
    public class ImageSettings
    {
        public bool IncludeBonusArtAtStartOfVolume { get; set; } = true;
        public bool IncludeBonusArtAtEndOfVolume { get; set; } = false;
        public bool IncludeInsertsAtStartOfVolume { get; set; } = false;
        public bool IncludeInsertsAtEndOfVolume { get; set; } = false;
        public bool IncludeInsertsInChapters { get; set; } = true;
        public int? MaxImageHeight { get; set; }
        public int? MaxImageWidth { get; set; }
        public int ImageQuality { get; set; } = 90;
    }
}
