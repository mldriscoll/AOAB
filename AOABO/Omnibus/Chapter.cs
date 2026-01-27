namespace AOABO.Omnibus
{
    public class Chapter : ChapterHolder
    {
        public enum ChapterType
        {
            Story,
            Bonus,
            NonStory,
            Part,
            Volume,
            Map,
            CharacterSheet,
            Afterword,
            ComfyLife,
            Poll,
            QnAs,
            DramaCD,
            Fanbook,
            MangaWritten,
            Covers
        }

        public ChapterType CType { get; set; } = ChapterType.Story;

        public string Name { get; set; } = string.Empty;

        public string SortOrder { get; set; } = string.Empty;

        public string POV { get; set; } = string.Empty;
        public List<Source> Sources { get; set; } = new List<Source> { };

        public string EndsBeforeLine { get; set; } = string.Empty;
        public string StartsAtLine { get; set; } = string.Empty;

        public List<SubSection> SubSections { get; set; } = new List<SubSection> { };

        public class SubSection
        {
            public int StartsAtIndex { get; set; }
            public string StartsAtLine { get; set; } = string.Empty;
            public int EndsAtIndex { get; set; }
            public string EndsAtLine { get; set; } = string.Empty;
        }



        public bool Match(Chapter other)
        {
            return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                && other.SortOrder.Equals(SortOrder, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Combine(Chapter other)
        {
            foreach (var newSource in other.Sources)
            {
                Sources.Add(newSource);
            }
            foreach (var chapter in other.Chapters)
            {
                var match = Chapters.FirstOrDefault(x => x.Match(chapter));
                if (match != null)
                    match.Combine(chapter);
                else
                    Chapters.Add(chapter);
            }
        }

        public List<Source> FindDupes(List<Source> sourceList)
        {
            var ret = new List<Source>();
            foreach (var s in sourceList)
            {
                if (Sources.Contains(s))
                {
                    ret.Add(s);
                }
            }

            foreach (var chapter in Chapters)
            {
                ret.AddRange(chapter.FindDupes(sourceList));
            }
            return ret;
        }

        public string Subfolder { get; set; } = string.Empty;

        public class Tag
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public Tag[] Tags { get; set; } = [];
        public int? Year { get; set; } = null;
        public string? Season { get; set; } = null;
        public string? OriginalSource { get; set; } = null;

        public string? Set { get; set; } = null;

        public Chapter Clone()
        {
            return new Chapter
            {
                Name = Name,
                Year = Year,
                Season = Season,
                OriginalSource = OriginalSource,
                Set = Set,
                CType = CType,
                EndsBeforeLine = EndsBeforeLine,
                POV = POV,
                SortOrder = SortOrder,
                Sources = Sources,
                StartsAtLine = StartsAtLine,
                Subfolder = Subfolder,
                Tags = Tags,
                SubSections = SubSections
            };
        }
    }
}