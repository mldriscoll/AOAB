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

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for CreateOmnibus.xaml
    /// </summary>
    public partial class CreateOmnibus : Window
    {
        private readonly Omnibus series;

        public CreateOmnibus(Omnibus series)
        {
            InitializeComponent();
            this.series = series;
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

            //var volumes = await JSONBuilder.GetVolumes(selection.InternalName);

            bool coverPicked = false;

            //foreach (var vol in selection.Volumes.OrderBy(x => x.Order))
            //{
            //if (!coverPicked)
            //{
            //    var cover = $"{temp}\\OEBPS\\Images\\Cover.jpg";
            //    if (!File.Exists(cover)) cover = $"{temp}\\item\\image\\cover.jpg";
            //    if (File.Exists("cover.jpg")) File.Delete("cover.jpg");
            //    File.Copy(cover, "cover.jpg");
            //    coverPicked = true;
            //}

            //var inChapters = inProcessor.Chapters.Where(x => x.SubFolder.Contains(volume.InternalName + "\\")).ToList();
            //var chapters = BuildChapterList(volume, x => true);

            foreach (var chapter in series.Chapters)
            {
                ProcessChapter(chapter, inProcessor, outProcessor, string.Empty);
            }

            //if (vol.ShowRemainingFiles)
            //{
            //    Console.WriteLine($"Unprocessed Files in volume {vol.ApiSlug}");
            //    foreach (var entry in inChapters.Where(x => !x.Processed))
            //    {
            //        Console.WriteLine($"\tUnprocessed chapter {entry.Name}");
            //    }
            //}
            //}

            outProcessor.Metadata.Add("<meta name=\"cover\" content=\"images/cover.jpg\" />");
            outProcessor.Images.Add(new Core.Processor.Image { Name = "cover.jpg", Referenced = true, OldLocation = "cover.jpg" });

            var coverContents = File.ReadAllText("Reference\\cover.txt");

            outProcessor.Chapters.Add(new Core.Processor.Chapter { Contents = coverContents, Name = "Cover.xhtml", SortOrder = "0000", SubFolder = "" });

            if (File.Exists($"{series.Name}.epub")) File.Delete($"{series.Name}.epub");

            outProcessor.Metadata.Add(@$"<dc:title>{series.Name}</dc:title>");
            //outProcessor.Metadata.Add($"<dc:creator id=\"creator01\">{selection.Author}</dc:creator>");
            outProcessor.Metadata.Add("<meta property=\"display-seq\" refines=\"#creator01\">1</meta>");
            //outProcessor.Metadata.Add($"<meta property=\"file-as\" refines=\"#creator01\">{selection.AuthorSort}</meta>");
            outProcessor.Metadata.Add("<meta property=\"role\" refines=\"#creator01\" scheme=\"marc:relators\">aut</meta>");
            outProcessor.Metadata.Add("<dc:language>en</dc:language>");
            outProcessor.Metadata.Add("<dc:publisher>J-Novel Club</dc:publisher>");
            outProcessor.Metadata.Add("<dc:identifier id=\"pub-id\">1</dc:identifier>");
            outProcessor.Metadata.Add($"<meta property=\"dcterms:modified\">{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ")}</meta>");

            //await outProcessor.FullOutput(outputFolder,
            //    false,
            //    Settings.MiscSettings.UseHumanReadableFileNames,
            //    Settings.MiscSettings.RemoveTempFolder,
            //    selection.Name,
            //    Settings.ImageSettings.MaxImageWidth,
            //    Settings.ImageSettings.MaxImageHeight,
            //    Settings.ImageSettings.ImageQuality);
            await outProcessor.FullOutput(outputFolder,
                false,
                true,
                false,
                series.Name,
                null,
                null,
                99);

            //if (Directory.Exists($"{inFolder}\\inputtemp")) Directory.Delete($"{inFolder}\\inputtemp", true);

            //Console.WriteLine($"\"{selection.Name}\" creation complete. Press any key to continue.");
            //Console.ReadKey();

        }

        private void ProcessChapter(Chapter chapter, Processor inProcessor, Processor outProcessor, string subfolder)
        {
            try
            {
                bool notFirst = false;
                var newChapter = new Core.Processor.Chapter
                {
                    Contents = string.Empty,
                    CssFiles = new List<string>(),
                    Name = chapter.Name + ".xhtml",
                    SubFolder = subfolder,
                    SortOrder = chapter.SortOrder,
                    //ChapterLinks = chapter.LinkedChapters
                };

                if (chapter.Sources.Any())
                {

                    newChapter.SortOrder = chapter.SortOrder;

                    //if (Settings.ImageSettings.CombineMangaSplashPages)
                    //{
                    //    foreach (var splash in chapter.SplashPages)
                    //    {
                    //        var one = inChapters.First(x => x.Name.Equals(splash.Right, StringComparison.InvariantCultureIgnoreCase));
                    //        var imR = inProcessor.Images.FirstOrDefault(x => one.Contents.Contains(x.Name));

                    //        var two = inChapters.First(x => x.Name.Equals(splash.Left, StringComparison.InvariantCultureIgnoreCase));
                    //        var imL = inProcessor.Images.FirstOrDefault(x => two.Contents.Contains(x.Name));

                    //        var right = await SixLabors.ImageSharp.Image.LoadAsync(imR.OldLocation);
                    //        var left = await SixLabors.ImageSharp.Image.LoadAsync(imL.OldLocation);

                    //        var outputImage = new Image<Rgba32>(right.Width + left.Width, right.Height);
                    //        outputImage.Mutate(x => x
                    //            .DrawImage(left, new Point(0, 0), 1f)
                    //            .DrawImage(right, new Point(left.Width, 0), 1f)
                    //            );

                    //        await outputImage.SaveAsJpegAsync(imR.OldLocation);
                    //        chapter.OriginalFilenames.Remove(splash.Left);

                    //        var widthRegex = new Regex("width=\"\\d*\"");
                    //        one.Contents = widthRegex.Replace(one.Contents, string.Empty);
                    //        var viewBoxRegex = new Regex("viewBox=\"[\\d ]*\"");
                    //        one.Contents = viewBoxRegex.Replace(one.Contents, $"viewBox=\"0 0 {outputImage.Width} {outputImage.Height}\"");
                    //    }
                    //}

                    foreach (var chapterFile in chapter.Sources)
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
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message} while processing file {chapterFile}");
                        }
                    }

                    //if (chapter.Splits.Any())
                    //{
                    //    var dict = chapter.Splits.ToDictionary(x => newChapter.Contents.IndexOf(x.SplitLine), x => x);
                    //    var keys = dict.Keys.OrderByDescending(x => x);
                    //    int? previousIndex = null;
                    //    var divRegex = new Regex("<div class=\".*?\">");
                    //    var div = divRegex.Match(newChapter.Contents).Value;
                    //    foreach (var key in keys)
                    //    {
                    //        var split = previousIndex.HasValue ? newChapter.Contents.Substring(key + dict[key].SplitLine.Length, previousIndex.Value - key - dict[key].SplitLine.Length) : newChapter.Contents.Substring(key + dict[key].SplitLine.Length);

                    //        var splitChapter = new Core.Processor.Chapter
                    //        {
                    //            Contents = $"<body>{div}<h1>{dict[key].Name}</h1>{split}</div>",
                    //            CssFiles = newChapter.CssFiles,
                    //            Name = dict[key].Name + ".xhtml",
                    //            SubFolder = string.IsNullOrWhiteSpace(dict[key].SubFolder) ? newChapter.SubFolder + $"\\{newChapter.SortOrder}-{newChapter.Name}" : dict[key].SubFolder,
                    //            SortOrder = dict[key].SortOrder,
                    //        };
                    //        outProcessor.Chapters.Add(splitChapter);
                    //        previousIndex = key;
                    //    }

                    //    if (chapter.KeepFirstSplitSection)
                    //    {
                    //        newChapter.Contents = newChapter.Contents.Substring(0, previousIndex ?? newChapter.Contents.Length) + "</div>";
                    //    }
                    //}

                    //if (Settings.ChapterSettings.UpdateChapterTitles)
                    //{
                    //    var match = chapterTitleRegex.Match(newChapter.Contents);
                    //    if (match.Success)
                    //        newChapter.Contents = newChapter.Contents.Replace(match.Value, $"<h1>{newChapter.Name}</h1>");
                    //}

                    //foreach (var replacement in chapter.Replacements)
                    //{
                    //    newChapter.Contents = newChapter.Contents.Replace(replacement.Original, replacement.Replacement);
                    //}

                    //if (!chapter.Splits.Any() || chapter.KeepFirstSplitSection)
                    //{
                    //    var matchingChapter = outProcessor.Chapters.FirstOrDefault(x => x.Name.Equals(newChapter.Name) && x.SortOrder == newChapter.SortOrder && x.SubFolder.Equals(newChapter.SubFolder));
                    //    if (matchingChapter != null)
                    //    {
                    //        matchingChapter.Contents = string.Concat(matchingChapter.Contents, newChapter.Contents);
                    //        matchingChapter.CssFiles = matchingChapter.CssFiles.Union(newChapter.CssFiles).Distinct().ToList();
                    //    }
                    //    else
                    //    {
                    //        outProcessor.Chapters.Add(newChapter);
                    //    }
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
                ProcessChapter(subChapter, inProcessor, outProcessor, subfolder);
            }
        }
    }
}
