using Core.Downloads;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using Microsoft.Win32;
using OBB_WPF.Library;
using System.IO;
using static Core.Downloads.LibraryResponse;
using System.Collections.Generic;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for DownloadWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        public DownloadWindow(List<Series> series)
        {
            InitializeComponent();
            Series = series;
        }

        List<Series> Series;
        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            bool? finished = e.UserState as bool?;
            if (finished != null)
            {
                Close();
                return;
            }

            Progress.Value = e.ProgressPercentage;
            CurrentBook.Text = (e.UserState as string)!;
        }

        private List<Book> BooksToDownload { get; set; } = new List<Book> { };

        public async void Run()
        {
            if (string.IsNullOrWhiteSpace(Settings.Configuration.SourceFolder))
            {
                var fp = new OpenFolderDialog();
                fp.Title = "Select the folder you store JNC downloads in";
                fp.ShowDialog();
                Settings.Configuration.SourceFolder = fp.FolderName;
                await JSON.Save("Configuration.json", Settings.Configuration);
            }

            var library = await Downloader.GetLibrary(new HttpClient(), Settings.Login!.AccessToken);
            BooksToDownload = new List<Book>();
            
            foreach (var book in library.books.Where(x => x.downloads.Any()))
            {
                if (Series.SelectMany(x => x.Volumes).Any(x => x.ApiSlug.Equals(book.volume.slug)))
                {
                    var filename = Settings.Configuration.SourceFolder + "\\" + book.volume.slug + ".epub";
                    if (!File.Exists(filename))
                    {
                        BooksToDownload.Add(book);
                    }
                }
            }

            Progress.Maximum = BooksToDownload.Count();
            //Progress.Width = BooksToDownload.Count();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (Settings.Login != null)
            {
                int c = 0;
                using (var client = new HttpClient())
                {
                    foreach (var book in BooksToDownload.OrderBy(x => x.volume.slug))
                    {
                        c++;
                        (sender as BackgroundWorker)!.ReportProgress(c, book.volume.slug);
                        var task = client.GetStreamAsync(book.downloads.Last().link);
                        task.Wait();
                        using (var stream = task.Result)
                        {
                            var filename = Settings.Configuration.SourceFolder + "\\" + book.volume.slug + ".epub";
                            using (var filestream = File.OpenWrite(filename))
                            {
                                stream.CopyTo(filestream);
                            }
                        }
                    }
                }
            }
            (sender as BackgroundWorker)!.ReportProgress(0, true);
        }
    }
}
