using Core;
using Core.Downloads;
using OBB.JSONCode;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OBB
{
    public static class JSONBuilder
    {
        static Regex ItemRefRegex = new Regex("\".*?\"");
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");
        private static readonly Regex chapterSubTitleRegex = new Regex("<h2>[\\s\\S]*?<\\/h2>");
        public static async Task ExtractJSON()
        {
            var inFolder = Settings.MiscSettings.GetInputFolder();
            var files = Directory.GetFiles(Settings.MiscSettings.GetInputFolder(), "*.epub");

            var dict = new Dictionary<int, string>();
            var series = await GetSeries();


            using (var client = new HttpClient()) 
            {
                var login = await Login.FromFile(client);
                login = login ?? await Login.FromConsole(client);
                var library = await Downloader.GetLibrary(client, login!.AccessToken);
                foreach (var vol in series.SelectMany(x => x.Volumes))
                {
                    if (!files.Any(x => x.Equals(inFolder + "\\" + vol.FileName, StringComparison.InvariantCultureIgnoreCase))
                        && library.books.Any(x => x.volume.slug.Equals(vol.ApiSlug)))
                    {
                        await Downloader.DoDownloads(client, login.AccessToken, inFolder, new List<Name> { new Name { ApiSlug = vol.ApiSlug, FileName = vol.FileName } }, MangaQuality.FourK);
                    }
                }
            }
            
            files = Directory.GetFiles(Settings.MiscSettings.GetInputFolder(), "*.epub");

            foreach (var file in files)
            {
                var serie = series.FirstOrDefault(x => x.Volumes.Any(y => (inFolder + "\\" + y.FileName).Equals(file)));
                if (serie == null)
                {
                    Console.WriteLine($"Epub {file} has not been added to a series yet");
                }
                else
                {
                    var vols = await GetVolumes(serie.InternalName);
                    var vol = serie.Volumes.First(x => (inFolder + "\\" + x.FileName).Equals(file));
                    if (vol.EditedBy == null)
                    {
                        Console.Write($"Mark {file} as edited by:");
                        var editor = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(editor)) vol.EditedBy = editor;

                        var volume = GenerateVolumeInfo(inFolder, vol.Title, vol.Order, file);
                        vols.RemoveAll(x => x.InternalName.Equals(vol.ApiSlug));
                        vols.Add(volume);
                        vols = vols.OrderBy(x => x.InternalName).ToList();
                    }
                    await SaveVolumes(serie.InternalName, vols, serie);
                }
            }

            await SaveSeries(series);
            Console.WriteLine("Volume json updated");
            Console.ReadLine();
        }

        public static async Task<List<Series>> GetSeries()
        {
            List<Series> series;

            using (var reader = new StreamReader("JSON\\Series.json"))
            {
                var a = (await JsonSerializer.DeserializeAsync<Series[]>(reader.BaseStream))!;

                series = a.ToList();
            }

            return series;
        }

        public static async Task<List<Volume>> GetVolumes(string seriesInternalName)
        {
            var filename = $"JSON\\{seriesInternalName}.json";
            if (!File.Exists(filename)) return new List<Volume>();
            if ((new FileInfo(filename)).Length == 0) return new List<Volume>();
            using (var reader = new StreamReader(filename))
            {
                return (await JsonSerializer.DeserializeAsync<List<Volume>>(reader.BaseStream))!;
            }
        }

        private static async Task SaveVolumes(string seriesInternalName, List<Volume> volumes, Series serie)
        {
            volumes = volumes.OrderBy(x =>
            {
                var match = serie.Volumes.FirstOrDefault(y => y.ApiSlug.Equals(x.InternalName));

                return match?.Order ?? 999;
            }).ToList();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            using (var writer = new StreamWriter($"JSON\\{seriesInternalName}.json"))
            {
                await JsonSerializer.SerializeAsync(writer.BaseStream, volumes, options);
            }
        }
        public static async Task SaveSeries(List<Series> series)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            using (var writer = new StreamWriter($"JSON\\Series.json"))
            {
                await JsonSerializer.SerializeAsync(writer.BaseStream, series.OrderBy(x => x.Name).ToArray(), options);
            }
        }

        public static Volume GenerateVolumeInfo(string inFolder, string volumeName, int volOrder, string epub)
        {
            if (Directory.Exists("jsontemp")) Directory.Delete("jsontemp", true);
            ZipFile.ExtractToDirectory(epub, "jsontemp");

            bool inSpine = false;
            List<string> chapterFiles = new List<string>();

            var volume = new Volume
            {
                InternalName = epub.Replace(inFolder + "\\", string.Empty).Replace(".epub", string.Empty),
                Chapters = new List<Chapter>
                {
                    new Chapter
                    {
                        ChapterName = volumeName,
                        SortOrder = volOrder.ToString("000")
                    }
                },
                BonusChapters = new List<Chapter>
                {
                    new Chapter
                    {
                        ChapterName = volumeName,
                        SortOrder = volOrder.ToString("000")
                    }
                },
                ExtraContent = new List<Chapter>
                {
                    new Chapter
                    {
                        ChapterName = volumeName,
                        SortOrder = volOrder.ToString("000")
                    }
                }
            };


            int order = 1;
            try
            {
                var content = File.ReadAllLines("jsontemp\\OEBPS\\content.opf");
                var imageFiles = Directory.GetFiles("jsontemp\\OEBPS\\Images", "*.jpg").Select(x => x.Replace("jsontemp\\OEBPS\\Images\\", string.Empty).Replace(".jpg", string.Empty).ToLower()).ToList();

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
                                    chapter.OriginalFilenames.AddRange(chapterFiles.Select(x => x.Replace(".xhtml", string.Empty)));

                                    var chapterContent = File.ReadAllText("jsontemp\\OEBPS\\text\\" + chapterFiles[0]);
                                    chapter.ChapterName = chapterTitleRegex.Match(chapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);

                                    int subSection = 1;
                                    foreach (Match subHeader in chapterSubTitleRegex.Matches(chapterContent))
                                    {
                                        chapter.Splits.Add(new ChapterSplit
                                        {
                                            Name = subHeader.Value.Replace("<h2>", "").Replace("</h2>", ""),
                                            SplitLine = subHeader.Value,
                                            SortOrder = subSection.ToString("00"),
                                            SubFolder = string.Empty
                                        });
                                        subSection++;
                                    }

                                    AddChapter(volume, chapter, volumeName, volOrder, imageFiles);

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
                finalChapter.OriginalFilenames.AddRange(chapterFiles.Select(x => x.Replace(".xhtml", string.Empty)));
                var finalChapterContent = File.ReadAllText("jsontemp\\OEBPS\\text\\" + chapterFiles[0]);
                finalChapter.ChapterName = chapterTitleRegex.Match(finalChapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                AddChapter(volume, finalChapter, volumeName, volOrder, imageFiles);

                foreach (var file in imageFiles)
                {
                    if (file.Contains("insert"))
                    {
                        AddImage(file, volume.Gallery[0].ChapterImages);
                    }
                    else
                    {
                        AddImage(file, volume.Gallery[0].SplashImages);
                    }
                }
                volume.Gallery[0].SubFolder = $"{volOrder}-{volumeName}";
            }
            // Backup for Manga that don't have content files
            catch (DirectoryNotFoundException noContent)
            {
                Console.WriteLine($"Directory not found: {noContent.Message}");

                var nav = File.ReadAllLines("jsontemp\\item\\nav.xhtml").Select(x => x.Trim()).ToList();
                var files = Directory.GetFiles("jsontemp\\item\\xhtml\\", "*.xhtml").Select(x => x.Replace(".xhtml", string.Empty).Replace("jsontemp\\item\\xhtml\\", string.Empty)).ToList();

                var incontents = false;
                Chapter? chapter = null;
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
                            chapter!.OriginalFilenames.AddRange(files);
                        }
                        else if (line.Equals("<ol>", StringComparison.InvariantCultureIgnoreCase))
                        {
                        }
                        else //Chapter Border
                        {

                            var linesplit = line.Split('"');
                            var firstPage = linesplit[1].Replace("xhtml/", string.Empty).Replace(".xhtml", string.Empty);

                            if (chapter != null)
                            {
                                var index = files.IndexOf(firstPage);
                                chapter.OriginalFilenames.AddRange(files.Take(index));
                                files.RemoveRange(0, index);
                            }

                            var title = linesplit[2].Substring(1).Replace("</a></li>", string.Empty);
                            chapter = new Chapter
                            {
                                ChapterName = title,
                            };
                            chapter.SortOrder = ((volOrder * 100) + order).ToString("00000");
                            order++;
                            volume.Chapters[0].Chapters.Add(chapter);
                        }
                    }
                }
            }

            return volume;
        }

        private static void AddChapter(Volume vol, Chapter chapter, string volumeName, int volumeSortOrder, List<string> imageFiles)
        {
            // No need to include the old contents pages or chapters that only include images
            if (chapter.OriginalFilenames.All(x => x.Equals("tocimg", StringComparison.InvariantCultureIgnoreCase)
                || x.Equals("toc", StringComparison.InvariantCultureIgnoreCase)
                || imageFiles.Any(y => y.Equals(x, StringComparison.InvariantCultureIgnoreCase))))
            {

            }
            else if (chapter.OriginalFilenames.Any(x => x.StartsWith("side")
                                    || x.StartsWith("interlude")
                                    || x.StartsWith("extra")
                                    || x.StartsWith("bonus")
                                    || x.StartsWith("diary")
                                    ))
            {
                vol.BonusChapters[0].Chapters.Add(chapter);
            }
            else if (chapter.OriginalFilenames.Any(x => x.StartsWith("afterword")))
            {
                chapter.SubFolder = "999-Afterwords";
                chapter.SortOrder = volumeSortOrder.ToString("000");
                chapter.ChapterName = volumeName;
                vol.ExtraContent.Add(chapter);
            }
            else if (chapter.OriginalFilenames.Any(x => x.StartsWith("character")))
            {
                chapter.ChapterName = "Character Sheet";
                vol.ExtraContent[0].Chapters.Add(chapter);
            }
            else if (chapter.OriginalFilenames.Any(x => x.StartsWith("map")))
            {
                chapter.ChapterName = "Map";
                chapter.SubFolder = "998-Maps";
                vol.ExtraContent.Add(chapter);
            }
            else
            {
                vol.Chapters[0].Chapters.Add(chapter);
            }
        }
        public static async Task GenerateTable()
        {
            var series = await GetSeries();

            series = series.OrderBy(x => x.Name).ToList();

            //var lines = new List<string> {
            //    "Series|Edited Volumes|Autogenerated Volumes|Editors",
            //    "|-|-|-|-"
            //};

            var fullyEdited = new List<string>();
            var partEdited = new List<string>();
            var unedited = new List<string>();

            foreach(var serie in series)
            {
                int prepub = serie.Volumes.Count(x => DateTime.Parse(x.Published!) > DateTime.UtcNow.Date);
                // Fully Edited
                if (serie.Volumes.All(x => !string.IsNullOrEmpty(x.EditedBy) || DateTime.Parse(x.Published!) > DateTime.UtcNow.Date)  && serie.Volumes.Count > prepub)
                {
                    if (serie.Volumes.Where(x => x.EditedBy != null).GroupBy(x => x.EditedBy).Count() > 1)
                    {
                        if (prepub > 0)
                            fullyEdited.Add($"* {serie.Name} ({serie.Volumes.Count} books) - Edited by {serie.Volumes.GroupBy(x => x.EditedBy).Aggregate(string.Empty, (acc, item) => string.Concat(acc, " ", item.Key))}");
                        else
                            fullyEdited.Add($"* {serie.Name} ({serie.Volumes.Count} books) - Edited by {serie.Volumes.GroupBy(x => x.EditedBy).Aggregate(string.Empty, (acc, item) => string.Concat(acc, " ", item.Key))}");
                    }
                    else
                    {
                        if (prepub > 0)
                            fullyEdited.Add($"* {serie.Name} ({serie.Volumes.Count - prepub} books) - Edited by {serie.Volumes[0].EditedBy} + {prepub} in pre-publication");
                        else
                            fullyEdited.Add($"* {serie.Name} ({serie.Volumes.Count - prepub} books) - Edited by {serie.Volumes[0].EditedBy}");
                    }
                }
                // Fully Unedited
                else if (serie.Volumes.All(x => string.IsNullOrEmpty(x.EditedBy)))
                {
                    if (prepub > 0)
                        unedited.Add($"* {serie.Name} ({serie.Volumes.Count - prepub} books) + {prepub} in pre-publication");
                    else
                        unedited.Add($"* {serie.Name} ({serie.Volumes.Count} books)");
                }
                else
                {
                    var editedVolumes = serie.Volumes.Where(x => !string.IsNullOrEmpty(x.EditedBy)).ToList();
                    if (editedVolumes.GroupBy(x => x.EditedBy).Count() > 1)
                    {
                        if (prepub > 0)
                            partEdited.Add($"* {serie.Name} ({serie.Volumes.Count - prepub} books) - Edited by {editedVolumes.GroupBy(x => x.EditedBy).Aggregate(string.Empty, (acc, item) => string.Concat(acc, " ", item, "(", item.Count(), " books)"))} + {prepub} in pre-publication");
                        else
                            partEdited.Add($"* {serie.Name} ({serie.Volumes.Count} books) - Edited by {editedVolumes.GroupBy(x => x.EditedBy).Aggregate(string.Empty, (acc, item) => string.Concat(acc, " ", item, "(", item.Count(), " books)"))}");
                    }
                    else
                    {
                        if (prepub > 0)
                            partEdited.Add($"* {serie.Name} ({serie.Volumes.Count - prepub} books) - Edited by {editedVolumes[0].EditedBy} ({editedVolumes.Count} books) + {prepub} in pre-publication");
                        else
                            partEdited.Add($"* {serie.Name} ({serie.Volumes.Count} books) - Edited by {editedVolumes[0].EditedBy} ({editedVolumes.Count} books)");
                    }
                }
            }

            var combined = new List<string> { "Fully Edited Series" };
            combined.AddRange(fullyEdited);
            combined.Add("");
            combined.Add("Partially Edited Series");
            combined.AddRange(partEdited);
            combined.Add("");
            combined.Add("Unedited Series");
            combined.AddRange(unedited);

            await File.WriteAllLinesAsync("table.txt", combined);
        }

        private static void AddImage(string file, List<string> imageList)
        {
            var miniFile = file.Replace("jsontemp\\OEBPS\\Images\\", string.Empty).Replace(".jpg", string.Empty).ToLower();

            if (miniFile.StartsWith("map")
                || miniFile.StartsWith("character"))
            {
                return;
            }

            if (File.Exists("jsontemp\\OEBPS\\text\\" + miniFile + ".xhtml"))
            {
                imageList.Add(miniFile);
            }
            else
            {
                var files = Directory.GetFiles("jsontemp\\OEBPS\\text").ToDictionary(x => x, x => File.ReadAllText(x));
                var fileMatches = files.Where(x => x.Value.Contains(miniFile + ".jpg", StringComparison.InvariantCultureIgnoreCase));
                foreach (var match in fileMatches)
                {
                    imageList.Add(match.Key.Replace("jsontemp\\OEBPS\\text\\", string.Empty).Replace(".xhtml", string.Empty));
                }
            }
        }
    }
}
