using System.IO;
using System.Text.RegularExpressions;

namespace OBB_WPF
{
    public static class Importer
    {
        static Regex ItemRefRegex = new Regex("\".*?\"");
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");
        private static readonly Regex chapterSubTitleRegex = new Regex("<h2>[\\s\\S]*?<\\/h2>");
        public static Omnibus GenerateVolumeInfo(string inFolder, string series, string? volumeName, int volOrder)
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

                            if (match.Equals("\"cover\"", StringComparison.InvariantCultureIgnoreCase))
                            {
                                match = "\"cover.xhtml\"";
                            }

                            if (!match.Contains(".xhtml")
                                || match.StartsWith("\"signup")
                                || match.StartsWith("\"copyright")
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
                                    foreach (var c in chapterFiles)
                                    {
                                        chapter.Sources.Add(new Source { File = $"{series}\\{volumeName}\\OEBPS\\text\\{c}", SortOrder = chapter.SortOrder + chapter.Sources.Count.ToString("00") });
                                    }

                                    var chapterContent = File.ReadAllText($"{inFolder}\\OEBPS\\text\\" + chapterFiles[0]);
                                    chapter.Name = chapterTitleRegex.Match(chapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                                    if (string.IsNullOrWhiteSpace(chapter.Name)) chapter.Name = chapter.SortOrder;


                                    AddChapter(chapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

                                    foreach(var file in chapterFiles.Skip(1))
                                    {
                                        chapterContent = string.Concat(chapterContent, File.ReadAllText($"{inFolder}\\OEBPS\\text\\{file}"));
                                    }

                                    Chapter subChapter = null;
                                    foreach (Match subHeader in chapterSubTitleRegex.Matches(chapterContent).Where(x => !string.Equals(x.Value, "<h2>I</h2>")))
                                    {
                                        if (subChapter != null)
                                        {
                                            subChapter.EndsBeforeLine = subHeader.Value;
                                        }
                                        else {
                                            chapter.EndsBeforeLine = subHeader.Value; 
                                        }
                                        subChapter = new Chapter
                                        {
                                            CType = chapter.CType,
                                            StartsAtLine = subHeader.Value,
                                            Name = subHeader.Value,
                                            SortOrder = chapter.Chapters.Count.ToString("000"),
                                            Sources = new System.Collections.ObjectModel.ObservableCollection<Source>(chapter.Sources)
                                        };
                                        chapter.Chapters.Add(subChapter);
                                    }

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
                foreach (var c in chapterFiles)
                {
                    finalChapter.Sources.Add(new Source { File = $"{series}\\{volumeName}\\OEBPS\\text\\{c}" });
                }
                var finalChapterContent = File.ReadAllText($"{inFolder}\\OEBPS\\text\\" + chapterFiles[0]);
                finalChapter.Name = chapterTitleRegex.Match(finalChapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                AddChapter(finalChapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

                foreach (var file in chapterFiles.Skip(1))
                {
                    finalChapterContent = string.Concat(finalChapterContent, File.ReadAllText($"{inFolder}\\OEBPS\\text\\{file}"));
                }

                Chapter finalSubChapter = null;
                foreach (Match subHeader in chapterSubTitleRegex.Matches(finalChapterContent))
                {
                    if (finalSubChapter != null)
                    {
                        finalSubChapter.EndsBeforeLine = subHeader.Value;
                    }
                    else
                    {
                        finalChapter.EndsBeforeLine = subHeader.Value;
                    }
                    finalSubChapter = new Chapter
                    {
                        CType = finalChapter.CType,
                        StartsAtLine = subHeader.Value,
                        Name = subHeader.Value,
                        SortOrder = finalChapter.Chapters.Count.ToString("000"),
                        Sources = new System.Collections.ObjectModel.ObservableCollection<Source>(finalChapter.Sources)
                    };
                    finalChapter.Chapters.Add(finalSubChapter);
                }

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
                int sourceOrder = 1;
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
                            foreach (var x in files)
                            {
                                chapter.Sources.Add(new Source { File = $"{series}\\{volumeName}\\item\\xhtml\\{x}", SortOrder = $"{volOrder:000}{order:00}{sourceOrder:000}" });
                                sourceOrder++;
                            }
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
                                foreach (var x in files.Take(index))
                                {
                                    chapter.Sources.Add(new Source { File = $"{series}\\{volumeName}\\item\\xhtml\\{x}", SortOrder = $"{volOrder:000}{order:00}{sourceOrder:000}" });
                                    sourceOrder++;
                                }
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
