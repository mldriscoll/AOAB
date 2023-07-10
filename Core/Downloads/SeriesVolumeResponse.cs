namespace Core.Downloads
{
    public class SeriesVolumeResponse
    {
        public string? title { get; set; }
        public string? slug { get; set; }
        public int number { get; set; }
        public List<SeriesCreators>? creators { get; set; }
        public string? publishing { get; set; }
    }
}
