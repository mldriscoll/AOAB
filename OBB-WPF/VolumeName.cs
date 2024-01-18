namespace OBB_WPF
{
    public class VolumeName
    {
        public string ApiSlug { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> EditedBy { get; set; } = new List<string>();
        public string? Published { get; set; } = null;
        public int Order { get; set; }
    }
}