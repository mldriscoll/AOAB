using System;
using System.Collections.Generic;
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
using OBB_WPF.Library;
using OBB_WPF.Editor;

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

            if (series.CustomSeries)
            {
                var AddVolumeButton = new Button();
                AddVolumeButton.Content = "Add New Volume";
                AddVolumeButton.Click += AddVolumeButton_Click;
                Grid.SetRow(AddVolumeButton, 2);
                Grid.SetColumn(AddVolumeButton, 2);
                ((Grid)this.Content).Children.Add(AddVolumeButton);
            }

            Load();
        }

        private async void AddVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            var filePopup = new OpenFileDialog();
            filePopup.Multiselect = true;
            filePopup.CheckFileExists = true;
            filePopup.InitialDirectory = Settings.Configuration.SourceFolder;

            if (filePopup.ShowDialog() ?? false)
            {
                if (filePopup.FileNames.Count() > 0)
                {
                    foreach (var file in filePopup.FileNames)
                    {
                        var finfo = new FileInfo(file);
                        series.Volumes.Add(new Library.VolumeName
                        {
                            ApiSlug = finfo.Name,
                            FileName = file,
                            Title = finfo.Name,
                            Order = series.Volumes.Count() + 1
                        });

                        await Unpacker.Unpack(series);

                        var ob = Importer.GenerateVolumeInfo($"{omnibus!.Name}\\{finfo.Name}", omnibus.Name, finfo.Name, series.Volumes.Count());
                        omnibus.Combine(ob);
                    }
                }
            }
#if DEBUG
            await JSON.Save("..\\..\\..\\JSON\\CustomSeries.json", MainWindow.CustomSeries);
#else
                await JSON.Save("JSON\\CustomSeries.json", MainWindow.CustomSeries);
