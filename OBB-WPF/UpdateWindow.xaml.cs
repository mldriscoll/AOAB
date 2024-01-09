using Core.Downloads;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async void Run()
        {
            Login? login = null;
            SeriesList list = null;
            using (var client = new HttpClient())
            {
                login = await Login.FromFile(client);

                if (login == null)
                {
                    var loginpage = new LoginWindow(client);
                    var success = loginpage.ShowDialog();
                    if (success.HasValue && success.Value)
                    {
                        login = await Login.FromFile(client);
                    }
                }

                if (login != null)
                {
                    list = await Downloader.GetSeriesList(client, login.AccessToken);
                }
            }

            if (list != null)
            {
                Progress.Maximum = list.series.Count();
                Progress.Width = list.series.Count();
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync(argument: new arg { list = list, login = login});
            }
        }

        private class arg
        {
            public SeriesList list { get; set; }
            public Login login { get; set; }
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            bool? finished = e.UserState as bool?;
            if (finished != null)
            {
                Close();
                return;
            }

            Progress.Value = e.ProgressPercentage;
            CurrentSeries.Text = (string)e.UserState;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var arg = e.Argument as arg;
            var login = arg.login;
            using (var client = new HttpClient())
            {
                if (login != null)
                {
                    var list = arg.list;
                    int c = 0;

                    foreach (var serie in list.series.Where(x => x.type.Equals("manga", StringComparison.InvariantCultureIgnoreCase)
                                                                || !x.title.Contains("ascendance of a bookworm", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        c++;
                        (sender as BackgroundWorker).ReportProgress(c, serie.title);
                        bool updated = false;
                        var series = MainWindow.Series.FirstOrDefault(x => x.ApiSlugs.Any(y => y.Slug.Equals(serie.slug, StringComparison.InvariantCultureIgnoreCase)));

                        var fullSeriesTask = Downloader.GetSeries(client, serie.slug);
                        fullSeriesTask.Wait();
                        var fullSeries = fullSeriesTask.Result;
                        if (series != null)
                        {
                            var order = 100 * (series.ApiSlugs.FirstOrDefault(x => x.Slug.Equals(serie.slug, StringComparison.InvariantCultureIgnoreCase))?.Order ?? 1);

                            series.Volumes.AddRange(fullSeries.volumes.Where(x => !series.Volumes.Any(y => y.ApiSlug.Equals(x.slug, StringComparison.OrdinalIgnoreCase))).ToList().Select(x => new VolumeName
                            {
                                ApiSlug = x.slug,
                                EditedBy = new List<string>(),
                                FileName = $"{x.slug}.epub",
                                Order = order + x.number,
                                Published = DateOnly.FromDateTime(DateTime.Parse(x.publishing)).ToString("yyyy-MM-dd")
                            }));
                            updated = true;
                        }
                        else if (fullSeries.volumes.Count > 1)
                        {
                            series = new Series
                            {
                                ApiSlugs = new List<SeriesSlug> { new SeriesSlug { Order = 1, Slug = serie.slug } },
                                Author = fullSeries.volumes.First().creators.First(x => x.role.Equals("AUTHOR")).name,
                                AuthorSort = fullSeries.volumes.First().creators.First(x => x.role.Equals("AUTHOR")).name.Split(' ').Reverse().Aggregate((str, agg) => string.Concat(str, ", ", agg)).Trim().ToUpper(),
                                InternalName = serie.slug,
                                Name = serie.title,
                                Volumes = fullSeries.volumes.Select(x => new VolumeName
                                {
                                    ApiSlug = x.slug ?? string.Empty,
                                    EditedBy = new List<string>(),
                                    FileName = $"{x.slug}.epub",
                                    Order = 100 + x.number,
                                    Published = DateOnly.FromDateTime(DateTime.Parse(x.publishing)).ToString("yyyy-MM-dd"),
                                    Title = x.title ?? string.Empty
                                }).ToList()
                            };

                            MainWindow.Series.Add(series);
                            updated = true;
                        }

                        if (updated)
                        {
                            MainWindow.Series = MainWindow.Series.OrderBy(x => x.Name).ToList();

#if DEBUG
                            var file = "..\\..\\..\\JSON\\Series.json";
#else
                            var file = "JSON\\Series.json";
#endif
                            var saveTask = JSON.Save(file, MainWindow.Series);
                            saveTask.Wait();
                        }
                    }
                }
            }
            (sender as BackgroundWorker).ReportProgress(0, true);
        }
    }
}
