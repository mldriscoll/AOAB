using System.IO;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace OBB_WPF.Editor
{
    public static class Importer
    {
        static Regex ItemRefRegex = new Regex("\".*?\"");
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");
        private static readonly Regex chapterSubTitleRegex = new Regex("<h2>[\\s\\S]*?<\\/h2>");
        public static Omnibus GenerateVolumeInfo(string inFolder, string series, string volumeName, int volOrder)
        {
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

            // Find Content File
            var opfFiles = Directory.GetFiles(inFolder, "*.opf", SearchOption.AllDirectories);
            var xmlSerializer = new XmlSerializer(typeof(Package));
            Package? content = null;
            string opfFolder = String.Empty;
            if (opfFiles.Any(x => x.Contains("content.opf", StringComparison.InvariantCultureIgnoreCase)))
            {
                var file = opfFiles.First(x => x.Contains("content.opf", StringComparison.InvariantCultureIgnoreCase));
                content = (xmlSerializer.Deserialize(File.OpenRead(file)) as Package)!;

                var finfo = new FileInfo(file);
                opfFolder = finfo.Directory!.FullName;
            }
            else if (opfFiles.Any(x => x.Contains("package.opf", StringComparison.InvariantCultureIgnoreCase)))
            {
                var file = opfFiles.First(x => x.Contains("package.opf", StringComparison.InvariantCultureIgnoreCase));
                content = (xmlSerializer.Deserialize(File.OpenRead(file)) as Package)!;

                var finfo = new FileInfo(file);
                opfFolder = finfo.Directory!.FullName;
            }


            string? textFolder = null;
            textFolder = Directory.GetDirectories(inFolder, "*text*", SearchOption.AllDirectories).FirstOrDefault();
            if (textFolder == null) textFolder = Directory.GetDirectories(inFolder, "*oebps*", SearchOption.AllDirectories).FirstOrDefault();

            try
            {
                if (content != null && textFolder != null)
                {
                    var imageFolder = Directory.GetDirectories(inFolder, "*images*", SearchOption.AllDirectories).First();
                    var imageFiles = Directory.GetFiles(imageFolder, "*.jpg").Select(x => x.Replace(".jpg", string.Empty))
                        .Union(Directory.GetFiles(imageFolder, "*.jpeg").Select(x => x.Replace(".jpeg", string.Empty)))
                        .Select(x => x.Replace($"{imageFolder}\\", string.Empty).ToLower())
                        .ToList();

                    var folInfo = new DirectoryInfo(inFolder);
                    ProcessLN(volumeName, volOrder, ob, ref order, content, imageFiles, textFolder, opfFolder, (str) => str.Replace(folInfo.FullName, inFolder).Replace("/","\\"));
                }
                else
                {
                    var dirInfo = new DirectoryInfo($"{inFolder}\\item\\xhtml\\");
                    order = ProcessManga((str) => str.Replace(dirInfo.FullName, inFolder).Replace("/","\\"), series, volumeName, volOrder, ob, order, $"{inFolder}\\item\\xhtml\\");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return ob;
        }

        private static int ProcessManga(Func<string, string> inFolder, string series, string volumeName, int volOrder, Omnibus ob, int order, string textFolder)
        {
            var nav = File.ReadAllLines($"{textFolder}\\..\\nav.xhtml").Select(x => x.Trim()).ToList();
            var files = Directory.GetFiles(textFolder, "*.xhtml").ToList();

            var incontents = false;
            Chapter? chapter = null;
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
                            chapter!.Sources.Add(new Source { File = inFolder(Ext(x)), SortOrder = $"{volOrder:000}{order:00}{sourceOrder:000}" });
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
                            var index = files.IndexOf((files.FirstOrDefault(x => x.EndsWith(firstPage, StringComparison.InvariantCultureIgnoreCase)))!);
                            foreach (var x in files.Take(index))
                            {
                                chapter.Sources.Add(new Source { File = inFolder(Ext(x)), SortOrder = $"{volOrder:000}{order:00}{sourceOrder:000}" });
                                sourceOrder++;
                            }
                            files.RemoveRange(0, index);
                        }

                        var title = linesplit[2].Substring(1).Replace("</a></li>", string.Empty);
                        chapter = new Chapter
                        {
                            Name = title,
                        };
                        chapter.SortOrder = (volOrder * 100 + order).ToString("00000");
                        order++;
                        ob.Chapters[0].Chapters.Add(chapter);
                    }
                }
            }

            return order;
        }

        private static void ProcessLN(string volumeName, int volOrder, Omnibus ob, ref int order, Package content, List<string> imageFiles, string textFolder, string opfFolder, Func<string, string> inFolder)
        {
            List<string> chapterFiles = new List<string>();

            foreach (var line in content.Spine)
            {
                var item = content.Manifest.FirstOrDefault(x => x.Id.Equals(line.Id, StringComparison.InvariantCultureIgnoreCase))?.Href;

                if ((item!.Contains('.') && !(item.Contains(".xhtml") || item.Contains(".html")))
                    || item.StartsWith("\"signup")
                    || item.StartsWith("\"copyright"))
                {

                }
                else if (item.Contains('_') || item.Contains("insert"))
                {
                    chapterFiles.Add(item);
                }
                else
                {
                    if (chapterFiles.Any())
                    {
                        var chapter = new Chapter();
                        chapter.SortOrder = (volOrder * 100 + order).ToString("00000");
                        order++;
                        foreach (var c in chapterFiles)
                        {
                            //if (File.Exists(Ext($"{textFolder}\\{c}")))
                            //    chapter.Sources.Add(new Source { File = Ext($"{textFolder}\\{c}").Replace(inFolder, string.Empty), SortOrder = chapter.SortOrder + chapter.Sources.Count.ToString("00") });
                            if (File.Exists(Ext($"{opfFolder}\\{c}")))
                                chapter.Sources.Add(new Source { File = inFolder(Ext($"{opfFolder}\\{c}")), SortOrder = chapter.SortOrder + chapter.Sources.Count.ToString("00") });
                        }

                        var chapterContent = File.ReadAllText(chapter.Sources[0].File);
                        chapter.Name = chapterTitleRegex.Match(chapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                        if (string.IsNullOrWhiteSpace(chapter.Name)) chapter.Name = chapter.SortOrder;


                        AddChapter(chapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

                        foreach (var file in chapter.Sources.Skip(1))
                        {
                            chapterContent = string.Concat(chapterContent, File.ReadAllText(file.File));
                        }

                        Chapter? subChapter = null;
                        foreach (Match subHeader in chapterSubTitleRegex.Matches(chapterContent).Where(x => !string.Equals(x.Value, "<h2>I</h2>") && !string.Equals(x.Value, "<h2>1</h2>"))
                            .Union(chapterTitleRegex.Matches(chapterContent).Skip(1)).OrderBy(x => x.Index))
                        {
                            if (subChapter != null)
                            {
                                subChapter.EndsBeforeLine = subHeader.Value;
                            }
                            else
                            {
                                chapter.EndsBeforeLine = subHeader.Value;
                            }
                            subChapter = new Chapter
                            {
                                CType = chapter.CType,
                                StartsAtLine = subHeader.Value,
                                Name = subHeader.Value.Replace("<h2>", string.Empty).Replace("</h2>", string.Empty).Replace("<h1>", string.Empty).Replace("</h1>", string.Empty),
                                SortOrder = chapter.SortOrder + chapter.Chapters.Count.ToString("000"),
                                Sources = new System.Collections.ObjectModel.ObservableCollection<Source>(chapter.Sources)
                            };
                            chapter.Chapters.Add(subChapter);
                        }

                    }
                    chapterFiles.Clear();
                    chapterFiles.Add(item);
                }
            }

            var finalChapter = new Chapter { SortOrder = (volOrder * 100 + order).ToString("00000") };
            foreach (var c in chapterFiles)
            {
                //if (File.Exists(Ext($"{textFolder}\\{c}")))
                //    finalChapter.Sources.Add(new Source { File = Ext($"{textFolder}\\{c}"), SortOrder = finalChapter.SortOrder + finalChapter.Sources.Count.ToString("00") });
                //else 
                if (File.Exists(Ext($"{opfFolder}\\{c}")))
                    finalChapter.Sources.Add(new Source { File = inFolder(Ext($"{opfFolder}\\{c}")), SortOrder = finalChapter.SortOrder + finalChapter.Sources.Count.ToString("00") });
            }
            var finalChapterContent = File.ReadAllText(finalChapter.Sources[0].File);
            finalChapter.Name = chapterTitleRegex.Match(finalChapterContent).Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
            if (string.IsNullOrWhiteSpace(finalChapter.Name)) finalChapter.Name = finalChapter.SortOrder;
            AddChapter(finalChapter, ob.Chapters[0], volumeName, volOrder, imageFiles);

            foreach (var file in finalChapter.Sources.Skip(1))
            {
                finalChapterContent = string.Concat(finalChapterContent, File.ReadAllText(file.File));
            }

            Chapter? finalSubChapter = null;
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

        private static string Ext(string str)
        {
            if (str.EndsWith(".xhtml", StringComparison.InvariantCultureIgnoreCase)) return str;
            if (str.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase)) return str;
            return $"{str}.xhtml";
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
    [Serializable, XmlRoot(ElementName = "package", Namespace = "http://www.idpf.org/2007/opf")]
    [XmlType("package")]
    public class Package
    {
        [XmlElement(ElementName = "metadata")]
        public Metadata Metadata { get; set; } = new Metadata();

        [XmlArray(ElementName = "manifest")]
        public List<Item> Manifest { get; set; } = new List<Item>();

        [XmlArray(ElementName = "spine")]
        public List<Itemref> Spine { get; set; } = new List<Itemref>();
    }
    public class Metadata
    {

    }
    public class Item
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = String.Empty;
        [XmlAttribute("href")]
        public string Href { get; set; } = String.Empty;
    }
    public class Itemref
    {
        [XmlAttribute("idref")]
        public string Id { get; set; } = String.Empty;
    }
}
