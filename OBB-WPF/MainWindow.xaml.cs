﻿using Core.Downloads;
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
            _ = Load(); // Waiting for a task in the WPF window constructor results in the window never fully loading. 
        }

        private async Task Load()
        {
            var config = JSON.Load<Configuration>("Configuration.json");
#if DEBUG
            var series = JSON.Load<List<Series>>($"..\\..\\..\\JSON\\Series.json");
            var custom = JSON.Load<List<Series>>("..\\..\\..\\JSON\\CustomSeries.json");
#else
            var series = JSON.Load<List<Series>>($"JSON\\Series.json");
            var custom = JSON.Load<List<Series>>("JSON\\CustomSeries.json");
#endif
            Settings.Configuration = await config ?? new Configuration();
            Series                 = await series ?? new List<Series>();
            CustomSeries           = await custom ?? new List<Series>();
            await Draw();
        }

        private async Task Draw()
        {
            Update.IsEnabled = false;
            AddCustom.IsEnabled = false;
            SeriesList.Items.Clear();
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
                var downloadPage = new DownloadWindow(Series);
                downloadPage.Run();
                downloadPage.ShowDialog();
            }

            foreach (var series in Series.Union(CustomSeries).OrderBy(x => x.Name).Where(x => x.Volumes.Any(y => File.Exists(Settings.Configuration.SourceFolder + "\\" + y.FileName) || File.Exists(y.FileName))))
            {
                var totalBooks = series.Volumes.Count;
                var availableBooks = series.Volumes.Where(x => File.Exists(Settings.Configuration.SourceFolder + "\\" + x.FileName)).ToList();
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

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var up = new UpdateWindow();
            up.Run();
            up.ShowDialog();
            Task t = Draw();
            t.Wait();
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
            Task t = Draw();
            t.Wait();
        }
    }
}