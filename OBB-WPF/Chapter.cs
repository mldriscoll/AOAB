using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OBB_WPF
{
    public class Chapter : ChapterHolder, INotifyPropertyChanged
    {
        public enum ChapterType
        {
            Story,
            Bonus,
            NonStory
        }

        public ChapterType CType { get; set; } = ChapterType.Story;
        public string ChapType
        {
            get { return CType.ToString(); }
            set
            {
                CType = (ChapterType)Enum.Parse(typeof(ChapterType), value);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ChapType"));
            }
        }

        private string _name = String.Empty;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        private string _sortOrder = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SortOrder {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SortOrder"));
            }
        }
        public ObservableCollection<Source> Sources { get; set; } = new ObservableCollection<Source> { };

        public ObservableCollection<Link> LinkedChapters { get; set; } = new ObservableCollection<Link>();

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
            foreach(var newSource in other.Sources)
            {
                Sources.Add(newSource);
            }
            foreach(var chapter in other.Chapters)
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

            foreach(var chapter in Chapters)
            {
                ret.AddRange(chapter.FindDupes(sourceList));
            }
            return ret;
        }

    }
}