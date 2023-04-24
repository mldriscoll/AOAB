namespace Core.Processor
{
    public class Chapter
    {
        public string Contents { get; set; } = string.Empty;
        public List<string> CssFiles { get; set; } = new List<string>();
        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set
            {
                var index = value.LastIndexOf('.');
                name = value[..index];
                Extension = value[(index + 1)..];
            }
        }
        public string Extension { get; set; } = ".xhtml";
        public string SortOrder { get; set; } = "00";
        public string SubFolder { get; set; } = string.Empty;

        public string FileName { get { return $"{SortOrder}-{Name}.{Extension}".Replace(":", ""); } }

        public string Set { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;

        public bool Processed { get; set; } = false;

        public string CombinedSortOrder()
        {
            if (string.IsNullOrWhiteSpace(SubFolder)) return SortOrder;

            return SubFolder + "\\" + SortOrder;
        }
    }
}