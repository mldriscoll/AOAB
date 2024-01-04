using System.Collections.ObjectModel;

namespace OBB_WPF
{
    public class Omnibus : ChapterHolder
    {
        public Source Cover { get; set; } = null;

        public string Name { get; set; } = string.Empty;

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

        public ObservableCollection<Source> UnusedSources { get; set; } = new ObservableCollection<Source>();
    }
}