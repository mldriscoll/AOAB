using Core.Downloads;
using Core;
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
using Microsoft.Win32;

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
                if (string.IsNullOrWhiteSpace(Configuration.SourceFolder))
                {
                    var fp = new OpenFolderDialog();
                    fp.Title = "Select the folder you store JNC downloads in";
                    fp.ShowDialog();
                    Configuration.SourceFolder = fp.FolderName;
                    await JSON.Save("Configuration.json", Configuration);
                }

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
                var totalBooks = series.Volumes.Count;
                var availableBooks = series.Volumes.Where(x => File.Exists(Configuration.SourceFolder + "\\" + x.FileName)).ToList();
                var uneditedBooks = availableBooks.Where(x => !x.EditedBy.Any()).Count();
                var uebstring = uneditedBooks > 0 ? $" ({uneditedBooks} unedited)" : string.Empty;

                var button = new Button { Content = $"{series.Name} {totalBooks} books / {availableBooks.Count} available{uebstring}" };
                if (uneditedBooks > 0)
                {
                    button.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 50, 50));
                }
                button.Click += (object sender, RoutedEventArgs e) =>
                {
                    var window = new CreateOmnibus(series);
                    window.Show();
                };
                button.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        var window = new SeriesPage(series);
                        window.Show();
                    }
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
}