using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

        private async void Load()
        {
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

            foreach (var series in Series)
            {
                var button = new Button { Content = series.Name };
                button.Click += (object sender, RoutedEventArgs e) => {
                    var window = new SeriesPage(series);
                    window.Show();
                };
                SeriesList.Items.Add(button);
            }
        }

        public static List<Series> Series = new List<Series>
        {
            new Series
            {
                Name = "Lady Rose Just Wants to Be a Commoner!",
                InternalName = "lady-rose-just-wants-to-be-a-commoner",
                ApiSlugs = new List<SeriesSlug>
                {
                    new SeriesSlug
                    {
                        Order = 1,
                        Slug = "lady-rose-just-wants-to-be-a-commoner"
                    }
                },
                Volumes = new List<VolumeName>
                {
                    new VolumeName
                    {
                        ApiSlug = "lady-rose-just-wants-to-be-a-commoner",
                        FileName = "lady-rose-just-wants-to-be-a-commoner.epub",
                        Title = "Lady Rose Just Wants to Be a Commoner! Volume 1",
                        Published = "2023-01-04",
                        Order = 101
                    },
                    new VolumeName
                    {
                        ApiSlug = "lady-rose-just-wants-to-be-a-commoner-2",
                        FileName = "lady-rose-just-wants-to-be-a-commoner-volume-2.epub",
                        Title = "Lady Rose Just Wants to Be a Commoner! Volume 2",
                        Published = "2023-03-22",
                        Order = 102
                    },
                    new VolumeName
                    {
                        ApiSlug = "lady-rose-just-wants-to-be-a-commoner-3",
                        FileName = "lady-rose-just-wants-to-be-a-commoner-volume-3.epub",
                        Title = "Lady Rose Just Wants to Be a Commoner! Volume 3",
                        Published = "2023-06-14",
                        Order = 103
                    },
                    new VolumeName
                    {
                        ApiSlug = "lady-rose-just-wants-to-be-a-commoner-4",
                        FileName = "lady-rose-just-wants-to-be-a-commoner-volume-4.epub",
                        Title = "Lady Rose Just Wants to Be a Commoner! Volume 4",
                        Published = "2023-08-30",
                        Order = 104
                    },
                    new VolumeName
                    {
                        ApiSlug = "lady-rose-just-wants-to-be-a-commoner-5",
                        FileName = "lady-rose-just-wants-to-be-a-commoner-volume-5.epub",
                        Title = "Lady Rose Just Wants to Be a Commoner! Volume 5",
                        Published = "2023-12-13",
                        Order = 105
                    }
                },
                Author = "Kooriame",
                AuthorSort = "KOORIAME"
            }
        };
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
        public bool ShowRemainingFiles { get; set; } = true;
        public string? Published { get; set; } = null;
        public int Order { get; set; }
    }

    public class Chapter : ChapterHolder, INotifyPropertyChanged
    {
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
    }

    public class Source
    {
        public string File { get; set; } = string.Empty;

        public ObservableCollection<string> Alternates { get; set; } = new ObservableCollection<string>();

        public Source OtherSide { get; set; } = null;

        public string SortOrder { get; set; } = string.Empty;
    }
}