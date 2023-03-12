namespace OBB.JSON
{
    public class Series
    {
        public string Name { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public List<VolumeName> Volumes { get; set; } = new List<VolumeName>();
        public string Author { get; set; } = string.Empty;
        public string AuthorSort { get; set; } = string.Empty;
    }
}
