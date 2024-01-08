using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Core.Processor;
using Point = SixLabors.ImageSharp.Point;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for CreateOmnibus.xaml
    /// </summary>
    public partial class CreateOmnibus : Window
    {
        private readonly Omnibus series;
        public bool ConfigCombineImages { get; set; } = true;
        public bool IncStoryChapters { get; set; } = true;
        public bool IncBonusChapters { get; set; } = true;
        public bool IncNonStoryChapters { get; set; } = true;
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*<\\/h1>");
        public bool UpdateChapterTitles { get; set; } = true;
        public string ImageWidth { get; set; }
        public string ImageHeight { get; set; }
        public string ImageQuality { get; set; }

        public CreateOmnibus(Omnibus series)
        {
            InitializeComponent();
            this.series = series;
            IncStoryChapters = MainWindow.Configuration.IncludeNormalChapters;
            IncBonusChapters = MainWindow.Configuration.IncludeExtraChapters;
            IncNonStoryChapters = MainWindow.Configuration.IncludeNonStoryChapters;
            ConfigCombineImages = MainWindow.Configuration.CombineMangaSplashPages;
            UpdateChapterTitles = MainWindow.Configuration.UpdateChapterTitles;
            if (MainWindow.Configuration.MaxImageWidth.HasValue) ImageWidth = MainWindow.Configuration.MaxImageWidth.Value.ToString();
            if (MainWindow.Configuration.MaxImageHeight.HasValue) ImageWidth = MainWindow.Configuration.MaxImageHeight.Value.ToString();
            ImageQuality = MainWindow.Configuration.ResizedImageQuality.ToString();
            DataContext = this;
        }

        public async Task Start()
        {
            //var inFolder = Settings.MiscSettings.InputFolder == null ? Environment.CurrentDirectory :
            //        Settings.MiscSettings.InputFolder.Length > 1 && Settings.MiscSettings.InputFolder[1].Equals(':') ? Settings.MiscSettings.InputFolder : Environment.CurrentDirectory + "\\" + Settings.MiscSettings.InputFolder;
            var inFolder = @"Temp";

            if (!Directory.Exists(inFolder)) Directory.CreateDirectory(inFolder);

            //if (Settings.MiscSettings.DownloadBooks)
            //{
            //    using (var client = new HttpClient())
            //    {
            //        if (Login != null)
            //        {
            //            await Downloader.DoDownloads(client, Login.AccessToken, inFolder, selection.Volumes.Select(x => new Name { ApiSlug = x.ApiSlug, FileName = x.FileName }));
            //        }
            //    }
            //}


            //var outputFolder = Settings.MiscSettings.OutputFolder == null ? Environment.CurrentDirectory :
            //    Settings.MiscSettings.OutputFolder.Length > 1 && Settings.MiscSettings.OutputFolder[1].Equals(':') ? Settings.MiscSettings.OutputFolder : Environment.CurrentDirectory + "\\" + Settings.MiscSettings.OutputFolder;
            var outputFolder = "D:\\a\\";

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var inProcessor = new Processor()
            {
                DisableHyphenProcessing = true
            };
            var outProcessor = new Processor();
            inProcessor.UnpackFolder($"{inFolder}");
            outProcessor.UnpackFolder($"{inFolder}");
            outProcessor.Chapters.Clear();

            if (series.Cover != null)
            {
                var entry = inProcessor.Chapters.First(x => (x.SubFolder + "\\" + x.Name + ".xhtml").Equals(series.Cover.File, StringComparison.InvariantCultureIgnoreCase));
                var imageRegex = new Regex("\\[ImageFolder\\]\\/[0-9]*?\\.jpg");
                var irMatch = imageRegex.Match(entry.Contents);
                var cim = inProcessor.Images.FirstOrDefault(x => x.Name.Equals(irMatch.Value.Replace("[ImageFolder]/", "")));
                if (File.Exists("cover.jpg")) File.Delete("cover.jpg");
                File.Copy(cim.OldLocation, "cover.jpg");
                outProcessor.Metadata.Add("<meta name=\"cover\" content=\"images/cover.jpg\" />");
                outProcessor.Images.Add(new Core.Processor.Image { Name = "cover.jpg", Referenced = true, OldLocation = "cover.jpg" });
                var coverContents = File.ReadAllText("Reference\\cover.txt");
                outProcessor.Chapters.Add(new Core.Processor.Chapter { Contents = coverContents, Name = "Cover.xhtml", SortOrder = "0000", SubFolder = "" });
            }

            foreach (var chapter in series.Chapters)
            {
                await ProcessChapter(chapter, inProcessor, outProcessor, string.Empty);
            }

            if (File.Exists($"{series.Name}.epub")) File.Delete($"{series.Name}.epub");

            outProcessor.Metadata.Add(@$"<dc:title>{series.Name}</dc:title>");
            outProcessor.Metadata.Add($"<dc:creator id=\"creator01\">{series.Author}</dc:creator>");
            outProcessor.Metadata.Add("<meta property=\"display-seq\" refines=\"#creator01\">1</meta>");
            outProcessor.Metadata.Add($"<meta property=\"file-as\" refines=\"#creator01\">{series.AuthorSort}</meta>");
            outProcessor.Metadata.Add("<meta property=\"role\" refines=\"#creator01\" scheme=\"marc:relators\">aut</meta>");
            outProcessor.Metadata.Add("<dc:language>en</dc:language>");
            outProcessor.Metadata.Add("<dc:publisher>J-Novel Club</dc:publisher>");
            outProcessor.Metadata.Add("<dc:identifier id=\"pub-id\">1</dc:identifier>");
            outProcessor.Metadata.Add($"<meta property=\"dcterms:modified\">{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ")}</meta>");

            if (int.TryParse(ImageWidth, out var width))
            {
                MainWindow.Configuration.MaxImageWidth = width;
            }
            else
            {
                MainWindow.Configuration.MaxImageWidth = null;
            }
            
            if (int.TryParse(ImageHeight, out var height))
            {
                MainWindow.Configuration.MaxImageHeight = height;
            }
            else
            {
                MainWindow.Configuration.MaxImageHeight = null;
            }
            
            if (int.TryParse(ImageQuality, out var quality))
            {
                MainWindow.Configuration.ResizedImageQuality = quality;
            }

            await outProcessor.FullOutput(outputFolder,
                false,
                false,
                false,
                series.Name,
                MainWindow.Configuration.MaxImageWidth,
                MainWindow.Configuration.MaxImageHeight,
                MainWindow.Configuration.ResizedImageQuality);

            MainWindow.Configuration.IncludeNormalChapters = IncStoryChapters;
            MainWindow.Configuration.IncludeExtraChapters = IncBonusChapters;
            MainWindow.Configuration.IncludeNonStoryChapters = IncNonStoryChapters;
            MainWindow.Configuration.CombineMangaSplashPages = ConfigCombineImages;
            MainWindow.Configuration.UpdateChapterTitles = UpdateChapterTitles;
            await JSON.Save("Configuration.JSON", MainWindow.Configuration);
        }

        private async Task ProcessChapter(Chapter chapter, Processor inProcessor, Processor outProcessor, string subfolder)
        {
            try
            {

                if (((chapter.CType == Chapter.ChapterType.Bonus && IncBonusChapters)
                    || (chapter.CType == Chapter.ChapterType.NonStory && IncNonStoryChapters)
                    || (chapter.CType == Chapter.ChapterType.Story && IncStoryChapters))
                    && chapter.Sources.Any())
                {
                    bool notFirst = false;
                    var newChapter = new Core.Processor.Chapter
                    {
                        Contents = string.Empty,
                        CssFiles = new List<string>(),
                        Name = chapter.Name + ".xhtml",
                        SubFolder = subfolder,
                        SortOrder = chapter.SortOrder,
                        ChapterLinks = chapter.LinkedChapters.Select(x => string.Concat(x.Target.SortOrder + "-" + x.Target.Name + ".xhtml")).ToList()
                    };

                    newChapter.SortOrder = chapter.SortOrder;

                    if (ConfigCombineImages)
                    {
                        foreach (var splash in chapter.Sources.Where(x => x.OtherSide != null))
                        {
                            var one = inProcessor.Chapters.First(x => (x.SubFolder + "\\" + x.Name + ".xhtml").Equals(splash.File, StringComparison.InvariantCultureIgnoreCase));
                            var imR = inProcessor.Images.FirstOrDefault(x => one.Contents.Contains(x.Name));

                            var two = inProcessor.Chapters.First(x => (x.SubFolder + "\\" + x.Name + ".xhtml").Equals(splash.OtherSide.File, StringComparison.InvariantCultureIgnoreCase));
                            var imL = inProcessor.Images.FirstOrDefault(x => two.Contents.Contains(x.Name));

                            var right = await SixLabors.ImageSharp.Image.LoadAsync(imR.OldLocation);
                            var left = await SixLabors.ImageSharp.Image.LoadAsync(imL.OldLocation);

                            var outputImage = new Image<Rgba32>(right.Width + left.Width, right.Height);
                            outputImage.Mutate(x => x
                                .DrawImage(left, new Point(0, 0), 1f)
                                .DrawImage(right, new Point(left.Width, 0), 1f)
                                );

                            await outputImage.SaveAsJpegAsync(imR.OldLocation);

                            var widthRegex = new Regex("width=\"\\d*\"");
                            one.Contents = widthRegex.Replace(one.Contents, string.Empty);
                            var viewBoxRegex = new Regex("viewBox=\"[\\d ]*\"");
                            one.Contents = viewBoxRegex.Replace(one.Contents, $"viewBox=\"0 0 {outputImage.Width} {outputImage.Height}\"");
                        }
                    }

                    foreach (var chapterFile in chapter.Sources.OrderBy(x => x.SortOrder))
                    {
                        try
                        {
                            var entry = inProcessor.Chapters.First(x => (x.SubFolder + "\\" + x.Name + ".xhtml").Equals(chapterFile.File, StringComparison.InvariantCultureIgnoreCase));
                            newChapter.CssFiles.AddRange(entry.CssFiles);
                            var fileContent = entry.Contents;

                            if (notFirst)
                            {
                                fileContent = fileContent.Replace("<body class=\"nomargin center\">", string.Empty).Replace("<body>", string.Empty);
                            }
                            else
                            {
                                notFirst = true;
                            }
                            newChapter.Contents = string.Concat(newChapter.Contents, fileContent.Replace("</body>", string.Empty));

                            entry.Processed = true;

                            if (!ConfigCombineImages)
                            {
                                var otherentry = inProcessor.Chapters.First(x => (x.SubFolder + "\\" + x.Name + ".xhtml").Equals(chapterFile.File, StringComparison.InvariantCultureIgnoreCase));
                                newChapter.CssFiles.AddRange(otherentry.CssFiles);
                                var otherfileContent = otherentry.Contents;

                                if (notFirst)
                                {
                                    otherfileContent = otherfileContent.Replace("<body class=\"nomargin center\">", string.Empty).Replace("<body>", string.Empty);
                                }
                                else
                                {
                                    notFirst = true;
                                }
                                newChapter.Contents = string.Concat(newChapter.Contents, otherfileContent.Replace("</body>", string.Empty));

                                entry.Processed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message} while processing file {chapterFile}");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(chapter.StartsAtLine))
                    {
                        var location = newChapter.Contents.IndexOf(chapter.StartsAtLine);
                        newChapter.Contents = newChapter.Contents.Substring(location);
                    }

                    if (!string.IsNullOrWhiteSpace(chapter.EndsBeforeLine))
                    {
                        var location = newChapter.Contents.IndexOf(chapter.EndsBeforeLine);
                        newChapter.Contents = newChapter.Contents.Substring(0, location);
                    }

                    if (MainWindow.Configuration.UpdateChapterTitles)
                    {
                        var match = chapterTitleRegex.Match(newChapter.Contents);
                        if (match.Success)
                            newChapter.Contents = newChapter.Contents.Replace(match.Value, $"<h1>{newChapter.Name}</h1>");
                    }

                    //foreach (var replacement in chapter.Replacements)
                    //{
                    //    newChapter.Contents = newChapter.Contents.Replace(replacement.Original, replacement.Replacement);
                    //}

                    outProcessor.Chapters.Add(newChapter);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error processing chapter {chapter.ChapterName} in book {vol.ApiSlug}");
                //Console.WriteLine(ex.ToString());
            }

            if (string.Equals(subfolder, string.Empty))
                subfolder = $"{chapter.SortOrder}-{chapter.Name}";
            else
                subfolder = $"{subfolder}\\{chapter.SortOrder}-{chapter.Name}";

            foreach(var subChapter in chapter.Chapters)
            {
                await ProcessChapter(subChapter, inProcessor, outProcessor, subfolder);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Start();
            Close();
        }
    }
}
