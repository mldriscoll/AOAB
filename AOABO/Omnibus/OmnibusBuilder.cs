using AOABO.Chapters;
using AOABO.Config;
using Core.Processor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static AOABO.Config.VolumeOptions;
using Configuration = AOABO.Config.Configuration;

namespace AOABO.Omnibus
{
    public class OmnibusBuilder
    {
        public enum PartToProcess
        {
            EntireSeries,
            PartOne,
            PartTwo,
            PartThree,
            PartFour,
            PartFive,
            Fanbooks,
            Hannelore
        }

        private static Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");

        public static async Task BuildOmnibus()
        {
            var inputFolder = string.IsNullOrWhiteSpace(Configuration.Options.Folder.InputFolder) ? Directory.GetCurrentDirectory() :
                Configuration.Options.Folder.InputFolder.Length > 1 && Configuration.Options.Folder.InputFolder[1].Equals(':') ? Configuration.Options.Folder.InputFolder : Directory.GetCurrentDirectory() + "\\" + Configuration.Options.Folder.InputFolder;

            var outputFolder = string.IsNullOrWhiteSpace(Configuration.Options.Folder.OutputFolder) ? Directory.GetCurrentDirectory() :
                Configuration.Options.Folder.OutputFolder.Length > 1 && Configuration.Options.Folder.OutputFolder[1].Equals(':') ? Configuration.Options.Folder.OutputFolder : Directory.GetCurrentDirectory() + "\\" + Configuration.Options.Folder.OutputFolder;

            var OverrideDirectory = inputFolder + "\\Overrides\\";


            Console.Clear();
            Console.WriteLine("Creating an Ascendance of a Bookworm Omnibus");
            Console.WriteLine();
            Console.WriteLine("How much of the series should be in the output file?");
            Console.WriteLine("0: Entire Series");
            Console.WriteLine("1: Part One (Daughter of a Soldier)");
            Console.WriteLine("2: Part Two (Apprentice Shrine Maiden)");
            Console.WriteLine("3: Part Three (Adopted Daughter of an Archduke)");
            Console.WriteLine("4: Part Four (Founder of the Royal Academy's So-Called Library Committee)");
            Console.WriteLine("5: Part Five (Avatar of a Goddess)");
            Console.WriteLine("6: Hannelore's Fifth Year at the Royal Academy");
            Console.WriteLine("7: Fanbooks");
            var key = Console.ReadKey();
            Console.WriteLine();
            PartToProcess partScope;
            string bookTitle;
            switch (key.KeyChar)
            {
                case '1':
                    partScope = PartToProcess.PartOne;
                    bookTitle = "Ascendance of a Bookworm Part 1 - Daughter of a Soldier";
                    break;
                case '2':
                    partScope = PartToProcess.PartTwo;
                    bookTitle = "Ascendance of a Bookworm Part 2 - Apprentice Shrine Maiden";
                    break;
                case '3':
                    partScope = PartToProcess.PartThree;
                    bookTitle = "Ascendance of a Bookworm Part 3 - Adopted Daughter of an Archduke";
                    break;
                case '4':
                    partScope = PartToProcess.PartFour;
                    bookTitle = "Ascendance of a Bookworm Part 4 - Founder of the Royal Academy's So-Called Library Committee";
                    break;
                case '5':
                    partScope = PartToProcess.PartFive;
                    bookTitle = "Ascendance of a Bookworm Part 5 - Avatar of a Goddess";
                    break;
                case '6':
                    partScope = PartToProcess.Hannelore;
                    bookTitle = "Ascendance of a Bookworm - Hannelore's Fifth Year at the Royal Academy";
                    break;
                case '7':
                    partScope = PartToProcess.Fanbooks;
                    bookTitle = "Ascendance of a Bookworm Fanbooks";
                    break;
                default:
                    partScope = PartToProcess.EntireSeries;
                    bookTitle = "Ascendance of a Bookworm Anthology";
                    break;
            }

            if (Directory.Exists($"{inputFolder}\\inputtemp")) Directory.Delete($"{inputFolder}\\inputtemp", true);
            Directory.CreateDirectory($"{inputFolder}\\inputtemp");

            var epubs = Directory.GetFiles(inputFolder, "*.epub");

            if (!epubs.Any())
                return;

            foreach (var vol in Configuration.VolumeNames)
            {
                try
                {
                    var file = vol.NameMatch(epubs);
                    if (file == null) continue;
                    var volume = Configuration.Volumes.FirstOrDefault(x => x.InternalName.Equals(vol.InternalName));
                    if (volume == null) continue;

                    //if ((partScope == PartToProcess.PartOne && !volume.ProcessedInPartOne)
                    //    || (partScope == PartToProcess.PartTwo && !volume.ProcessedInPartTwo)
                    //    || (partScope == PartToProcess.PartThree && !volume.ProcessedInPartThree)
                    //    || (partScope == PartToProcess.PartFour && !volume.ProcessedInPartFour)
                    //    || (partScope == PartToProcess.PartFive && !volume.ProcessedInPartFive)
                    //    || (partScope == PartToProcess.Fanbooks && !volume.ProcessedInFanbooks)
                    //    || (partScope == PartToProcess.Hannelore && !volume.ProcessedInHannelore)) continue;

                    ZipFile.ExtractToDirectory(file, $"{inputFolder}\\inputtemp\\{volume.InternalName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message} while unzipping file {vol.FileName}.epub");
                }
            }

            var outProcessor = new Processor();
            var inProcessor = new Processor();

            inProcessor.DisableHyphenProcessing = true;
            await inProcessor.UnpackFolder($"{inputFolder}\\inputtemp");
            await outProcessor.UnpackFolder($"{inputFolder}\\inputtemp");
            outProcessor.Chapters.Clear();

            IFolder folder = Configuration.Options.OutputYearFormat == 0 ? new YearNumberFolder() : new YearFolder();
            Configuration.ReloadVolumes();

            var povChapters = new List<Chapters.MoveableChapter>();
            var missingFiles = new List<string>();



            Omnibus omnibus;
            using (var obStream = File.OpenRead("JSON\\ascendance-of-a-bookworm.json"))
            {
                var obSerializer = new DataContractJsonSerializer(typeof(Omnibus));
                var obj = obSerializer.ReadObject(obStream) ?? throw new Exception("Failed to load Omnibus configuration");
                omnibus = (Omnibus)obj;
            }

            if (partScope != PartToProcess.EntireSeries)
            {
                foreach(var part in omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part).ToArray())
                {
                    if (partScope == PartToProcess.PartOne && part.Name.Equals("Daughter of a Soldier")) continue;

                    omnibus.Chapters.Remove(part);
                }
            }

