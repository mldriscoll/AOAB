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
        public static async Task ExtractJSON()
        {
            var inFolder = Settings.MiscSettings.InputFolder == null ? Environment.CurrentDirectory :
                Settings.MiscSettings.InputFolder.Length > 1 && Settings.MiscSettings.InputFolder[1].Equals(':') ? Settings.MiscSettings.InputFolder : Environment.CurrentDirectory + "\\" + Settings.MiscSettings.InputFolder;

            var files = Directory.GetFiles(inFolder, "*.epub");
            var dict = new Dictionary<int, string>();


            List<Series> series;
            List<string> volumes = new List<string>();

            using (var reader = new StreamReader("JSON\\Series.json"))
            {
                var a = await JsonSerializer.DeserializeAsync<Series[]>(reader.BaseStream);
                
                series = a.ToList();
            }

            foreach(var serie in series)
            {
                using (var reader = new StreamReader($"JSON\\{serie.InternalName}.json"))
                {
                    var a = await JsonSerializer.DeserializeAsync<Volume[]>(reader.BaseStream);
                    foreach(var entry in a)
                    {
                        serie.Volumes.RemoveAll(x => x.InternalName.Equals(entry.InternalName));
                    }
                }
            }

            var unMappedFiles = series.SelectMany(x => x.Volumes).Select(x => x.FileName);

            foreach (var file in files)
            {
                if (unMappedFiles.Contains(file.Replace(inFolder + "\\", string.Empty)))
                {

                    dict.Add(dict.Keys.Count, file);
                    Console.WriteLine($"{dict.Keys.Last()} - {dict[dict.Keys.Last()].Replace(inFolder + "\\", string.Empty)}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Which .epub file do you wish to extract JSON from?");

            var read = Console.ReadLine();

            if (!int.TryParse(read, out var pick)) return;
            if (!dict.ContainsKey(pick)) return;

            Console.WriteLine("Which Volume of the series is this?");
            var volumeName = Console.ReadLine();

            var epub = dict[pick];

            if (Directory.Exists("jsontemp")) Directory.Delete("jsontemp", true);
            ZipFile.ExtractToDirectory(epub, "jsontemp");

            var content = File.ReadAllLines("jsontemp\\OEBPS\\content.opf");
            bool inSpine = false;
            List<string> chapterFiles = new List<string>();

            var volume = new Volume
            {
                InternalName = epub.Replace(inFolder + "\\", string.Empty).Replace(".epub", string.Empty),
                Chapters = new List<Chapter>
                {
                    new Chapter
                    {
                        ChapterName = $"Volume {volumeName}",
                        SortOrder = volumeName
                    }
                },
                BonusChapters = new List<Chapter>
                {
                    new Chapter
                    {
                        ChapterName = $"Volume {volumeName}",
                        SortOrder = volumeName
                    }
                }
            };

            int order = 1;
            foreach(var line in content)
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
                        
                        if (!match.Contains(".xhtml"))
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
                                chapter.SortOrder = volumeName + order.ToString("00");
                                order++;
                                volume.Chapters[0].Chapters.Add(chapter);
                                chapter.OriginalFilenames.AddRange(chapterFiles.Select(x => x.Replace(".xhtml", string.Empty)));
                                var chapterContent = File.ReadAllText("jsontemp\\OEBPS\\text\\" + chapterFiles[0]);
                                chapter.ChapterName = chapterTitleRegex.Match(chapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
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

            var finalChapter = new Chapter
            {
                ChapterName = "",
            };
            volume.Chapters[0].Chapters.Add(finalChapter);
            finalChapter.OriginalFilenames.AddRange(chapterFiles.Select(x => x.Replace(".xhtml", string.Empty)));

            var imageFiles = Directory.GetFiles("jsontemp\\OEBPS\\Images", "*.jpg");
            foreach(var file in imageFiles)
            {
                if (file.Contains("Insert"))
                {
                    volume.Gallery.ChapterImages.Add(file.Replace("jsontemp\\OEBPS\\Images\\", string.Empty).Replace(".jpg", string.Empty).ToLower());
                }
                else
                {
                    volume.Gallery.SplashImages.Add(file.Replace("jsontemp\\OEBPS\\Images\\", string.Empty).Replace(".jpg", string.Empty).ToLower());
                }
            }
            volume.Gallery.SubFolder = $"{volumeName}-Volume {volumeName}";

            using (var writer = new StreamWriter("Volume.json"))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                await JsonSerializer.SerializeAsync(writer.BaseStream, volume, options);
            }

            Console.WriteLine("Draft volume json saved to Volume.json");
            Console.ReadLine();
        }
    }
}
