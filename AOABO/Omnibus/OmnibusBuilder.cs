using AOABO.Config;
using System.IO.Compression;
using System.Text.RegularExpressions;

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
            Fanbooks
        }

        public static string OverrideDirectory = Directory.GetCurrentDirectory() + "\\Overrides\\";
        private static Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");

        public static void BuildOmnibus()
        {
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
            Console.WriteLine("6: Fanbooks");
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
                    partScope = PartToProcess.Fanbooks;
                    bookTitle = "Ascendance of a Bookworm Fanbooks";
                    break;
                default:
                    partScope = PartToProcess.EntireSeries;
                    bookTitle = "Ascendance of a Bookworm Anthology";
                    break;
            }

            if (Directory.Exists("inputtemp")) Directory.Delete("inputtemp", true);
            Directory.CreateDirectory("inputtemp");

            var epubs = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.epub");

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

                    if ((partScope == PartToProcess.PartOne && !volume.ProcessedInPartOne)
                        || (partScope == PartToProcess.PartTwo && !volume.ProcessedInPartTwo)
                        || (partScope == PartToProcess.PartThree && !volume.ProcessedInPartThree)
                        || (partScope == PartToProcess.PartFour && !volume.ProcessedInPartFour)
                        || (partScope == PartToProcess.PartFive && !volume.ProcessedInPartFive)
                        || (partScope == PartToProcess.Fanbooks && !volume.ProcessedInFanbooks)) continue;

                    ZipFile.ExtractToDirectory(file, $"inputtemp\\{volume.InternalName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message} while unzipping file {vol.FileName}.epub");
                }
            }

            var outProcessor = new Processor.Processor();
            var inProcessor = new Processor.Processor();

            inProcessor.UnpackFolder("inputtemp");
            outProcessor.UnpackFolder("inputtemp");
            outProcessor.Chapters.Clear();

            IFolder folder = Configuration.Options.OutputYearFormat == 0 ? new YearNumberFolder() : new YearFolder();
            Configuration.ReloadVolumes();

            foreach (var vol in Configuration.VolumeNames)
            {
                try
                {
                    var file = vol.NameMatch(epubs);
                    if (file == null) continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No file found that matches volume {vol.FileName}");
                    Console.WriteLine(ex.Message);
                    continue;
                }
                Volume? volume = null;
                try
                {
                    volume = Configuration.Volumes.FirstOrDefault(x => x.InternalName.Equals(vol.InternalName));
                    if (volume == null) continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No entry in Volumes.json found that matches internal name {vol.InternalName}");
                    Console.WriteLine(ex.Message);
                    continue;
                }

                if (partScope == PartToProcess.PartOne && !volume.ProcessedInPartOne
                    || partScope == PartToProcess.PartTwo && !volume.ProcessedInPartTwo
                    || partScope == PartToProcess.PartThree && !volume.ProcessedInPartThree
                    || partScope == PartToProcess.PartFour && !volume.ProcessedInPartFour
                    || partScope == PartToProcess.PartFive && !volume.ProcessedInPartFive) continue;

                Console.WriteLine($"Processing book {volume.InternalName}");

                List<Chapter> chapters;
                switch (partScope)
                {
                    case PartToProcess.PartOne:
                        chapters = BuildChapterList(volume, c => c.ProcessedInPartOne);
                        break;
                    case PartToProcess.PartTwo:
                        chapters = BuildChapterList(volume, c => c.ProcessedInPartTwo);
                        break;
                    case PartToProcess.PartThree:
                        chapters = BuildChapterList(volume, c => c.ProcessedInPartThree);
                        break;
                    case PartToProcess.PartFour:
                        chapters = BuildChapterList(volume, c => c.ProcessedInPartFour);
                        break;
                    case PartToProcess.PartFive:
                        chapters = BuildChapterList(volume, c => c.ProcessedInPartFive);
                        break;
                    case PartToProcess.Fanbooks:
                        chapters = BuildChapterList(volume, c => c.ProcessedInFanbooks);
                        break;
                    default:
                        chapters = BuildChapterList(volume, c => true);
                        break;
                }

                var inChapters = inProcessor.Chapters.Where(x => x.SubFolder.Contains(volume.InternalName)).ToList();
                foreach (var chapter in chapters)
                {

                    try
                    {
                        bool notFirst = false;
                        var newChapter = new Processor.Chapter
                        {
                            Contents = string.Empty,
                            CssFiles = new List<string>(),
                            Name = (Configuration.Options.OutputStructure == OutputStructure.Volumes ? chapter.AltName ?? chapter.ChapterName : chapter.ChapterName) + ".xhtml",
                            SubFolder = folder.MakeFolder(chapter.GetSubFolder(Configuration.Options.OutputStructure), Configuration.Options.StartYear, chapter.Year ?? 0)
                        };
                        newChapter.SortOrder = chapter.SortOrder;
                        outProcessor.Chapters.Add(newChapter);


                        if (File.Exists(OverrideDirectory + chapter.ChapterName + ".xhtml"))
                        {
                            newChapter.Contents = File.ReadAllText(OverrideDirectory + chapter.ChapterName + ".xhtml");
                        }
                        else
                        {
                            foreach (var chapterFile in chapter.OriginalFilenames)
                            {
                                try
                                {
                                    var entry = inChapters.First(x => x.Name.Equals(chapterFile));
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
                                    throw new Exception($"{ex.Message} while processing file {chapterFile}", ex);
                                }
                            }

                            newChapter.Contents = string.Concat(newChapter.Contents, "</body>");
                        }

                        if (Configuration.Options.UpdateChapterNames)
                        {
                            var match = chapterTitleRegex.Match(newChapter.Contents);
                            if(match.Success)
                                newChapter.Contents = newChapter.Contents.Replace(match.Value, $"<h1>{newChapter.Name}</h1>");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing chapter {chapter.ChapterName} in book {vol.InternalName}");
                        Console.WriteLine(ex.ToString());
                    }
                }

                if (vol.OutputUnusedFiles)
                {
                    foreach (var entry in inChapters.Where(x => !x.Processed))
                    {
                        Console.WriteLine($"Unprocessed chapter {entry.Name}");
                    }
                }
            }


            outProcessor.baseFolder = Directory.GetCurrentDirectory();
            outProcessor.Metadata.Add("<meta name=\"cover\" content=\"images/cover.jpg\" />");
            outProcessor.Images.Add(new Processor.Image { Name = "cover.jpg", Referenced = true, OldLocation = "cover.jpg" });

            var coverContents = File.ReadAllText("Reference\\cover.txt");

            outProcessor.Chapters.Add(new Processor.Chapter { Contents = coverContents, Name = "Cover.xhtml", SortOrder = "00", SubFolder = "00-Cover" });

            if (File.Exists($"{bookTitle}.epub")) File.Delete($"{bookTitle}.epub");

            outProcessor.Metadata.Add(@$"<dc:title>{bookTitle}</dc:title>");
            outProcessor.Metadata.Add("<dc:creator id=\"creator01\">Miya Kazuki</dc:creator>");
            outProcessor.Metadata.Add("<meta property=\"display-seq\" refines=\"#creator01\">1</meta>");
            outProcessor.Metadata.Add("<meta property=\"file-as\" refines=\"#creator01\">KAZUKI, MIYA</meta>");
            outProcessor.Metadata.Add("<meta property=\"role\" refines=\"#creator01\" scheme=\"marc:relators\">aut</meta>");
            outProcessor.Metadata.Add("<dc:language>en</dc:language>");
            outProcessor.Metadata.Add("<dc:publisher>J-Novel Club</dc:publisher>");
            outProcessor.Metadata.Add("<dc:identifier id=\"pub-id\">1</dc:identifier>");

            outProcessor.FullOutput(false, Configuration.Options.UseHumanReadableFileStructure, bookTitle);

            if (Directory.Exists("inputtemp")) Directory.Delete("inputtemp", true);

            Console.WriteLine($"\"{bookTitle}\" creation complete. Press any key to continue.");
            Console.ReadKey();
        }

        private static List<Chapter> BuildChapterList(Volume volume, Func<Chapter, bool> filter)
        {
            List<Chapter> chapters = new List<Chapter>();
            if (Configuration.Options.IncludeRegularChapters)
            {
                if (!Configuration.Options.IncludeImagesInChapters)
                {
                    volume.Chapters.ForEach(x => x.RemoveInserts());
                }
                chapters.AddRange(volume.Chapters.Where(filter));
            }

            if (volume.Gallery != null && filter(volume.Gallery))
            {
                var startGallery = volume.Gallery.GetChapter(true, Configuration.Options.SplashImages == GallerySetting.Start, Configuration.Options.ChapterImages == GallerySetting.Start);
                if (startGallery != null) chapters.Add(startGallery);

                var endGallery = volume.Gallery.GetChapter(false, Configuration.Options.SplashImages == GallerySetting.End, Configuration.Options.ChapterImages == GallerySetting.End);
                if (endGallery != null) chapters.Add(endGallery);
            }

            if (!Configuration.Options.IncludeImagesInChapters)
            {
                volume.BonusChapters.ForEach(x => x.RemoveInserts());
            }
            switch (Configuration.Options.BonusChapterSetting)
            {
                case BonusChapterSetting.Chronological:
                    chapters.AddRange(volume.BonusChapters.Where(filter));
                    break;
                case BonusChapterSetting.EndOfBook:
                    chapters.AddRange(volume.BonusChapters.Where(filter));
                    break;
            }

            if (Configuration.Options.MangaChapters != BonusChapterSetting.LeaveOut)
            {
                chapters.AddRange(volume.MangaChapters.Where(filter));
            }

            if (Configuration.Options.ComfyLifeChapters != ComfyLifeSetting.None && volume.ComfyLifeChapter != null && filter(volume.ComfyLifeChapter))
            {
                chapters.Add(volume.ComfyLifeChapter);
            }

            if ((Configuration.Options.CharacterSheets == CharacterSheets.All) && (volume.CharacterSheet != null) && filter(volume.CharacterSheet))
            {
                chapters.Add(volume.CharacterSheet);
            }
            else if ((Configuration.Options.CharacterSheets == CharacterSheets.PerPart) && (volume.CharacterSheet != null) && volume.CharacterSheet.PartSheet && filter(volume.CharacterSheet))
            {
                chapters.Add(volume.CharacterSheet);
            }

            if (Configuration.Options.Maps)
            {
                chapters.AddRange(volume.Maps.Where(filter));
            }

            if (volume.Afterword != null && Configuration.Options.AfterwordSetting != AfterwordSetting.None && filter(volume.Afterword))
            {
                chapters.Add(volume.Afterword);
            }

            if(Configuration.Options.Polls && volume.CharacterPoll != null && filter(volume.CharacterPoll))
            {
                chapters.Add(volume.CharacterPoll);
            }

            if (Configuration.Options.Collection.POVChapterCollection)
            {
                chapters.AddRange(volume.BonusChapters.Select(x => x.GetCollectionChapter()).Where(filter));
                chapters.AddRange(volume.POVChapters.Select(x => x.GetCollectionChapter()).Where(filter));
            }
            chapters.AddRange(volume.POVChapters.Where(filter));

            return chapters;
        }
    }
}