            IEnumerable<Chapter>? parts;
            switch (Configuration.Options.OutputStructure)
            {
                case OutputStructure.Parts:
                    parts = omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part);
                    foreach(var part in parts)
                    {
                        var volumes = part.Chapters.Where(x => x.CType == Chapter.ChapterType.Volume).ToArray();
                        foreach(var vol in volumes)
                        {
                            part.Chapters.AddRange(vol.Chapters);
                            vol.Chapters.Clear();
                        }
                    }
                    break;
                case OutputStructure.Flat:
                    parts = [.. omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part)];
                    foreach (var part in parts)
                    {
                        var volumes = part.Chapters.Where(x => x.CType == Chapter.ChapterType.Volume).ToArray();
                        foreach (var vol in volumes)
                        {
                            omnibus.Chapters.Add(vol);
                            omnibus.Chapters.AddRange(vol.Chapters);
                            vol.Chapters.Clear();
                            part.Chapters.Remove(vol);
                        }
                    }
                    break;
                case OutputStructure.Seasons:
                    int year = Configuration.Options.StartYear - 1;

                    var currentYear = new Chapter { CType = Chapter.ChapterType.Story, Name = $"Year {year:00}", SortOrder = year.ToString("00") };
                    var currentSeason = new Chapter { CType = Chapter.ChapterType.Story, Name = "Unknown", SortOrder = "1" };
                    omnibus.Chapters.Add(currentYear);
                    currentYear.Chapters.Add(currentSeason);

