using Core.Downloads;
using Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Json;
using static Core.Downloads.LibraryResponse.Book;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        public static Configuration Configuration { get; set; } = new Configuration();
        public static Login Login { get; set; }

        private async void Load()
        {
            Configuration = (await JSON.Load<Configuration>("Configuration.json")) ?? new Configuration();
#if DEBUG
            if (File.Exists($"..\\..\\..\\JSON\\Series.json"))
            {
                using (var stream = File.OpenRead($"..\\..\\..\\JSON\\Series.json"))
                {
                    Series = await JsonSerializer.DeserializeAsync<List<Series>>(stream);
                }
            }
#else
            if (File.Exists($"JSON\\Series.json"))
            {
                using (var stream = File.OpenRead($"JSON\\Series.json"))
                {
                    Series = await JsonSerializer.DeserializeAsync<List<Series>>(stream);
                }
            }
#endif
            Draw();
        }

        private async void Draw()
        {
            Update.IsEnabled = false;
            SeriesList.Items.Clear();
            SeriesList list = null;
            if (Login == null)
            {
                using (var client = new HttpClient())
                {
                    Login = await Login.FromFile(client);

                    if (Login == null)
                    {
                        var loginpage = new LoginWindow(client);
                        var success = loginpage.ShowDialog();
                        if (success.HasValue && success.Value)
                        {
                            Login = await Login.FromFile(client);
                        }
                    }
                }
            }

            if (Login != null)
            {
                var library = await Downloader.GetLibrary(new HttpClient(), Login.AccessToken);

                using (var client = new HttpClient())
                {
                    foreach (var book in library.books.Where(x => x.downloads.Any()))
                    {
                        if (Series.SelectMany(x => x.Volumes).Any(x => x.ApiSlug.Equals(book.volume.slug)))
                        {
                            var filename = Configuration.SourceFolder + "\\" + book.volume.slug + ".epub";
                            if (!File.Exists(filename))
                            {
                                using (var stream = await client.GetStreamAsync(book.downloads.Last().link))
                                {
                                    using (var filestream = File.OpenWrite(filename))
                                    {
                                        await stream.CopyToAsync(filestream);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var series in Series.Where(x => x.Volumes.Any(y => File.Exists(Configuration.SourceFolder + "\\" + y.FileName))))
            {
                var button = new Button { Content = series.Name };
                button.Click += (object sender, RoutedEventArgs e) =>
                {
                    var window = new SeriesPage(series);
                    window.Show();
                };
                SeriesList.Items.Add(button);
            }

            Update.IsEnabled = true;
        }

        public static List<Series> Series = new List<Series>();

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            var up = new UpdateWindow();
            up.Run();
            up.ShowDialog();
            Draw();
        }
    }

    public class Series
    {
        public string Name { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public List<SeriesSlug> ApiSlugs { get; set; } = new List<SeriesSlug>();
        public List<VolumeName> Volumes { get; set; } = new List<VolumeName>();
        public string Author { get; set; } = string.Empty;
        public string AuthorSort { get; set; } = string.Empty;
    }

    public class SeriesSlug
    {
        public int Order { get; set; }
        public string? Slug { get; set; }
    }
    public class VolumeName
    {
        public string ApiSlug { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> EditedBy { get; set; } = new List<string>();
        public string? Published { get; set; } = null;
        public int Order { get; set; }
    }

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

        private string _name;
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

    public class Link
    {
        public string OriginalLink { get; set; }
        public string Target { get; set; }
    }

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
            Chapters = new ObservableCollection<Chapter>(Chapters.OrderBy(x => x.SortOrder));

            foreach(var chapter in Chapters)
            {
                chapter.Sources = new ObservableCollection<Source>(chapter.Sources.OrderBy(x => x.SortOrder));
                chapter.Sort();
            }
        }
    }

    public class Source : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string File { get; set; } = string.Empty;

        public ObservableCollection<string> Alternates { get; set; } = new ObservableCollection<string>();

        public Source OtherSide { get; set; } = null;

        public string SortOrder { get; set; } = string.Empty;

        public string LeftURI
        {
            set {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("LeftURI"));
            }
            get
            {
                if (OtherSide == null) return "about:blank";

                if (System.IO.File.Exists($"TEMP\\{OtherSide.File}")) return $"TEMP\\{OtherSide.File}";

                foreach (var alt in OtherSide.Alternates)
                    if (System.IO.File.Exists($"TEMP\\{alt}")) return $"TEMP\\{alt}";

                return "about:blank";
            }
        }

        public string RightURI
        {
            get
            {
                if (System.IO.File.Exists($"TEMP\\{File}")) return $"TEMP\\{File}";

                foreach (var alt in Alternates)
                    if (System.IO.File.Exists($"TEMP\\{alt}")) return $"TEMP\\{alt}";

                return "about:blank";
            }
        }
    }
}