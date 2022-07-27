namespace AOABO.Processor
{
    public class Chapter
    {
        public string Contents { get; set; }
        public List<string> CssFiles { get; set; }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                var index = value.LastIndexOf('.');
                name = value.Substring(0, index);
                Extension = value.Substring(index + 1);
            }
        }
        public string Extension { get; set; }
        public string SortOrder { get; set; }
        public string SubFolder { get; set; }

        public string FileName { get { return $"{SortOrder}-{Name}.{Extension}"; } }
    }
}