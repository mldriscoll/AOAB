using Core.Downloads;
using Core;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using OBB_WPF.Library;
using OBB_WPF.Custom;

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

        private async void Load()
        {
            Configuration = (await JSON.Load<Configuration>("Configuration.json")) ?? new Configuration();
#if DEBUG
            Series = await JSON.Load<List<Series>>($"..\\..\\..\\JSON\\Series.json") ?? new List<Series>();
            CustomSeries = await JSON.Load<List<Series>>("..\\..\\..\\JSON\\CustomSeries.json") ?? new List<Series>();
#else
            Series = await JSON.Load<List<Series>>($"JSON\\Series.json") ?? new List<Series>();
            CustomSeries = await JSON.Load<List<Series>>("JSON\\CustomSeries.json") ?? new List<Series>();
#endif
            Draw();
        }

        private async void Draw()
        {
            Update.IsEnabled = false;
            AddCustom.IsEnabled = false;
            SeriesList.Items.Clear();
            SeriesList list = null;
            if (Settings.Login == null)
            {
                using (var client = new HttpClient())
                {
                    Settings.Login = await Login.FromFile(client);

                    while(Settings.Login == null)
                    {
                        var loginpage = new LoginWindow(client);
                        loginpage.ShowDialog();
                    }
                }
            }

            if (Settings.Login != null)
            {
                if (string.IsNullOrWhiteSpace(Configuration.SourceFolder))
                {
                    var fp = new OpenFolderDialog();
                    fp.Title = "Select the folder you store JNC downloads in";
                    fp.ShowDialog();
                    Configuration.SourceFolder = fp.FolderName;
                    await JSON.Save("Configuration.json", Configuration);
                }

                var library = await Downloader.GetLibrary(new HttpClient(), Settings.Login.AccessToken);

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

            foreach (var series in Series.Union(CustomSeries).OrderBy(x => x.Name).Where(x => x.Volumes.Any(y => File.Exists(Configuration.SourceFolder + "\\" + y.FileName) || File.Exists(y.FileName))))
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
            AddCustom.IsEnabled = true;
        }

        public static List<Series> Series = new List<Series>();
        public static List<Series> CustomSeries = new List<Series>();

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            var up = new UpdateWindow();
            up.Run();
            up.ShowDialog();
            Draw();
        }

        private void Summary_Click(object sender, RoutedEventArgs e)
        {
            var summary = new LibrarySummary(Series);
            summary.Show();
        }

        private void AddCustom_Click(object sender, RoutedEventArgs e)
        {
            var popup = new CreateCustomSeries();
            popup.ShowDialog();
            Draw();
        }
    }
}