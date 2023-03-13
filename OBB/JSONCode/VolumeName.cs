namespace OBB.JSONCode
{
    public class VolumeName
    {
        public string ApiSlug { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? EditedBy { get; set; }
        public bool ShowRemainingFiles { get; set; } = true;
    }
}
