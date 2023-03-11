using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Core.Processor
{
    public class Processor
    {
        public List<CSS> CSS = new List<CSS>();
        public List<Chapter> Chapters = new List<Chapter>();
        public List<Image> Images = new List<Image>();
        public List<NavPoint> NavPoints = new List<NavPoint>();
        public List<string> Metadata = new List<string>();

        public async Task FullOutput(string baseFolder, bool textOnly, bool humanReadable, bool deleteFolder, string name, int? maxX = null, int? maxY = null, int imageQuality = 90)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            var folder = $"{baseFolder}\\temp";
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(folder + "\\oebps");
            Directory.CreateDirectory(folder + "\\META-INF");

            File.WriteAllText($"{folder}\\mimetype", "application/epub+zip");
            File.WriteAllText($"{folder}\\META-INF\\container.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\"><rootfiles><rootfile full-path=\"oebps/content.opf\" media-type=\"application/oebps-package+xml\"/></rootfiles></container>");

            foreach (var chapter in Chapters)
            {
                foreach (var imMatch in imageRegex.Matches(chapter.Contents))
                {
                    var match = (Match)imMatch;

                    var imageFile = match.Value.Replace("src=\"", "").Replace("xlink:href=\"", "").Replace("[ImageFolder]/", string.Empty).Replace("\"", "");
                    var im = Images.FirstOrDefault(x => x.Name.Equals(imageFile, StringComparison.OrdinalIgnoreCase));

                    if (im != null)
                    {
                        im.Referenced = true;
                    }
                }
            }

            File.WriteAllText(folder + "\\oebps\\css.css", CSS.Aggregate(string.Empty, (file, css) => string.Concat(file, $"{css.Name} {css.Contents}\r\n")));
            List<string> manifest = new List<string>();
            List<string> spine = new List<string>();
            manifest.Add($"    <item id={"\""}css{"\""} href={"\""}css.css{"\""} media-type={"\""}text/css{"\""}/>");

            if (!textOnly)
            {
                Directory.CreateDirectory(folder + "\\oebps\\images");

                foreach (var im in Images.Where(x => x.Referenced))
                {
                    if (maxX.HasValue || maxY.HasValue)
                    {
                        var scale = 1m;
                        var image = await SixLabors.ImageSharp.Image.LoadAsync(im.OldLocation);

                        if (maxX.HasValue && image.Width > maxX)
                        {
                            scale = image.Width / (decimal)maxX.Value;
                        }

                        if (maxY.HasValue && image.Height > maxY)
                        {
                            var yScale = image.Height / (decimal)maxY.Value;

                            scale = scale > yScale ? scale : yScale;
                        }

                        if (scale > 1)
                        {
                            image.Mutate(
                                i => i.Resize((int)(image.Width / scale), (int)(image.Height / scale)));
                            await image.SaveAsJpegAsync(folder + "\\oebps\\images\\" + im.Name, new JpegEncoder { Quality = imageQuality });
                        }
                        else
                        {
                            File.Copy(im.OldLocation, folder + "\\oebps\\images\\" + im.Name);
                        }
                    }
                    else
                    {
                        File.Copy(im.OldLocation, folder + "\\oebps\\images\\" + im.Name);
                    }
                    manifest.Add($"    <item id={"\""}im{Images.IndexOf(im)}{"\""} href={"\""}images/{im.Name}{"\""} media-type={"\""}image/jpeg{"\""}/>");
                }
            }

            int tocCounter = 0;

            Directory.CreateDirectory($"{folder}\\oebps\\Text");
            foreach (var chapter in Chapters.OrderBy(x => x.SubFolder + "\\" + x.SortOrder))
            {
                string cssLink = "../";
                var fileName = humanReadable ? chapter.FileName : $"{tocCounter}.xhtml";
                string imFolderReplace;

                if (humanReadable && !string.IsNullOrWhiteSpace(chapter.SubFolder))
                {
                    var li = chapter.SubFolder.LastIndexOf('\\');
                    if (li > 0)
                    {
                        var subdir = chapter.SubFolder;
                        cssLink = subdir.Split('\\').Aggregate("../", (agg, str) => string.Concat(agg, "../"));
                        imFolderReplace = subdir.Split('\\').Aggregate("../images", (agg, str) => string.Concat("../", agg));
                        Directory.CreateDirectory($"{folder}\\oebps\\Text\\{subdir}");
                    }
                    else
                    {
                        cssLink = "../";
                        imFolderReplace = "../../images";
                        Directory.CreateDirectory($"{folder}\\oebps\\Text\\{chapter.SubFolder}");
                    }
                }
                else
                {
                    imFolderReplace = "../images";
                }

                var a = humanReadable ? chapter.SubFolder + "\\" : string.Empty;
                var fullFileName = $"{folder}\\oebps\\Text\\{a + fileName}";

                while (File.Exists(fullFileName))
                {
                    chapter.SortOrder = chapter.SortOrder + "x";
                }

                var subFolderSplit = chapter.SubFolder.Split('\\');
                List<NavPoint> nps = NavPoints;

                foreach (var fold in subFolderSplit)
                {
                    if (fold.Equals("text", StringComparison.InvariantCultureIgnoreCase)) continue;
                    var index = fold.IndexOf('-');
                    string folderName;
                    if (index == -1)
                    {
                        folderName = fold;
                    }
                    else
                    {
                        folderName = fold.Substring(index + 1);
                    }

                    if (!string.IsNullOrWhiteSpace(chapter.SubFolder))
                    {
                        var np = nps.FirstOrDefault(x => x.Label.Equals(folderName));
                        if (np == null)
                        {
                            np = new NavPoint { Label = folderName, Source = Uri.EscapeDataString($"Text/{a + fileName}".Replace('\\', '/').Replace(":", "").Replace(" ", "")), Id = tocCounter };
                            tocCounter++;
                            nps.Add(np);
                        }
                        nps = np.navPoints;
                    }
                }

                if (textOnly)
                {
                    chapter.Contents = ImgRemover.Replace(chapter.Contents, string.Empty);
                }
                else
                {
                    chapter.Contents = chapter.Contents.Replace("[ImageFolder]", imFolderReplace);
                }

                nps.Add(new NavPoint { Label = chapter.Name, Source = Uri.EscapeDataString($"Text/{a + fileName}".Replace('\\', '/').Replace(":", "").Replace(" ", "")), Id = tocCounter });
                tocCounter++;


                File.WriteAllText(fullFileName, $@"<?xml version='1.0' encoding='utf-8'?>
<html xmlns={"\""}http://www.w3.org/1999/xhtml{"\""} xmlns:epub={"\""}http://www.idpf.org/2007/ops{"\""} xml:lang={"\""}en{"\""}>
  <head>
    <title>{name}</title>
    <meta http-equiv={"\""}Content-Type{"\""} content={"\""}text/html; charset=utf-8{"\""} />
  <link rel={"\""}stylesheet{"\""} type={"\""}text/css{"\""} href={"\""}{cssLink}css.css{"\""} />
</head>{ chapter.Contents}</html>");
                manifest.Add($"    <item id={"\""}id{Chapters.IndexOf(chapter)}{"\""} href={"\""}Text/{(a + fileName).Replace('\\', '/').Replace(":", "").Replace(" ", "")}{"\""} media-type={"\""}application/xhtml+xml{"\""}/>");
                spine.Add($"    <itemref idref={"\""}id{Chapters.IndexOf(chapter)}{"\""}/>");
            }

            manifest.Add("    <item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");

            File.WriteAllText($"{folder}\\oebps\\content.opf",
    $@"<?xml version={"\""}1.0{"\""} encoding={"\""}UTF-8{"\""}?>
<package xmlns={"\""}http://www.idpf.org/2007/opf{"\""} version={"\""}3.0{"\""} unique-identifier={"\""}pub-id{"\""} xml:lang={"\""}en{"\""}>
  <metadata xmlns:opf={"\""}http://www.idpf.org/2007/opf{"\""} xmlns:dc={"\""}http://purl.org/dc/elements/1.1/{"\""}>
{Metadata.Aggregate(string.Empty, (agg, str) => string.Concat(agg, str, "\r\n"))}
  </metadata>
  <manifest>
{manifest.Aggregate(string.Empty, (agg, str) => string.Concat(agg, str, "\r\n"))}  </manifest>
  <spine toc={"\""}ncx{"\""}>
{spine.Aggregate(string.Empty, (agg, str) => string.Concat(agg, str, "\r\n"))}  </spine>
  <guide>
    <reference type={"\""}cover{"\""} href={"\""}00-Cover/00-Cover.xhtml{"\""} title={"\""}Cover{"\""}/>    
  </guide>
</package>
");

            File.WriteAllText($"{folder}\\oebps\\toc.ncx", $"<?xml version='1.0' encoding='utf-8'?>\r\n<ncx xmlns=\"http://www.daisy.org/z3976/2005/ncx/\" version=\"2005-1\" xml:lang=\"en\">\r\n  <head>\r\n    <meta name=\"dtb:depth\" content=\"{NavPoints.Max(x => x.MaxTabs) + 2}\" />\r\n  </head>\r\n  <docTitle>\r\n    <text>{name}</text>\r\n  </docTitle>\r\n  <navMap>\r\n"
                + NavPoints.Aggregate(string.Empty, (agg, np) => string.Concat(agg, np, "\r\n")) + "  </navMap>\r\n</ncx>");

            if (File.Exists($"{baseFolder}\\{name}.epub")) File.Delete($"{baseFolder}\\{name}.epub");
            var archive = ZipFile.Open($"{baseFolder}\\{name}.epub", ZipArchiveMode.Create);
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith("mimetype"))
                {
                    archive.CreateEntryFromFile(file, "mimetype", CompressionLevel.NoCompression);
                }
                else
                {
                    var target = file.Replace(folder + "\\", string.Empty).Replace("\\", "/").Replace(":", "").Replace(" ", "");
                    archive.CreateEntryFromFile(file, target, CompressionLevel.Optimal);
                }
            }
            archive.Dispose();
            if (deleteFolder)
            {
                Directory.Delete(folder, true);
            }
        }

        static Regex ImgRemover = new Regex("<img.*?\\/>");

        public void UnpackFolder(string folder)
        {
            LoadCSS(folder);

            LoadImages(folder);

            LoadText(folder);
        }

        private void LoadCSS(string folder)
        {
            var cssFiles = Directory.GetFiles(folder, "*.css", SearchOption.AllDirectories);
            var classStartRegex = new Regex("[A-Z,a-z,.]");

            foreach (var f in cssFiles)
            {
                var text = File.ReadAllText(f);
                int start = -1;
                int space = -1;
                int open = -1;
                for (int counter = 0; counter < text.Length; counter++)
                {
                    if (start == -1 && classStartRegex.IsMatch(text.Substring(counter, 1)))
                    {
                        start = counter;
                    }

                    if (start == -1) continue;

                    if (space == -1 && text[counter].Equals(' '))
                    {
                        space = counter - 1;
                    }

                    if (space == -1) continue;

                    if (open == -1 && text[counter].Equals('{'))
                    {
                        open = counter;
                    }

                    if (open == -1) continue;

                    if (text[counter].Equals('}'))
                    {
                        var contents = text.Substring(open, counter - open + 1);
                        var match = CSS.FirstOrDefault(x => x.Contents.Equals(contents));
                        if (match != null)
                        {
                            match.OldNames.Add($"{f}:{text.Substring(start, space - start + 1)}");
                        }
                        else
                        {
                            var substring = text.Substring(start, space - start + 1);

                            if (substring.StartsWith('.'))
                            {
                                var newCSS = new CSS
                                {
                                    Contents = contents,
                                    Name = $".Style{CSS.Count}",
                                    OldNames = new List<string>
                                    {
                                    $"{f}:{substring}"
                                    }
                                };

                                CSS.Add(newCSS);
                            }
                            else
                            {
                                var newCSS = new CSS
                                {
                                    Contents = contents,
                                    Name = substring,
                                    OldNames = new List<string>
                                    {
                                    $"{f}:{substring}"
                                    }
                                };

                                CSS.Add(newCSS);
                            }
                        }

                        start = -1;
                        space = -1;
                        open = -1;
                    }
                }
            }
        }

        private void LoadImages(string folder)
        {
            var imageFiles = Directory.GetFiles(folder, "*.jpg", SearchOption.AllDirectories)
                .Union(Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
                .Union(Directory.GetFiles(folder, "*.jpeg", SearchOption.AllDirectories))
                .ToList();

            foreach (var im in imageFiles)
            {
                var oldName = im.Split('\\').Last();
                var extension = oldName.Split('.').Last();
                Images.Add(new Image
                {
                    OldLocation = im,
                    Name = $"{Images.Count:0000}.{extension}"
                });
            }
        }

        private void LoadText(string folder)
        {
            var files = GetHtmlFiles(folder).OrderBy(x => x).ToList();

            foreach (var f in files)
            {
                ImportHtmlFile(folder, f);
            }
        }

        private static List<string> GetHtmlFiles(string folder)
        {
            var list = new List<string>();
            list.AddRange(Directory.GetFiles(folder, "*.html"));
            list.AddRange(Directory.GetFiles(folder, "*.xhtml"));

            foreach (var d in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
            {
                list.AddRange(GetHtmlFiles(d));
            }

            return list;
        }

        static Regex cssLinks = new Regex("href=\".*css\"");
        static Regex bodyRegex = new Regex("<body[\\s\\S]*</body>");
        static Regex classRegex = new Regex("class=\".*?\"");
        static Regex imageRegex = new Regex("(src|xlink:href)=\".*?\"");

        private void ImportHtmlFile(string baseFolder, string file)
        {
            var chapter = new Chapter { CssFiles = new List<string>() };
            Chapters.Add(chapter);

            var text = File.ReadAllText(file);
            var folders = file.Replace(baseFolder + "\\", string.Empty).Split('\\').ToList();
            var last = folders.Last();
            var index = last.IndexOf('-');
            if (index == -1)
            {
                chapter.Name = last;
                chapter.SortOrder = "000";
            }
            else
            {
                chapter.Name = last.Substring(index + 1);
                chapter.SortOrder = last.Substring(0, index);
            }
            folders.Remove(folders.Last());
            chapter.SubFolder = folders.Count > 0 ? folders.Aggregate(string.Empty, (agg, dir) => string.Concat(agg, "\\", dir)).Substring(1) : string.Empty;

            foreach (var cssLinkObject in cssLinks.Matches(text))
            {
                var cssLink = (Match)cssLinkObject;
                var link = text.Substring(cssLink.Index, cssLink.Length).Replace("href=\"", string.Empty).Replace("\"", string.Empty).Replace(" rel=stylesheet type=text/css", string.Empty);
                var folderSplit = link.Replace(baseFolder, string.Empty).Split('/').ToList();

                var fo = new List<string>(folders);
                foreach (var dir in folderSplit)
                {
                    if (dir.Equals(".."))
                    {
                        fo.Remove(fo.Last());
                    }
                    else
                    {
                        fo.Add(dir);
                    }
                }
                chapter.CssFiles.Add(fo.Aggregate(baseFolder, (agg, dir) => string.Concat(agg, "\\", dir)));
            }

            if (chapter.CssFiles.Count == 0)
            {
                chapter.CssFiles.Add(baseFolder + "\\css.css");
            }

            chapter.Contents = bodyRegex.Match(text).Value;
            List<Tuple<string, string>> CssClassReplacements = new List<Tuple<string, string>>();

            foreach (Match match in classRegex.Matches(chapter.Contents))
            {
                var cssclassname = match.Value.Substring(match.Value.IndexOf(' ') + 1).Replace("class=\"", string.Empty).Replace("\"", string.Empty);

                var names = cssclassname.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var name in names)
                {
                    var css = CSS.FirstOrDefault(x => chapter.CssFiles.Select(y => y + ":." + name).Any(y => x.OldNames.Any(z => z.Equals(y))));
                    if (css == null)
                    {
                        css = CSS.FirstOrDefault(x => chapter.CssFiles.Select(y => y + ":" + name).Any(y => x.OldNames.Any(z => z.Equals(y))));
                    }

                    if (css != null)
                    {
                        CssClassReplacements.Add(new Tuple<string, string>(name, css.Name));
                    }
                }
            }

            foreach (Match match in classRegex.Matches(chapter.Contents))
            {
                var repl = match.Value;
                foreach (var rep in CssClassReplacements.GroupBy(x => x.Item2, x => x.Item1))
                {
                    foreach (var orig in rep.Distinct())
                    {
                        repl = repl.Replace(orig, rep.Key);
                    }
                }

                chapter.Contents = chapter.Contents.Replace(match.Value, repl);
            }

            List<Tuple<string, string>> ImageReplacements = new List<Tuple<string, string>>();
            foreach (var imMatch in imageRegex.Matches(chapter.Contents))
            {
                var match = (Match)imMatch;

                var imageFile = match.Value.Replace("src=\"", string.Empty).Replace("xlink:href=\"", string.Empty).Replace("\"", string.Empty);
                var loc = new List<string>(folders);
                foreach (var imFileBit in imageFile.Split('/'))
                {
                    if (imFileBit.Equals(".."))
                    {
                        loc.Remove(loc.Last());
                    }
                    else
                    {
                        loc.Add(imFileBit);
                    }
                }

                var imFileLocation = loc.Aggregate(baseFolder, (agg, str) => string.Concat(agg, "\\", str));
                var im = Images.FirstOrDefault(x => x.OldLocation.Equals(imFileLocation, StringComparison.OrdinalIgnoreCase));

                if (im == null)
                {
                    if (imageFile.StartsWith("[ImageFolder]"))
                    {
                        im = Images.FirstOrDefault(x => x.OldLocation.Equals(baseFolder + "\\images\\" + imageFile.Split('/').Last(), StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (im != null)
                {
                    ImageReplacements.Add(new Tuple<string, string>(imageFile, $"[ImageFolder]/{im.Name}"));
                }
            }

            foreach (var rep in ImageReplacements.Where(x => !x.Item1.Equals(x.Item2)).GroupBy(x => x.Item2, x => x.Item1))
            {
                foreach (var orig in rep.Distinct())
                {
                    chapter.Contents = chapter.Contents.Replace(orig, rep.Key);
                }
            }
        }
    }
}