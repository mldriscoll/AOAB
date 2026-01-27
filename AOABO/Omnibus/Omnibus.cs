namespace AOABO.Omnibus
{
    public class Omnibus : ChapterHolder
    {
        public Source? Cover { get; set; } = null;

        public string Author { get; set; } = string.Empty;
        public string AuthorSort { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;

        public void Combine(Omnibus other)
        {
            foreach (var chapter in other.Chapters)
            {
                var match = Chapters.FirstOrDefault(x => x.Match(chapter));
                if (match != null)
                    match.Combine(chapter);
                else
                    Chapters.Add(chapter);

            }
        }
    }
}