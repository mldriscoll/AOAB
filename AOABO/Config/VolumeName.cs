namespace AOABO.Config
{
    public class VolumeName
    {
        public string InternalName { get; set; }
        public string ApiSlug { get; set; }
        public string FileName { get; set; }
        public bool OutputUnusedFiles { get; set; } = true;
        public List<string> Quality { get; set; }

        public string NameMatch(string[] names)
        {
            var matches = getMatches(names).ToList();

            switch (matches.Count())
            {
                case 0: return null;
                case 1: return matches[0];
                default:
                    foreach (var q in Quality)
                    {
                        var match = matches.FirstOrDefault(x => x.Contains(q));
                        if (match is not null) return match;
                    }
                    return null;
            }
        }

        private IEnumerable<string> getMatches(string[] names)
        {
            foreach (var name in names)
            {
                if (name.EndsWith(FileName)) yield return name;

                if (Quality is null) continue;

                foreach (var quality in Quality)
                {
                    var str = string.Format(FileName, quality);
                    if (name.EndsWith(str)) yield return name;
                }
            }
        }
    }
}