#endif
        }

        Omnibus? omnibus;

        private async Task Load()
        {
            await Unpacker.Unpack(series);


#if DEBUG
            omnibus = await JSON.Load<Omnibus>($"..\\..\\..\\JSON\\{series.InternalName}.json");
#else
            omnibus = await JSON.Load<Omnibus>($"JSON\\{series.InternalName}.json");
#endif
            if (omnibus == null)
            {
                omnibus = new Omnibus
                {
                    Name = series.Name,
                    Author = series.Author,
                    AuthorSort = series.AuthorSort
                };
            }

            omnibus.InternalName = series.InternalName;

            foreach (var vol in series.Volumes.Where(x => !x.EditedBy.Any()))
            {
                try
                {
                    var ob = Importer.GenerateVolumeInfo($"{omnibus.InternalName}\\{vol.ApiSlug}", omnibus.Name, vol.ApiSlug, vol.Order);
                    omnibus.Combine(ob);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
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
        public static DependencyObject? FindParent<T>(DependencyObject? dependencyObject)
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
                Dispatcher.BeginInvoke(() => DragDrop.DoDragDrop((DependencyObject)sender, sender, DragDropEffects.Move));
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
                    omnibus!.Remove(draggedChapter);
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
                var draggedSource = (((SourcePreview)lvi).DataContext as Source)!;
                CurrentChapter!.Sources.Remove(draggedSource);
                dropTarget.Sources.Add(draggedSource);
            }
            e.Handled = true;
        }

        Chapter? CurrentChapter = null;

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
            var page = new CreateOmnibus(omnibus!);
            page.ShowDialog();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Configuration.EditorName))
            {
                var window = new PickEditorNameWindow();
                window.ShowDialog();
            }
            
            var saveSeries = false;
            foreach (var a in series.Volumes)
            {
                if (!a.EditedBy.Any() || !a.EditedBy.Any(x => x.Equals(Settings.Configuration.EditorName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (File.Exists($"{Settings.Configuration.SourceFolder}\\{a.FileName}"))
                    {
                        a.EditedBy.Add(Settings.Configuration.EditorName!);
                        saveSeries = true;
                    }
                }
            }

            omnibus!.Sort();

#if DEBUG
            var omnibusFile = $"..\\..\\..\\JSON\\{omnibus.InternalName}.json";
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
                var draggedChapter = (((TreeViewItem)tvi).DataContext as Chapter)!;
                DeleteSources(draggedChapter);
                omnibus!.Remove(draggedChapter);

                omnibus.UnusedSources = new ObservableCollection<Source>(omnibus.UnusedSources.Where(x => !string.IsNullOrWhiteSpace(x.File)).OrderBy(x => x.File));
            }

            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = (((SourcePreview)lvi).DataContext as Source)!;
                CurrentChapter!.Sources.Remove(draggedSource);
                omnibus!.UnusedSources.Add(draggedSource);
            }
        }

        private void DeleteSources(Chapter chapter)
        {
            foreach(var source in chapter.Sources)
            {
                omnibus!.UnusedSources.Add(source);
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
                var draggedChapter = (((TreeViewItem)tvi).DataContext as Chapter)!;

                omnibus!.Remove(draggedChapter);
                omnibus.Chapters.Add(draggedChapter);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChapterName.DataContext = null;
            SortOrder.DataContext = null;
            Sources.ItemsSource = omnibus!.UnusedSources;
        }

        private void CoverButton_Drop(object sender, DragEventArgs e)
        {
            var lvi = e.Data.GetData(typeof(SourcePreview));
            if (lvi != null)
            {
                var draggedSource = (((SourcePreview)lvi).DataContext as Source)!;
                CurrentChapter!.Sources.Remove(draggedSource);

                if (omnibus!.Cover != null) omnibus.UnusedSources.Add(omnibus.Cover);
                omnibus.Cover = draggedSource;
            }
        }

        private void CoverButton_Click(object sender, RoutedEventArgs e)
        {
            var chapter = new Chapter
            {
                Sources = new ObservableCollection<Source>(new List<Source> { omnibus!.Cover! })
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
                var draggedSource = (((SourcePreview)lvi).DataContext as Source)!;

                if (draggedSource != target)
                {
                    var popup = new CombineSources(target!, draggedSource);
                    var result = popup.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        CurrentChapter!.Sources.Remove(draggedSource);
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
                var sortOrder = omnibus!.Chapters.Any() ? omnibus.Chapters.OrderByDescending(x => x.SortOrder).First().SortOrder + "x" : "001";
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
            if (CurrentChapter == null) return;
            var sc = new SplitChapter(CurrentChapter);
            sc.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CurrentChapter == null) return;
            var linkWindow = new CreateLink(CurrentChapter);
            linkWindow.Show();
        }

        private async void ImportMapping_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON|*.json";
            var picked = openFileDialog.ShowDialog();

            if (picked == true) {
                await ImportOld.Import(omnibus!, openFileDialog.FileName);
            }
            ChapterList.ItemsSource = omnibus!.Chapters;
        }

        private void ViewSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var source = (button.DataContext as Source)!;
            var window = new ViewSource(source);
            window.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var window = new SummaryPage(omnibus!);
            window.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            RedoSortOrders(string.Empty, omnibus!);
        }

        private void RedoSortOrders(string prefix, ChapterHolder holder)
        {
            int i = 1;
            var chapters = holder.Chapters.OrderBy(x => x.SortOrder).ToList();

            foreach (var chapter in chapters)
            {
                holder.Chapters.Remove(chapter);
                var order = $"{prefix}{i:000}";
                chapter.SortOrder = order;
                RedoSortOrders(order, chapter);
                holder.Chapters.Add(chapter);
                i++;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (CurrentChapter == null) return;
            var subs = new Subsections(CurrentChapter);
            subs.ShowDialog();
        }
    }

    public class ChapterTreeViewItem : TreeViewItem
    {
        public Chapter Chapter { get; set; } = new Chapter();
    }
}
