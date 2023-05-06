namespace OBB.JSONCode
{
    public class Chapter
    {
        public string ChapterName { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
        public string SubFolder { get; set; } = string.Empty;
        public List<string> OriginalFilenames { get; set; } = new List<string>();

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();

        public List<ChapterSplit> Splits { get; set; } = new List<ChapterSplit>();

        public List<Replacements> Replacements { get; set; } = new List<Replacements>();

        public List<string> LinkedChapters { get; set; } = new List<string>();

        public bool KeepFirstSplitSection { get; set; } = true;
        public void RemoveInserts()
        {
            OriginalFilenames.RemoveAll(x => x.StartsWith("insert", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
