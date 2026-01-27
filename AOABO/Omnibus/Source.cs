namespace AOABO.Omnibus
{
    public class Source
    {
        public string File { get; set; } = string.Empty;

        public List<string> Alternates { get; set; } = new List<string>();

        public Source? OtherSide { get; set; } = null;

        public string SortOrder { get; set; } = string.Empty;

        public bool Exists(string path)
        {
            if (System.IO.File.Exists($"{path}\\{File}")) return true;
            foreach (var alternate in Alternates)
            {
                if (System.IO.File.Exists($"{path}\\{alternate}")) return true;
            }

            if (OtherSide != null) return OtherSide.Exists(path);

            return false;
        }
    }
}