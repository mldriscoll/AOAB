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
            var inFolder = Settings.MiscSettings.GetInputFolder();
            var files = Directory.GetFiles(Settings.MiscSettings.GetInputFolder(), "*.epub");

            var dict = new Dictionary<int, string>();

            List<Series> series = await GetSeries();
            List<string> volumes = new List<string>();

            foreach (var serie in series)
            {
                if (File.Exists($"JSON\\{serie.InternalName}.json"))
                {
                    using (var reader = new StreamReader($"JSON\\{serie.InternalName}.json"))
                    {
                        var a = await JsonSerializer.DeserializeAsync<Volume[]>(reader.BaseStream);
                        foreach (var entry in a)
                        {
                            serie.Volumes.RemoveAll(x => x.ApiSlug.Equals(entry.InternalName));
                        }
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

            Volume volume = GenerateVolumeInfo(inFolder, volumeName, epub);

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

        public static async Task ExtractAvailableJSON()
        {
            var inFolder = Settings.MiscSettings.GetInputFolder();
            var files = Directory.GetFiles(Settings.MiscSettings.GetInputFolder(), "*.epub");

            List<Series> series = await GetSeries();

            foreach (var serie in series)
            {
                string seriesJsonFileName = $"JSON\\{serie.InternalName}.json";
                List<Volume?> existingVolumeData = new(serie.Volumes.Count);
                existingVolumeData.AddRange(serie.Volumes.Select<VolumeName, Volume?>(x => null));

                if (File.Exists(seriesJsonFileName))
                {
                    using (var reader = new StreamReader(seriesJsonFileName))
                    {
                        var a = await JsonSerializer.DeserializeAsync<Volume[]>(reader.BaseStream);
                        foreach (var vol in a)
                        {
                            int pos = serie.Volumes.FindIndex(x => x.ApiSlug.Equals(vol.InternalName));
                            if (pos == -1)
                            {
                                Console.WriteLine($"Found volume data {vol.InternalName} without matching volume name in {serie.InternalName}.");
                                existingVolumeData.Add(vol);
                            }
                            else
                            {
                                existingVolumeData[pos] = vol;
                            }
                        }
                        existingVolumeData.AddRange(a);
                    }
                }

                bool generated = false;
                for (var i = 0; i < serie.Volumes.Count; i++)
                {
                    if (existingVolumeData[i] != null)
                        continue;
                    string epubFileName = inFolder + "\\" + serie.Volumes[i].FileName;
                    if (!files.Contains(epubFileName))
                    {
                        Console.WriteLine($"File not found {epubFileName} not extracting volume metadata.");
                        continue;
                    }
                    Console.WriteLine($"Extracting volume metadata from {epubFileName} (Volume {i + 1}).");
                    existingVolumeData[i] = GenerateVolumeInfo(inFolder, (i + 1).ToString(), epubFileName);
                    generated = true;
                }

                if (generated)
                {
                    Console.WriteLine($"Updating {seriesJsonFileName} with new volume data.");
                    using (var writer = new StreamWriter(seriesJsonFileName))
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        await JsonSerializer.SerializeAsync(writer.BaseStream, existingVolumeData.Where(x => x != null).ToArray(), options);
                    }
                }
            }
        }

        private static async Task<List<Series>> GetSeries()
        {
            List<Series> series;

            using (var reader = new StreamReader("JSON\\Series.json"))
            {
                var a = await JsonSerializer.DeserializeAsync<Series[]>(reader.BaseStream);

                series = a.ToList();
            }

            return series;
        }

        private static Volume GenerateVolumeInfo(string inFolder, string? volumeName, string epub)
        {
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
            foreach (var file in imageFiles)
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
            return volume;
        }

        public static async Task GenerateTable()
        {
            var series = await GetSeries();

            series.RemoveAll(x => x.Volumes.Count < 2);

            series = series.OrderBy(x => x.Name).ToList();

            File.Delete("JSON\\Series.json");

            using (var writer = new StreamWriter("JSON\\Series.json"))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                await JsonSerializer.SerializeAsync<List<Series>>(writer.BaseStream, series, options);
            }

            var lines = new List<string> {
                "|Series|Edited Volumes|Autogenerated Volumes|Editors",
                "|-|-|-|-"
            };

            var files = Directory.GetFiles("JSON", "*.json").ToList();
            files.Remove("JSON\\Series.json");

            foreach(var serie in series)
            {
                var edited = serie.Volumes.Count(x => x.EditedBy != null);
                var unedited = serie.Volumes.Count(x => x.EditedBy == null);
                var editors = serie.Volumes.Where(x => x.EditedBy != null)?.Select(x => x.EditedBy)?.Distinct();
                string editor = editors.Any() ? editors.Aggregate((editor, set) => string.Concat(set, editor, " ")) : string.Empty;

                lines.Add($"|{serie.Name}|{edited}|{unedited}|{editor}");
                files.Remove("JSON\\" + serie.InternalName + ".json");
            }

            foreach(var file in files)
            {
                File.Delete(file);
            }

            await File.WriteAllLinesAsync("table.txt", lines);
        }
    }
}
