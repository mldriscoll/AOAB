using AOABO.Chapters;
using AOABO.Config;
using Core;
using Core.Downloads;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Media.Ocr;

namespace AOABO.OCR
{
    internal class OCR
    {
        static string tempDirectory = Directory.GetCurrentDirectory() + "\\temp";

        internal static async Task BuildOCROverrides(Login login)
        {
            var inputFolder = string.IsNullOrWhiteSpace(Configuration.Options.Folder.InputFolder) ? Directory.GetCurrentDirectory() :
                Configuration.Options.Folder.InputFolder.Length > 1 && Configuration.Options.Folder.InputFolder[1].Equals(':') ? Configuration.Options.Folder.InputFolder : Directory.GetCurrentDirectory() + "\\" + Configuration.Options.Folder.InputFolder;

            var overrideDirectory = inputFolder + "\\Overrides";

            if (!Directory.Exists(overrideDirectory)) Directory.CreateDirectory(overrideDirectory);
            if(Directory.Exists(tempDirectory)) Directory.Delete(tempDirectory, true);

            foreach(var vol in Configuration.Volumes.Where(x => x.OCR))
            {
                Directory.CreateDirectory(tempDirectory);
                await Download(vol.InternalName, login);

                ZipFile.ExtractToDirectory(tempDirectory + "\\temp.epub", tempDirectory);

                foreach (var chapter in vol.BonusChapters.Where(x => x.OCR != null))
                {
                    await DoOCR(chapter, overrideDirectory);
                }
                Directory.Delete(tempDirectory, true);
            }

        }

        private static async Task Download(string volume, Login login)
        {
            var volname = Configuration.VolumeNames.First(x => x.InternalName.Equals(volume));

            await Downloader.DownloadSpecificVolume(volname.ApiSlug, login.AccessToken, tempDirectory + "\\temp.epub", new HttpClient());
        }

        public static Color GetPixel(int x, int y, BitmapData data, Byte[] Pixels, int Bpp)
        {
            Color clr = Color.Empty;

            int i = (y * data.Stride) + (x * Bpp);

            if (i > Pixels.Length - Bpp)
                throw new IndexOutOfRangeException();

            if (Bpp == 4) 
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3];
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Bpp == 3)
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Bpp == 1)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        private static int GetBPP(PixelFormat f)
        {
            switch (f)
            {
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                default:
                    throw new Exception("Unknown Bitmap Format");
            }
        }

        private static async Task DoOCR(BonusChapter chapter, string overrideDirectory)
        {
            try
            {
                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(OcrEngine.AvailableRecognizerLanguages.First(x => x.LanguageTag.StartsWith("en-")));

                string chapterContent = string.Empty;

                var OcrContent = new List<string>();
                bool firstPage = true;
                foreach (var chapterFile in chapter.OriginalFilenames)
                {
                    var filename = $"{tempDirectory}\\item\\image\\i-{chapterFile:000}.jpg";

                    if (chapter.OCR?.Crop ?? false)
                    {
                        var minX = int.MaxValue; 
                        var maxX = int.MinValue;
                        var minY = int.MaxValue;
                        var maxY = int.MinValue;

                        using (Bitmap b = new Bitmap(filename))
                        {
                            var data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, b.PixelFormat);

                            var bpp = GetBPP(b.PixelFormat);
                            Color white = Color.FromArgb(255, 255, 255, 255);

                            var Pixels = new byte[data.Stride * b.Height];
                            Marshal.Copy(data.Scan0, Pixels, 0, Pixels.Length);

                            for (int y = 0; y < b.Height; y++)
                            {
                                for (int x = 0; x < b.Width; x++)
                                {
                                    if (GetPixel(x, y, data, Pixels, bpp) != white)
                                    {
                                        if (x < minX) minX = x;
                                        if (x > maxX) maxX = x;
                                        if (y < minY) minY = y;
                                        if (y > maxY) maxY = y;
                                    }
                                }
                            }

                            minX /= 2;
                            minY /= 2;
                            maxX = b.Width - ((b.Width - maxX) / 2);
                            maxY = b.Height - ((b.Height - maxY) / 2);
                        }

                        using (var a = new ImageProcessor.ImageFactory())
                        {
                            a.Load(filename);
                            a.Crop(new Rectangle(minX / 2, minY / 2, maxX - (minX / 2), maxY - (minY / 2)));
                            a.Save(filename);
                        }
                    }

                    var memStream = new MemoryStream();
                    using (var stream = File.OpenRead(filename))
                    {
                        await stream.CopyToAsync(memStream);
                    }
                    var bd = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());

                    var result = await ocrEngine.RecognizeAsync(await bd.GetSoftwareBitmapAsync());
                    try
                    {
                        if (result.Lines.Count > 0)
                        {
                            var leftmost = result.Lines.Min(x => x.Words[0].BoundingRect.Left);
                            var rightmost = result.Lines.OrderByDescending(x => x.Words[0].BoundingRect.Left).Skip(firstPage ? chapter.OCR.HeaderLines : 0).First().Words[0].BoundingRect.Left;
                            var threshold = leftmost + ((rightmost - leftmost) / 2);
                            firstPage = false;

                            var oldTop = 0.0;
                            foreach (var line in result.Lines)
                            {
                                if (line.Words[0].BoundingRect.Left < threshold)
                                {
                                    OcrContent.Add(line.Text);
                                }
                                else if (line.Words[0].BoundingRect.Top - oldTop >= 150 && oldTop > 0)
                                {
                                    OcrContent.Add($"</p>\r\n<br />\r\n<p>{line.Text}");
                                }
                                else
                                {
                                    OcrContent.Add($"</p>\r\n<p>{line.Text}");
                                }
                                oldTop = line.Words[0].BoundingRect.Top;
                            }
                            OcrContent.First().Replace("</p>\r\n<p>", string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{ex.Message} while processing file {chapterFile}", ex);
                    }
                }

                var previous = string.Empty;
                var header = chapter.OCR?.Header ?? OcrContent.First();
                var body = OcrContent.Skip(1).Aggregate(string.Empty, (agg, s) => string.Concat(agg, " ", s));

                var speechRegex = new Regex("\".*?\"");
                body = speechRegex.Replace(body, new MatchEvaluator(ReplaceSpeechMarks));

                var apostropheRegex = new Regex("[a-z,A-Z]'[a-z,A-Z]");
                body = apostropheRegex.Replace(body, new MatchEvaluator(ReplaceApostrophe));

                foreach (var correction in chapter.OCR.Corrections)
                {
                    body = body.Replace(correction.Original, correction.Replacement);
                }

                foreach(var italic in chapter.OCR.Italics)
                {
                    body = body.Replace(italic.Start, $"<i>{italic.Start}").Replace(italic.End, $"{italic.End}</i>");
                }

                var content = $"<h1>{header}</h1>\r\n<p>{body}</p>";
                chapterContent = File.ReadAllText("OCR\\OCRTemplate.txt").Replace("[Content]", content);

                File.WriteAllText(overrideDirectory + "\\" + chapter.OverrideName + ".xhtml", chapterContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing chapter {chapter.OverrideName}");
                Console.WriteLine(ex.ToString());
            }
        }

        public static string ReplaceSpeechMarks(Match m)
        {
            return "“" + m.ToString().Substring(1, m.ToString().Length-2) + "”";
        }

        public static string ReplaceApostrophe(Match m)
        {
            return m.ToString().Replace("'", "’");
        }
    }
}
