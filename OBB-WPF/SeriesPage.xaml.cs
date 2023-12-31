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

            omnibus = new Omnibus();

            //Load Existing Omnibus

            foreach(var vol in series.Volumes)
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

            foreach (var chapter in omnibus.Chapters)
            {
                try
                {
                    var item = new TreeViewItem { Header = chapter.Name, Tag = chapter };
                    item.Selected += Chapter_Selected;
                    foreach (var subchapter in chapter.Chapters) AddItemChildren(item, subchapter);
                    ChapterList.Items.Add(item);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void Chapter_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)sender;
            var chapter = (Chapter)tvi.Tag;

            if (chapter != null)
            {
                ChapterName.Text = chapter.Name;
            }

            if (chapter.Sources.Any())
                Browser.Source = new Uri($"file://{Environment.CurrentDirectory}\\Temp\\{chapter.Sources[0].SourceBook}\\{chapter.Sources[0].File}");
            else
                Browser.Source = new Uri("about:blank");

            e.Handled = true;
        }

        private void AddItemChildren(TreeViewItem item, Chapter chapter)
        {
            var subItem = new TreeViewItem { Header = chapter.Name };
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
                Chapters = new List<Chapter>
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
                                    chapter.Sources.AddRange(chapterFiles.Select(x => new Source { File = "OEBPS\\text\\" + x, SourceBook = volumeName }));

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
                finalChapter.Sources.AddRange(chapterFiles.Select(x => new Source { File = "OEBPS\\text\\" + x, SourceBook = volumeName }));
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
                            chapter.Sources.AddRange(files.Select(x => new Source { File = "item\\xhtml\\" + x, SourceBook = volumeName }));
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
                                chapter.Sources.AddRange(files.Take(index).Select(x => new Source { File = "item\\xhtml\\" + x, SourceBook = volumeName }));
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

    public static class Translator
    {

    }
}
