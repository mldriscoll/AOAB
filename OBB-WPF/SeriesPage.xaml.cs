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
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Globalization;
using System.Windows.Markup;
using System.Collections.ObjectModel;

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
                Name = series.Name
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
                    var ob = Importer.GenerateVolumeInfo($"Temp\\{vol.ApiSlug}", vol.ApiSlug, vol.Order);
                    omnibus.Combine(ob);
                }
                catch(Exception ex)
                {

                }
            }

            ChapterList.ItemsSource = omnibus.Chapters;

        }

        bool _IsDragging = false;
        private void Chapter_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging)
            {
                _IsDragging = true;
                DragDrop.DoDragDrop((DependencyObject)sender, sender, DragDropEffects.Move);
            }
        }

        private void DropOnChapter(object sender, DragEventArgs e)
        {
            var dropTarget = (Chapter)((TreeViewItem)sender).Tag;
            var draggedChapter = ((TextBlock)e.Data.GetData(typeof(TextBlock))).DataContext as Chapter;

            if (draggedChapter != null && draggedChapter != dropTarget)
            {
                omnibus.Remove(draggedChapter);
                dropTarget.Chapters.Add(draggedChapter);
                dropTarget.Chapters = new ObservableCollection<Chapter>(dropTarget.Chapters.OrderBy(x => x.SortOrder));
            }
            e.Handled = true;
        }

        private void Chapter_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)sender;
            var chapter = (Chapter)tvi.Tag;

            if (chapter != null)
            {
                ChapterName.DataContext = chapter;
                SortOrder.DataContext = chapter;
                DragChapter.DataContext = chapter;
            }

            if (chapter.Sources.Any())
                Browser.Source = new Uri($"file://{Environment.CurrentDirectory}\\Temp\\{chapter.Sources[0].File}");
            else
                Browser.Source = new Uri("about:blank");

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
            page.Show();
            this.Hide();
            await page.Start();
            this.Show();
            page.Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Editor.Text))
            {
                MessageBox.Show(this, "Please enter an editor name before saving changes");
            }
            else
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var saveSeries = false;
                foreach (var a in series.Volumes)
                {
                    if (!a.EditedBy.Any() || !a.EditedBy.Any(x => x.Equals(Editor.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (File.Exists($"{Configuration.SourceFolder}\\{a.FileName}"))
                        {
                            a.EditedBy.Add(Editor.Text);
                            saveSeries = true;
                        }
                    }
                }

#if DEBUG
                using (var stream = File.OpenWrite($"..\\..\\..\\JSON\\{omnibus.Name}.json"))
                {
                    await JsonSerializer.SerializeAsync(stream, omnibus, options: options);
                }

                if (saveSeries)
                {
                    using (var stream = File.OpenWrite($"..\\..\\..\\JSON\\Series.json"))
                    {
                        await JsonSerializer.SerializeAsync(stream, MainWindow.Series, options: options);
                    }
                }
#else
                using (var stream = File.OpenWrite($"JSON\\{omnibus.Name}.json"))
                {
                    await JsonSerializer.SerializeAsync(stream, omnibus, options: options);
                }

                if (saveSeries)
                {
                    using (var stream = File.OpenWrite($"JSON\\Series.json"))
                    {
                        await JsonSerializer.SerializeAsync(stream, MainWindow.Series, options: options);
                    }
                }
#endif
            }
        }

        private void DragChapter_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsDragging = false;
        }

        private void DragChapter_MouseLeave(object sender, MouseEventArgs e)
        {
            _IsDragging=false;
        }
    }

    public class ChapterTreeViewItem : TreeViewItem
    {
        public Chapter Chapter { get; set; }
    }

    public static class Configuration
    {
        public static string SourceFolder { get; set; } = "D:\\JNC\\Raw";
    }

    public static class Unpacker
    {
        public static Task Unpack(Series series)
        {
            if (Directory.Exists("Temp")) Directory.Delete("Temp", true);

            Directory.CreateDirectory("Temp");

            foreach (var volume in series.Volumes.Where(x => File.Exists($"{Configuration.SourceFolder}\\{x.FileName}")))
            {
                ZipFile.ExtractToDirectory($"{Configuration.SourceFolder}\\{volume.FileName}", $"Temp\\{volume.ApiSlug}");
            }
            return Task.CompletedTask;
        }
    }

    public static class Importer
    {
        static Regex ItemRefRegex = new Regex("\".*?\"");
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");
        private static readonly Regex chapterSubTitleRegex = new Regex("<h2>[\\s\\S]*?<\\/h2>");
        public static Omnibus GenerateVolumeInfo(string inFolder, string? volumeName, int volOrder)
        {
            bool inSpine = false;
            List<string> chapterFiles = new List<string>();

            var ob = new Omnibus
            {
                Chapters = new System.Collections.ObjectModel.ObservableCollection<Chapter>
                {
                    new Chapter
                    {
                        Name = volumeName,
                        SortOrder = volOrder.ToString("000")
                    }
                }
            };


            int order = 1;
            try
            {
                var content = File.ReadAllLines($"{inFolder}\\OEBPS\\content.opf");
                var imageFiles = Directory.GetFiles($"{inFolder}\\OEBPS\\Images", "*.jpg").Select(x => x.Replace($"{inFolder}\\OEBPS\\Images\\", string.Empty).Replace(".jpg", string.Empty).ToLower()).ToList();

                foreach (var line in content)
                {
                    if (inSpine)
                    {
                        if (line.Contains("</spine"))
                        {
                            inSpine = false;
                        }
                        else
                        {
                            var match = ItemRefRegex.Match(line).Value;

                            if (!match.Contains(".xhtml")
                                || match.StartsWith("\"signup")
                                || match.StartsWith("\"copyright")
                                || match.StartsWith("\"frontmatter")
                                )
                            {

                            }
                            else if (match.Contains('_') || match.StartsWith("\"insert"))
                            {
                                chapterFiles.Add(match.Replace("\"", ""));
                            }
                            else
                            {
                                if (chapterFiles.Any())
                                {
                                    var chapter = new Chapter();
                                    chapter.SortOrder = ((volOrder * 100) + order).ToString("00000");
                                    order++;
                                    chapter.Sources.AddRange(chapterFiles.Select(x => new Source { File = $"{volumeName}\\OEBPS\\text\\{x}" }));

                                    var chapterContent = File.ReadAllText($"{inFolder}\\OEBPS\\text\\" + chapterFiles[0]);
                                    chapter.Name = chapterTitleRegex.Match(chapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);

                                    //int subSection = 1;
                                    //foreach (Match subHeader in chapterSubTitleRegex.Matches(chapterContent))
                                    //{
                                    //    chapter.Splits.Add(new ChapterSplit
                                    //    {
                                    //        Name = subHeader.Value.Replace("<h2>", "").Replace("</h2>", ""),
                                    //        SplitLine = subHeader.Value,
                                    //        SortOrder = subSection.ToString("00"),
                                    //        SubFolder = string.Empty
                                    //    });
                                    //    subSection++;
                                    //}

                                    AddChapter(chapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

                                }
                                chapterFiles.Clear();
                                chapterFiles.Add(match.Replace("\"", ""));
                            }
                        }
                    }

                    if (line.Contains("<spine"))
                    {
                        inSpine = true;
                    }
                }

                var finalChapter = new Chapter { SortOrder = ((volOrder * 100) + order).ToString("00000") };
                finalChapter.Sources.AddRange(chapterFiles.Select(x => new Source { File = $"{volumeName}\\OEBPS\\text\\{x}" }));
                var finalChapterContent = File.ReadAllText($"{inFolder}\\OEBPS\\text\\" + chapterFiles[0]);
                finalChapter.Name = chapterTitleRegex.Match(finalChapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                AddChapter(finalChapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

                foreach (var file in imageFiles)
                {
                    //if (file.Contains("insert"))
                    //{
                    //    AddImage(file, volume.Gallery[0].ChapterImages);
                    //}
                    //else
                    //{
                    //    AddImage(file, volume.Gallery[0].SplashImages);
                    //}
                }
                //volume.Gallery[0].SubFolder = $"{volOrder}-{volumeName}";
            }
            // Backup for Manga that don't have content files
            catch (DirectoryNotFoundException noContent)
            {
                var nav = File.ReadAllLines($"{inFolder}\\item\\nav.xhtml").Select(x => x.Trim()).ToList();
                var files = Directory.GetFiles($"{inFolder}\\item\\xhtml\\", "*.xhtml").Select(x => x.Replace($"{inFolder}\\item\\xhtml\\", string.Empty)).ToList();

                var incontents = false;
                Chapter chapter = null;
                foreach (var line in nav)
                {
                    if (line.Equals("<h1>Table of Contents</h1>", StringComparison.InvariantCultureIgnoreCase))
                    {
                        incontents = true;
                    }
                    else if (incontents)
                    {
                        if (line.Equals("</ol>", StringComparison.InvariantCultureIgnoreCase))
                        {
                            incontents = false;
                            chapter.Sources.AddRange(files.Select(x => new Source { File = $"{volumeName}\\item\\xhtml\\{x}" }));
                        }
                        else if (line.Equals("<ol>", StringComparison.InvariantCultureIgnoreCase))
                        {
                        }
                        else //Chapter Border
                        {

                            var linesplit = line.Split('"');
                            var firstPage = linesplit[1].Replace("xhtml/", string.Empty);

                            if (chapter != null)
                            {
                                var index = files.IndexOf(firstPage);
                                chapter.Sources.AddRange(files.Take(index).Select(x => new Source {File = $"{volumeName}\\item\\xhtml\\{x}" }));
                                files.RemoveRange(0, index);
                            }

                            var title = linesplit[2].Substring(1).Replace("</a></li>", string.Empty);
                            chapter = new Chapter
                            {
                                Name = title,
                            };
                            chapter.SortOrder = ((volOrder * 100) + order).ToString("00000");
                            order++;
                            ob.Chapters[0].Chapters.Add(chapter);
                        }
                    }
                }
            }

            return ob;
        }

        private static void AddChapter(Chapter chapter, Chapter parentChapter, string volumeName, int volumeSortOrder, List<string> imageFiles)
        {
            // No need to include the old contents pages or chapters that only include images
            if (chapter.Sources.All(x => x.File.Equals("tocimg", StringComparison.InvariantCultureIgnoreCase)
                || x.File.Equals("toc", StringComparison.InvariantCultureIgnoreCase)
                || imageFiles.Any(y => y.Equals(x.File, StringComparison.InvariantCultureIgnoreCase))))
            {

            }
            else if (chapter.Sources.Any(x => x.File.StartsWith("side")
                                    || x.File.StartsWith("interlude")
                                    || x.File.StartsWith("extra")
                                    || x.File.StartsWith("bonus")
                                    || x.File.StartsWith("diary")
                                    ))
            {
                //vol.BonusChapters[0].Chapters.Add(chapter);
            }
            else if (chapter.Sources.Any(x => x.File.StartsWith("afterword")))
            {
                //chapter.SubFolder = "999-Afterwords";
                //chapter.SortOrder = volumeSortOrder.ToString("000");
                //chapter.ChapterName = volumeName;
                //vol.ExtraContent.Add(chapter);
            }
            else if (chapter.Sources.Any(x => x.File.StartsWith("character")))
            {
                //chapter.ChapterName = "Character Sheet";
                //vol.ExtraContent[0].Chapters.Add(chapter);
            }
            else if (chapter.Sources.Any(x => x.File.StartsWith("map")))
            {
                //chapter.ChapterName = "Map";
                //chapter.SubFolder = "998-Maps";
                //vol.ExtraContent.Add(chapter);
            }
            parentChapter.Chapters.Add(chapter);
        }
    }
}
