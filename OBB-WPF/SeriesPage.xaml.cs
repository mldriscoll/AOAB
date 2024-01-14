using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
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
using System.Text.Json;
using System.Globalization;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using Microsoft.Win32;
using CefSharp;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for SeriesPage.xaml
    /// </summary>
    public partial class SeriesPage : Window
    {
        private readonly Series series;

        public SeriesPage(Series series)
        {
            InitializeComponent();
            this.series = series;
            Title = series.Name;

            Load();
        }

        Omnibus omnibus;

        private async Task Load()
        {
            await Unpacker.Unpack(series);

            omnibus = new Omnibus
            {
                Name = series.Name,
                Author = series.Author,
                AuthorSort = series.AuthorSort,
            };

#if DEBUG
            if (File.Exists($"..\\..\\..\\JSON\\{omnibus.Name}.json"))
            {
                using (var stream = File.OpenRead($"..\\..\\..\\JSON\\{omnibus.Name}.json"))
                {
                    omnibus = await JsonSerializer.DeserializeAsync<Omnibus>(stream);
                }
            }
#else
            if (File.Exists($"JSON\\{omnibus.Name}.json"))
            {
                using (var stream = File.OpenRead($"JSON\\{omnibus.Name}.json"))
                {
                    omnibus = await JsonSerializer.DeserializeAsync<Omnibus>(stream);
                }
            }
#endif

            foreach (var vol in series.Volumes.Where(x => !x.EditedBy.Any()))
            {
                try
                {
                    var ob = Importer.GenerateVolumeInfo($"{omnibus.Name}\\{vol.ApiSlug}", omnibus.Name, vol.ApiSlug, vol.Order);
                    omnibus.Combine(ob);
                }
                catch(Exception ex)
                {

                }
            }

            ChapterList.ItemsSource = omnibus.Chapters;

        }

        bool _IsDragging = false;
        bool _IsDraggingSource = false;
        private void Chapter_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging)
            {
                var tbSource = e.OriginalSource as TextBlock;
                if (tbSource != null)
                {
                    _IsDragging = true;
                    DragDrop.DoDragDrop((DependencyObject)sender, FindParent<TreeViewItem>(tbSource), DragDropEffects.Move);
                }
            }
        }
        public static DependencyObject FindParent<T>(DependencyObject dependencyObject)
        {
            while (dependencyObject != null && typeof(T) != dependencyObject.GetType())
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);

            return dependencyObject;
        }

        private void DragSource_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_IsDraggingSource)
            {
                _IsDraggingSource = true;
                DragDrop.DoDragDrop((DependencyObject)sender, sender, DragDropEffects.Move);
            }
        }

        private void DropOnChapter(object sender, DragEventArgs e)
        {
            var dropTarget = (Chapter)((TreeViewItem)sender).Tag;
            var tvi = e.Data.GetData(typeof(TreeViewItem));
            if (tvi != null)
            {
                var draggedChapter = ((TreeViewItem)tvi).DataContext as Chapter;

                if (draggedChapter != null && draggedChapter != dropTarget)
                {
                    omnibus.Remove(draggedChapter);
                    dropTarget.Chapters.Add(draggedChapter);
                    var chapters = dropTarget.Chapters.ToList();
                    chapters.Add(draggedChapter);
                    foreach(var c in chapters.OrderBy(x => x.SortOrder))
                    {
                        dropTarget.Chapters.Remove(c);
                        dropTarget.Chapters.Add(c);
                    }
                }
            }

            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = ((SourcePreview)lvi).DataContext as Source;
                CurrentChapter.Sources.Remove(draggedSource);
                dropTarget.Sources.Add(draggedSource);
            }
            e.Handled = true;
        }

        Chapter CurrentChapter = null;

        private void Chapter_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)sender;
            CurrentChapter = (Chapter)tvi.Tag;

            if (CurrentChapter != null)
            {
                ChapterName.DataContext = CurrentChapter;
                SortOrder.DataContext = CurrentChapter;
                Sources.ItemsSource = CurrentChapter.Sources;
                ChapterType.DataContext = CurrentChapter;
                StartLine.DataContext = CurrentChapter;
                EndLine.DataContext = CurrentChapter;
            }

            e.Handled = true;
        }

        private void AddItemChildren(TreeViewItem item, Chapter chapter)
        {
            var subItem = new ChapterTreeViewItem { Header = chapter.Name, Chapter = chapter };
            subItem.Tag = chapter;
            subItem.Selected += Chapter_Selected;
            foreach (var subchapter in chapter.Chapters) AddItemChildren(subItem, subchapter);
            item.Items.Add(subItem);
        }

        private async void BuildOmnibus(object sender, RoutedEventArgs e)
        {
            var page = new CreateOmnibus(omnibus);
            page.ShowDialog();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MainWindow.Configuration.EditorName))
            {
                var window = new PickEditorNameWindow();
                window.ShowDialog();
            }
            
            var saveSeries = false;
            foreach (var a in series.Volumes)
            {
                if (!a.EditedBy.Any() || !a.EditedBy.Any(x => x.Equals(MainWindow.Configuration.EditorName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (File.Exists($"{MainWindow.Configuration.SourceFolder}\\{a.FileName}"))
                    {
                        a.EditedBy.Add(MainWindow.Configuration.EditorName);
                        saveSeries = true;
                    }
                }
            }

            omnibus.Sort();

#if DEBUG
            var omnibusFile = $"..\\..\\..\\JSON\\{omnibus.Name}.json";
            var seriesFile = $"..\\..\\..\\JSON\\Series.json";
#else
            var omnibusFile = $"JSON\\{omnibus.Name}.json";
            var seriesFile = $"JSON\\Series.json";
#endif
            await JSON.Save(omnibusFile, omnibus);
            if (saveSeries)
            {
                await JSON.Save(seriesFile, MainWindow.Series);
            }
        }

        private void DragChapter_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsDragging = false;
        }

        private void DragSource_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsDraggingSource = false;
        }

        private void DragChapter_MouseLeave(object sender, MouseEventArgs e)
        {
            _IsDragging = false;
        }

        private void DragSource_MouseLeave(object sender, MouseEventArgs e)
        {
            _IsDraggingSource = false;
        }

        private void Delete_Button_Drop(object sender, DragEventArgs e)
        {
            var tvi = e.Data.GetData(typeof(TreeViewItem));
            if (tvi != null)
            {
                var draggedChapter = ((TreeViewItem)tvi).DataContext as Chapter;
                DeleteSources(draggedChapter);
                omnibus.Remove(draggedChapter);

                omnibus.UnusedSources = new ObservableCollection<Source>(omnibus.UnusedSources.OrderBy(x => x.File));
            }

            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = ((SourcePreview)lvi).DataContext as Source;
                CurrentChapter.Sources.Remove(draggedSource);
                omnibus.UnusedSources.Add(draggedSource);
            }
        }

        private void DeleteSources(Chapter chapter)
        {
            foreach(var source in chapter.Sources)
            {
                omnibus.UnusedSources.Add(source);
            }

            foreach(var subChapter in chapter.Chapters)
            {
                DeleteSources(subChapter);
            }
        }

        private void Root_Drop(object sender, DragEventArgs e)
        {
            var tvi = e.Data.GetData(typeof(TreeViewItem));
            if (tvi != null)
            {
                var draggedChapter = ((TreeViewItem)tvi).DataContext as Chapter;

                omnibus.Remove(draggedChapter);
                omnibus.Chapters.Add(draggedChapter);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChapterName.DataContext = null;
            SortOrder.DataContext = null;
            Sources.ItemsSource = omnibus.UnusedSources;
        }

        private void CoverButton_Drop(object sender, DragEventArgs e)
        {
            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = ((SourcePreview)lvi).DataContext as Source;
                CurrentChapter.Sources.Remove(draggedSource);

                if (omnibus.Cover != null) omnibus.UnusedSources.Add(omnibus.Cover);
                omnibus.Cover = draggedSource;
            }
        }

        private void CoverButton_Click(object sender, RoutedEventArgs e)
        {
            var chapter = new Chapter
            {
                Sources = new ObservableCollection<Source>(new List<Source> { omnibus.Cover })
            };

            CurrentChapter = chapter;

            ChapterName.DataContext = CurrentChapter;
            SortOrder.DataContext = CurrentChapter;
            Sources.ItemsSource = CurrentChapter.Sources;
            ChapterType.DataContext = CurrentChapter;
        }

        private void Source_Drop(object sender, DragEventArgs e)
        {
            var target = ((SourcePreview)sender).DataContext as Source;
            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = ((SourcePreview)lvi).DataContext as Source;

                if (draggedSource != target)
                {
                    var popup = new CombineSources(target, draggedSource);
                    var result = popup.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        CurrentChapter.Sources.Remove(draggedSource);
                    }
                }
            }
        }

        private void NewChapter_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentChapter != null)
            {
                var sortOrder = CurrentChapter.Chapters.Any() ? CurrentChapter.Chapters.OrderByDescending(x => x.SortOrder).First().SortOrder + "x" : CurrentChapter.SortOrder + "001";
                var chapter = new Chapter
                {
                    CType = CurrentChapter.CType,
                    Name = "New Chapter",
                    SortOrder = sortOrder
                };
                CurrentChapter.Chapters.Add(chapter);
            }
            else
            {
                var sortOrder = omnibus.Chapters.Any() ? omnibus.Chapters.OrderByDescending(x => x.SortOrder).First().SortOrder + "x" : "001";
                var chapter = new Chapter
                {
                    CType = Chapter.ChapterType.Story,
                    Name = "New Chapter",
                    SortOrder = sortOrder
                };
                omnibus.Chapters.Add(chapter);
            }
        }

        private void SplitChapter_Click(object sender, RoutedEventArgs e)
        {
            var sc = new SplitChapter(CurrentChapter);
            sc.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var linkWindow = new CreateLink(CurrentChapter);
            linkWindow.Show();
        }

        private async void ImportMapping_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON|*.json";
            var picked = openFileDialog.ShowDialog();

            if (picked == true) {
                await ImportOld.Import(omnibus, openFileDialog.FileName);
            }
            ChapterList.ItemsSource = omnibus.Chapters;
        }

        private void ViewSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var source = button.DataContext as Source;
            var window = new ViewSource(source);
            window.Show();
        }
    }

    public class ChapterTreeViewItem : TreeViewItem
    {
        public Chapter Chapter { get; set; }
    }

    public static class Unpacker
    {
        public static Task Unpack(Series series)
        {
            if (!Directory.Exists(series.Name)) Directory.CreateDirectory(series.Name);

            foreach (var volume in series.Volumes.Where(x => File.Exists($"{MainWindow.Configuration.SourceFolder}\\{x.FileName}")))
            {
                var directory = $"{series.Name}\\{volume.ApiSlug}";
                if (!Directory.Exists(directory)) ZipFile.ExtractToDirectory($"{MainWindow.Configuration.SourceFolder}\\{volume.FileName}", directory);
                else
                {
                    var finfo = new FileInfo($"{MainWindow.Configuration.SourceFolder}\\{volume.FileName}");
                    var dinfo = new DirectoryInfo(directory);
                    if (dinfo.CreationTimeUtc < finfo.CreationTimeUtc)
                    {
                        Directory.Delete(directory);
                        ZipFile.ExtractToDirectory($"{MainWindow.Configuration.SourceFolder}\\{volume.FileName}", directory);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
