using System.Collections.ObjectModel;

namespace OBB_WPF
{
    public abstract class ChapterHolder
    {
        public ObservableCollection<Chapter> Chapters { get; set; } = new ObservableCollection<Chapter>();
        public void Remove(Chapter chapter)
        {
            Chapters.Remove(chapter);
            foreach (var subchapter in Chapters)
            {
                subchapter.Remove(chapter);
            }
        }

        public List<Source> AllSources(string prefix)
        {
            var sources = new List<Source>();
            foreach(var chapter in Chapters)
            {
                sources.AddRange(chapter.Sources.Where(x => x.File.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)));
                foreach(var s in sources)
                {
                    chapter.Sources.Remove(s);
                }
                sources.AddRange(chapter.AllSources(prefix));
            }
            return sources;
        }

        public void RemoveEmpties()
        {
            foreach(var chapter in Chapters)
            {
                chapter.RemoveEmpties();
            }

            Chapters = new ObservableCollection<Chapter>(Chapters.Where(x => x.Sources.Any() || x.Chapters.Any()));
        }

        public void Sort()
        {
            var c = Chapters.OrderBy(x => x.SortOrder).ToList();
            foreach(var chapter in c)
            {
                Chapters.Remove(chapter);
                Chapters.Add(chapter);
            }

            foreach(var chapter in Chapters)
            {
                var sources = chapter.Sources.Where(x => x != null).OrderBy(x => x.SortOrder).ToList();
                chapter.Sources.Clear();
                foreach (var s in sources) chapter.Sources.Add(s);
                chapter.Sort();
            }
        }
    }
}