                    foreach(var part in omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part).ToArray())
                    {
                        omnibus.Chapters.Remove(part);
                        currentSeason.Chapters.Add(part);
                        foreach(var vol in part.Chapters)
                        {
                            currentSeason.Chapters.Add(vol);
                            foreach(var chapter in vol.Chapters)
                            {
                                var newYear = chapter.Tags.FirstOrDefault(x => x.Name.Equals("Year"));
                                if (newYear != null)
                                {
                                    currentYear = new Chapter { CType = Chapter.ChapterType.Story, Name = $"Year {Configuration.Options.StartYear + int.Parse(newYear.Value):00}", SortOrder = $"{Configuration.Options.StartYear + int.Parse(newYear.Value):00}" };
                                    omnibus.Chapters.Add(currentYear);
                                }

                                var newSeason = chapter.Tags.FirstOrDefault(x => x.Name.Equals("Season"));
                                if (newSeason != null)
                                {
                                    currentSeason = new Chapter
                                    {
                                        CType = Chapter.ChapterType.Story,
                                        Name = newSeason.Value,
                                        SortOrder = newSeason.Value switch
                                        {
                                            "Summer" => "1",
                                            "Autumn" => "2",
                                            "Winter" => "3",
                                            "Spring" => "4",
                                            _ => "0",
                                        }
                                    };
                                    currentYear.Chapters.Add(currentSeason);
                                }

                                currentSeason.Chapters.Add(chapter);
                            }
                            vol.Chapters.Clear();
                        }
                        part.Chapters.Clear();
                    }
                    break;
                case OutputStructure.Volumes:
                    break;
            }

            if (Configuration.Options.Chapter.UpdateChapterNames)
            {
                var flatList = BuildChapterList(omnibus, false).Where(x => !string.IsNullOrWhiteSpace(x.POV)).ToArray();
                foreach (var chap in flatList)
                    chap.Name = $"{chap.Name} [{chap.POV}]";
            }

            if (Configuration.Options.Extras.Afterword == AfterwordSetting.None)
            {
                foreach (var part in omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part))
                {
                    foreach (var vol in part.Chapters.Where(x => x.CType == Chapter.ChapterType.Volume))
                    {
                        vol.Chapters.RemoveAll(x => x.CType == Chapter.ChapterType.Afterword);
                    }
                }
            }
            else if (Configuration.Options.Extras.Afterword == AfterwordSetting.OmnibusEnd)
            {
                var afterwords = new Chapter
                {
                    Name = "Afterwords",
                    SortOrder = "999",
                    CType = Chapter.ChapterType.Afterword,
                    Chapters = []
                };

                foreach (var part in omnibus.Chapters.Where(x => x.CType == Chapter.ChapterType.Part))
                {
                    foreach (var vol in part.Chapters.Where(x => x.CType == Chapter.ChapterType.Volume))
                    {
                        afterwords.Chapters.AddRange(vol.Chapters.Where(x => x.CType == Chapter.ChapterType.Afterword));
                        vol.Chapters.RemoveAll(x => x.CType == Chapter.ChapterType.Afterword);
                    }
                }

                omnibus.Chapters.Add(afterwords);

                foreach (var afterword in afterwords.Chapters)
                {
                    var source = afterword.Tags.FirstOrDefault(x => x.Name.Equals("Source"));
                    if (source != null)
                    {
                        afterword.Name = source.Value;
                    }
                }
            }
            
            var flatChapterList = BuildChapterList(omnibus, true).ToArray();

            foreach (var chapter in flatChapterList)
            {
                if (chapter.Chapters.Count == 0 && chapter.Sources.Count == 0) continue;
                try
                {
                    bool notFirst = false;
                    var sources = BuildSourceList(chapter);
                    var newChapter = new Core.Processor.Chapter
                    {
                        Contents = string.Empty,
                        CssFiles = new List<string>(),
                        Name = chapter.Name + ".xhtml",
                        SubFolder = chapter.Subfolder,
                        Set = string.Empty,
                        Priority = 0
                    };
                    newChapter.SortOrder = chapter.SortOrder;
                    outProcessor.Chapters.Add(newChapter);

                    foreach (var chapterFile in sources)
                    {
                        try
                        {
                            var entry = inProcessor.Chapters.FirstOrDefault(x => string.Equals(chapterFile.File, $"{x.SubFolder}\\{x.Name}.xhtml", StringComparison.InvariantCultureIgnoreCase))
                                ?? inProcessor.Chapters.First(x => string.Equals(chapterFile.File, $"{x.SubFolder}\\p-{x.Name}.xhtml", StringComparison.InvariantCultureIgnoreCase));
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

                            if (true && chapterFile.OtherSide != null && !string.IsNullOrWhiteSpace(chapterFile.OtherSide.File))
                            {
                                var left = inProcessor.Chapters.FirstOrDefault(x => string.Equals(chapterFile.OtherSide.File, $"{x.SubFolder}\\{x.Name}.xhtml", StringComparison.InvariantCultureIgnoreCase))
                                    ?? inProcessor.Chapters.First(x => string.Equals(chapterFile.OtherSide.File, $"{x.SubFolder}\\p-{x.Name}.xhtml", StringComparison.InvariantCultureIgnoreCase));

                                var imR = inProcessor.Images.FirstOrDefault(x => entry.Contents.Contains(x.Name));
                                var imL = inProcessor.Images.FirstOrDefault(x => left.Contents.Contains(x.Name));

                                var rightIm = await SixLabors.ImageSharp.Image.LoadAsync(imR.OldLocation);
                                var leftIm = await SixLabors.ImageSharp.Image.LoadAsync(imL.OldLocation);

                                var outputImage = new Image<Rgba32>(rightIm.Width + leftIm.Width, rightIm.Height);
                                outputImage.Mutate(x => x
                                    .DrawImage(leftIm, new Point(0, 0), 1f)
                                    .DrawImage(rightIm, new Point(leftIm.Width, 0), 1f)
                                    );

                                await outputImage.SaveAsJpegAsync(imR.OldLocation + "combi");

                                var widthRegex = new Regex("width=\"\\d*\"");
                                entry.Contents = widthRegex.Replace(entry.Contents, string.Empty);
                                var viewBoxRegex = new Regex("viewBox=\"[\\d ]*\"");
                                entry.Contents = viewBoxRegex.Replace(entry.Contents, $"viewBox=\"0 0 {outputImage.Width} {outputImage.Height}\"");
                            }

                            newChapter.Contents = string.Concat(newChapter.Contents, fileContent.Replace("</body>", string.Empty));

                            entry.Processed = true;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"{ex.Message} while processing file {chapterFile}", ex);
                        }
                    }

                    if (Configuration.Options.Chapter.UpdateChapterNames)
                    {
                        var match = chapterTitleRegex.Match(newChapter.Contents);
                        if (match.Success)
                            newChapter.Contents = newChapter.Contents.Replace(match.Value, $"<h1>{newChapter.Name}</h1>");
                    }
                    if (!string.IsNullOrWhiteSpace(chapter.StartsAtLine))
                    {
                        var location = newChapter.Contents.IndexOf(chapter.StartsAtLine);
                        newChapter.Contents = newChapter.Contents.Substring(location).Replace(chapter.StartsAtLine, $"<body><section><div><h1>{newChapter.Name}</h1>");
                    }

                    if (!string.IsNullOrWhiteSpace(chapter.EndsBeforeLine))
                    {
                        var location = newChapter.Contents.IndexOf(chapter.EndsBeforeLine);
                        newChapter.Contents = newChapter.Contents.Substring(0, location);
                    }
                }
                catch (Exception)
                {
                }
            }


            if (Configuration.Options.OutputStructure == OutputStructure.Volumes)

            //foreach (var vol in Configuration.VolumeNames)
            //{
            //    try
            //    {
            //        var file = vol.NameMatch(epubs);
            //        if (file == null)
            //        {
            //            missingFiles.Add(vol.FileName);
            //            continue;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"No file found that matches volume {vol.FileName}");
            //        Console.WriteLine(ex.Message);
            //        continue;
            //    }

            //    Volume? volume = null;
            //    try
            //    {
            //        volume = Configuration.Volumes.FirstOrDefault(x => x.InternalName.Equals(vol.InternalName));
            //        if (volume == null) continue;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"No entry in Volumes.json found that matches internal name {vol.InternalName}");
            //        Console.WriteLine(ex.Message);
            //        continue;
            //    }

            //    if (partScope == PartToProcess.PartOne && !volume.ProcessedInPartOne
            //        || partScope == PartToProcess.PartTwo && !volume.ProcessedInPartTwo
            //        || partScope == PartToProcess.PartThree && !volume.ProcessedInPartThree
            //        || partScope == PartToProcess.PartFour && !volume.ProcessedInPartFour
            //        || partScope == PartToProcess.PartFive && !volume.ProcessedInPartFive
            //        || partScope == PartToProcess.Hannelore && !volume.ProcessedInHannelore) continue;

            //    Console.WriteLine($"Processing book {volume.InternalName}");

            //    List<Chapters.Chapter> chapters;
            //    switch (partScope)
            //    {
            //        case PartToProcess.PartOne:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInPartOne);
            //            break;
            //        case PartToProcess.PartTwo:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInPartTwo);
            //            break;
            //        case PartToProcess.PartThree:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInPartThree);
            //            break;
            //        case PartToProcess.PartFour:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInPartFour);
            //            break;
            //        case PartToProcess.PartFive:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInPartFive);
            //            break;
            //        case PartToProcess.Fanbooks:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInFanbooks);
            //            break;
            //        case PartToProcess.Hannelore:
            //            chapters = BuildChapterList(volume, c => c.ProcessedInHannelore);
            //            break;
            //        default:
            //            chapters = BuildChapterList(volume, c => true);
            //            break;
            //    }

            //    var inChapters = inProcessor.Chapters.Where(x => x.SubFolder.Contains(volume.InternalName)).ToList();
            //    foreach (var chapter in chapters)
            //    {

            //        try
            //        {
            //            bool notFirst = false;
            //            var newChapter = new Core.Processor.Chapter
            //            {
            //                Contents = string.Empty,
            //                CssFiles = new List<string>(),
            //                Name = chapter.ChapterName + ".xhtml",
            //                SubFolder = folder.MakeFolder(chapter.GetSubFolder(Configuration.Options.OutputStructure), Configuration.Options.StartYear, chapter.Year),
            //                Set = chapter.Set,
            //                Priority = chapter.Priority
            //            };
            //            newChapter.SortOrder = chapter.SortOrder;
            //            outProcessor.Chapters.Add(newChapter);


            //            if (File.Exists($"{OverrideDirectory}{(chapter as MoveableChapter)?.OverrideName}.xhtml"))
            //            {
            //                newChapter.Contents = File.ReadAllText(OverrideDirectory + (chapter as MoveableChapter)?.OverrideName + ".xhtml");
            //            }
            //            else
            //            {
            //                foreach (var chapterFile in chapter.OriginalFilenames)
            //                {
            //                    try
            //                    {
            //                        var entry = inChapters.First(x => x.Name.Equals(chapterFile));
            //                        newChapter.CssFiles.AddRange(entry.CssFiles);
            //                        var fileContent = entry.Contents;

            //                        if (notFirst)
            //                        {
            //                            fileContent = fileContent.Replace("<body class=\"nomargin center\">", string.Empty).Replace("<body>", string.Empty);
            //                        }
            //                        else
            //                        {
            //                            notFirst = true;
            //                        }
            //                        newChapter.Contents = string.Concat(newChapter.Contents, fileContent.Replace("</body>", string.Empty));

            //                        entry.Processed = true;
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        throw new Exception($"{ex.Message} while processing file {chapterFile}", ex);
            //                    }
            //                }
            //            }

            //            if (Configuration.Options.Chapter.UpdateChapterNames)
            //            {
            //                var match = chapterTitleRegex.Match(newChapter.Contents);
            //                if(match.Success)
            //                    newChapter.Contents = newChapter.Contents.Replace(match.Value, $"<h1>{newChapter.Name}</h1>");
            //            }
            //            if (!string.IsNullOrWhiteSpace(chapter.StartLine))
            //            {
            //                var location = newChapter.Contents.IndexOf(chapter.StartLine);
            //                newChapter.Contents = newChapter.Contents.Substring(location).Replace(chapter.StartLine, $"<body><section><div><h1>{newChapter.Name}</h1>");
            //            }

            //            if (!string.IsNullOrWhiteSpace(chapter.EndLine))
            //            {
            //                var location = newChapter.Contents.IndexOf(chapter.EndLine);
            //                newChapter.Contents = newChapter.Contents.Substring(0, location);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Error processing chapter {chapter.ChapterName} in book {vol.InternalName}");
            //            Console.WriteLine(ex.ToString());
            //        }
            //    }

            //    if (vol.OutputUnusedFiles)
            //    {
            //        foreach (var entry in inChapters.Where(x => !x.Processed))
            //        {
            //            Console.WriteLine($"Unprocessed chapter {entry.Name}");
            //        }
            //    }
            ////}

            outProcessor.Metadata.Add("<meta name=\"cover\" content=\"images/cover.jpg\" />");
            outProcessor.Images.Add(new Core.Processor.Image { Name = "cover.jpg", Referenced = true, OldLocation = "cover.jpg" });

            var coverContents = File.ReadAllText("Reference\\cover.txt");

            outProcessor.Chapters.Add(new Core.Processor.Chapter { Contents = coverContents, Name = "Cover.xhtml", SortOrder = "00", SubFolder = "00-Cover" });

            if (File.Exists($"{bookTitle}.epub")) File.Delete($"{bookTitle}.epub");

            outProcessor.Metadata.Add(@$"<dc:title>{bookTitle}</dc:title>");
            outProcessor.Metadata.Add("<dc:creator id=\"creator01\">Miya Kazuki</dc:creator>");
            outProcessor.Metadata.Add("<meta property=\"display-seq\" refines=\"#creator01\">1</meta>");
            outProcessor.Metadata.Add("<meta property=\"file-as\" refines=\"#creator01\">KAZUKI, MIYA</meta>");
            outProcessor.Metadata.Add("<meta property=\"role\" refines=\"#creator01\" scheme=\"marc:relators\">aut</meta>");
            outProcessor.Metadata.Add("<dc:language>en</dc:language>");
            outProcessor.Metadata.Add("<dc:publisher>J-Novel Club</dc:publisher>");
            outProcessor.Metadata.Add("<dc:identifier id=\"pub-id\">1</dc:identifier>");
            outProcessor.Metadata.Add($"<meta property=\"dcterms:modified\">{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ")}</meta>");

            await outProcessor.FullOutput(outputFolder, false, Configuration.Options.UseHumanReadableFileStructure, Configuration.Options.Folder.DeleteTempFolder, bookTitle, Configuration.Options.Image.MaxWidth, Configuration.Options.Image.MaxHeight, Configuration.Options.Image.Quality);

            if (Directory.Exists($"{inputFolder}\\inputtemp")) Directory.Delete($"{inputFolder}\\inputtemp", true);

            Console.WriteLine();
            if (missingFiles.Any())
            {
                Console.WriteLine("Books that could not be found while making this omnibus:");
                foreach (var file in missingFiles) Console.WriteLine(file);
            }
            Console.WriteLine();

            Console.WriteLine($"\"{bookTitle}\" creation complete. Press any key to continue.");
            Console.ReadKey();
        }

        private static IEnumerable<Chapter> BuildChapterList(ChapterHolder ch, bool setSubfolders)
        {
            foreach(var chapter in ch.Chapters)
            {
                yield return chapter;
                foreach (var innerchap in BuildChapterList(chapter, setSubfolders))
                {
                    if (setSubfolders)
                    {
                        if (string.IsNullOrWhiteSpace(innerchap.Subfolder))
                        {
                            innerchap.Subfolder = $"{chapter.SortOrder}-{chapter.Name}";
                        }
                        else
                        {
                            innerchap.Subfolder = string.Concat(chapter.SortOrder, "-", chapter.Name, "\\", innerchap.Subfolder);
                        }
                    }
                    yield return innerchap;
                }
            }
        }

        private static IEnumerable<Source> BuildSourceList(Chapter ch)
        {
            foreach(var source in ch.Sources)
            {
                if ((source.OtherSide != null) && (!string.IsNullOrWhiteSpace(source.OtherSide.File)))
                    source.OtherSide.File = AdjustSourceString(source.OtherSide.File);
                
                if (!string.IsNullOrWhiteSpace(source.File))
                    source.File = AdjustSourceString(source.File);

                yield return source;
            }
        }

        private static string AdjustSourceString(string source)
        {
            foreach(var name in Configuration.VolumeNames)
            {
                if (source.Contains($"\\{name.ApiSlug}\\"))
                    return source.Replace($"ascendance-of-a-bookworm\\{name.ApiSlug}\\", $"{name.InternalName}\\");
            }

            return source;
        }

        private static List<Chapters.Chapter> BuildChapterList(Volume volume, Func<Chapters.Chapter, bool> filter)
        {
            var chapters = new List<Chapters.Chapter>();

            if (Configuration.Options.Chapter.UpdateChapterNames)
            {
                volume.POVChapters.ForEach(x => x.ApplyPOVToTitle());
                volume.BonusChapters.ForEach(x => x.ApplyPOVToTitle());
                volume.MangaChapters.ForEach(x => x.ApplyPOVToTitle());
            }

            if (Configuration.Options.Chapter.IncludeRegularChapters)
            {
                if (!Configuration.Options.Image.IncludeImagesInChapters)
                {
                    volume.Chapters.ForEach(x => x.RemoveInserts());
                }
                chapters.AddRange(volume.Chapters.Where(filter));
            }

            if (volume.Gallery != null && filter(volume.Gallery))
            {
                var startGallery = volume.Gallery.GetChapter(true, Configuration.Options.Image.SplashImages == GallerySetting.Start, Configuration.Options.Image.ChapterImages == GallerySetting.Start);
                if (startGallery != null) chapters.Add(startGallery);

                var endGallery = volume.Gallery.GetChapter(false, Configuration.Options.Image.SplashImages == GallerySetting.End, Configuration.Options.Image.ChapterImages == GallerySetting.End);
                if (endGallery != null) chapters.Add(endGallery);
            }

            if (!Configuration.Options.Image.IncludeImagesInChapters)
            {
                volume.BonusChapters.ForEach(x => x.RemoveInserts());
            }
            switch (Configuration.Options.Chapter.BonusChapter)
            {
                case BonusChapterSetting.Chronological:
                    chapters.AddRange(volume.BonusChapters.Where(filter));
                    break;
                case BonusChapterSetting.EndOfBook:
                    chapters.AddRange(volume.BonusChapters.Where(filter));
                    break;
            }

            if (Configuration.Options.Chapter.MangaChapters != BonusChapterSetting.LeaveOut)
            {
                chapters.AddRange(volume.MangaChapters.Where(filter));
            }

            if (Configuration.Options.Extras.ComfyLifeChapters != ComfyLifeSetting.None && volume.ComfyLifeChapter != null && filter(volume.ComfyLifeChapter))
            {
                chapters.Add(volume.ComfyLifeChapter);
            }

            if ((Configuration.Options.Extras.CharacterSheets == CharacterSheets.All) && (volume.CharacterSheet != null) && filter(volume.CharacterSheet))
            {
                chapters.Add(volume.CharacterSheet);
            }
            else if ((Configuration.Options.Extras.CharacterSheets == CharacterSheets.PerPart) && (volume.CharacterSheet != null) && volume.CharacterSheet.PartSheet && filter(volume.CharacterSheet))
            {
                chapters.Add(volume.CharacterSheet);
            }

            if (Configuration.Options.Extras.Maps)
            {
                chapters.AddRange(volume.Maps.Where(filter));
            }

            if (volume.Afterword != null && Configuration.Options.Extras.Afterword != AfterwordSetting.None && filter(volume.Afterword))
            {
                chapters.Add(volume.Afterword);
            }

            if(Configuration.Options.Extras.Polls && volume.CharacterPoll != null && filter(volume.CharacterPoll))
            {
                chapters.Add(volume.CharacterPoll);
            }

            if (Configuration.Options.Collection.POVChapterCollection)
            {
                chapters.AddRange(volume.BonusChapters.Where(x => !string.IsNullOrWhiteSpace(x.POV)).Select(x => x.GetCollectionChapter()).Where(filter));
                chapters.AddRange(volume.POVChapters.Where(x => !string.IsNullOrWhiteSpace(x.POV)).Select(x => x.GetCollectionChapter()).Where(filter));
                chapters.AddRange(volume.MangaChapters.Where(x => !string.IsNullOrWhiteSpace(x.POV)).Select(x => x.GetCollectionChapter()).Where(filter));
            }
            chapters.AddRange(volume.POVChapters.Where(filter));

            return chapters;
        }
    }


    public class Omnibus : ChapterHolder
    {
        public Source? Cover { get; set; } = null;

        public string Author { get; set; } = string.Empty;
        public string AuthorSort { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;

        public void Combine(Omnibus other)
        {
            foreach (var chapter in other.Chapters)
            {
                var match = Chapters.FirstOrDefault(x => x.Match(chapter));
                if (match != null)
                    match.Combine(chapter);
                else
                    Chapters.Add(chapter);

            }
        }

        public void RemoveDupesFromUnusedList()
        {
            UnusedSources.Remove(Cover!);


            foreach (var chapterList in Chapters.Select(x => x.FindDupes(UnusedSources.ToList())))
            {
                foreach (var source in chapterList)
                {
                    UnusedSources.Remove(source);
                }
            }

            UnusedSources = new ObservableCollection<Source>(UnusedSources.Distinct());
        }


        public ObservableCollection<Source> UnusedSources { get; set; } = new ObservableCollection<Source>();
    }


    public abstract class ChapterHolder
    {
        public List<Chapter> Chapters { get; set; } = [];

        public List<Source> AllSources(string prefix)
        {
            var sources = new List<Source>();
            foreach (var chapter in Chapters)
            {
                sources.AddRange(chapter.Sources.Where(x => x.File.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)));
                foreach (var s in sources)
                {
                    chapter.Sources.Remove(s);
                }
                sources.AddRange(chapter.AllSources(prefix));
            }
            return sources;
        }

        public void RemoveEmpties()
        {
            foreach (var chapter in Chapters)
            {
                chapter.RemoveEmpties();
            }

            Chapters = [.. Chapters.Where(x => x.Sources.Any() || x.Chapters.Any())];
        }

        public void Sort()
        {
            var c = Chapters.OrderBy(x => x.SortOrder).ToList();
            foreach (var chapter in c)
            {
                Chapters.Remove(chapter);
                Chapters.Add(chapter);
            }

            foreach (var chapter in Chapters)
            {
                var sources = chapter.Sources.Where(x => x != null).OrderBy(x => x.SortOrder).ToList();
                chapter.Sources.Clear();
                foreach (var s in sources) chapter.Sources.Add(s);
                chapter.Sort();
            }
        }
    }

    public class Chapter : ChapterHolder
    {
        public enum ChapterType
        {
            Story,
            Bonus,
            NonStory,
            Part,
            Volume,
            Map,
            CharacterSheet,
            Afterword
        }

        public ChapterType CType { get; set; } = ChapterType.Story;

        public string Name { get; set; } = string.Empty;

        public string SortOrder { get; set; } = string.Empty;

        public string POV { get; set; } = string.Empty;
        public ObservableCollection<Source> Sources { get; set; } = new ObservableCollection<Source> { };

        public ObservableCollection<Link> LinkedChapters { get; set; } = new ObservableCollection<Link>();

        public string EndsBeforeLine { get; set; } = string.Empty;
        public string StartsAtLine { get; set; } = string.Empty;

        public List<SubSection> SubSections { get; set; } = new List<SubSection> { };

        public class SubSection
        {
            public int StartsAtIndex { get; set; }
            public string StartsAtLine { get; set; } = string.Empty;
            public int EndsAtIndex { get; set; }
            public string EndsAtLine { get; set; } = string.Empty;
        }



        public bool Match(Chapter other)
        {
            return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                && other.SortOrder.Equals(SortOrder, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Combine(Chapter other)
        {
            foreach (var newSource in other.Sources)
            {
                Sources.Add(newSource);
            }
            foreach (var chapter in other.Chapters)
            {
                var match = Chapters.FirstOrDefault(x => x.Match(chapter));
                if (match != null)
                    match.Combine(chapter);
                else
                    Chapters.Add(chapter);
            }
        }

        public List<Source> FindDupes(List<Source> sourceList)
        {
            var ret = new List<Source>();
            foreach (var s in sourceList)
            {
                if (Sources.Contains(s))
                {
                    ret.Add(s);
                }
            }

            foreach (var chapter in Chapters)
            {
                ret.AddRange(chapter.FindDupes(sourceList));
            }
            return ret;
        }

        public string Subfolder { get; set; } = string.Empty;

        public class Tag
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        public Tag[] Tags { get; set; } = [];
    }
    public class Source : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string File { get; set; } = string.Empty;

        public ObservableCollection<string> Alternates { get; set; } = new ObservableCollection<string>();

        public Source? OtherSide { get; set; } = null;

        public string SortOrder { get; set; } = string.Empty;

        [JsonIgnore]
        public string LeftURI
        {
            set
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("LeftURI"));
            }
            get
            {
                if (OtherSide == null) return "about:blank";

                if (System.IO.File.Exists(OtherSide.File)) return OtherSide.File;

                foreach (var alt in OtherSide.Alternates)
                    if (System.IO.File.Exists(alt)) return alt;

                return "about:blank";
            }
        }

        [JsonIgnore]
        public string RightURI
        {
            get
            {
                if (System.IO.File.Exists(File)) return File;

                foreach (var alt in Alternates)
                    if (System.IO.File.Exists(alt)) return alt;

                return "about:blank";
            }
        }
    }
    public class Link
    {
        public string OriginalLink { get; set; } = String.Empty;
        public string Target { get; set; } = String.Empty;
    }
}