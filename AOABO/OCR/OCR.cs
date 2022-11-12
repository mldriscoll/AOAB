using AOABO.Config;
using AOABO.Downloads;
using IronOcr;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Ocr;

namespace AOABO.OCR
{
    internal class OCR
    {
        static string overrideDirectory = Directory.GetCurrentDirectory() + "\\Overrides";
        static string tempDirectory = Directory.GetCurrentDirectory() + "\\temp";

        internal static async Task BuildOCROverrides(Login login)
        {
            IronOcr.Installation.LicenseKey = "IRONOCR.FREETRIAL.20322-486E55E083-ACZD3RTYRBA4OGZW-GWGJKFOCQ5EZ-7PYKKXLHQ6ID-KEUSYXK4JIOZ-4NTZKQN5XAA3-PHVKHJ-TJLFVJDIJW6IEA-DEPLOYMENT.TRIAL-SVDJ2L.TRIAL.EXPIRES.05.DEC.2022";


            if (!Directory.Exists(overrideDirectory)) Directory.CreateDirectory(overrideDirectory);
            if(Directory.Exists(tempDirectory)) Directory.Delete(tempDirectory, true);

            foreach(var vol in Configuration.Volumes.Where(x => x.OCR))
            {
                Directory.CreateDirectory(tempDirectory);
                await Download(vol.InternalName, login);

                ZipFile.ExtractToDirectory(tempDirectory + "\\temp.epub", tempDirectory);

                foreach (var chapter in vol.Chapters.Where(x => x.OCR != null))
                {
                    await DoOCR(chapter);
                }
                Directory.Delete(tempDirectory, true);
            }

        }

        private static async Task Download(string volume, Login login)
        {
            var volname = Configuration.VolumeNames.First(x => x.InternalName.Equals(volume));

            await Downloader.DownloadSpecificVolume(volname.ApiSlug, login.AccessToken, tempDirectory + "\\temp.epub", new HttpClient());
        }

        private static async Task DoOCR(Chapter chapter)
        {
            try
            {
                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(OcrEngine.AvailableRecognizerLanguages.First(x => x.LanguageTag.Equals("en-US")));

                string chapterContent = string.Empty;

                var OcrContent = new List<string>();
                foreach (var chapterFile in chapter.OriginalFilenames)
                {
                    var memStream = new MemoryStream();
                    using (var stream = File.OpenRead($"{tempDirectory}\\item\\image\\i-{chapterFile:000}.jpg"))
                    {
                        await stream.CopyToAsync(memStream);
                    }
                    var bd = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());

                    var result = await ocrEngine.RecognizeAsync(await bd.GetSoftwareBitmapAsync());
                    try
                    {
                        if (result.Lines.Count > 0)
                        {
                            var oldTop = 0.0;
                            foreach (var line in result.Lines)
                            {
                                if (line.Words[0].BoundingRect.Left < 350)
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
                var header = chapter.OCR.Header ?? OcrContent.First();
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

                File.WriteAllText(overrideDirectory + "\\" + chapter.ChapterName + ".xhtml", chapterContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing chapter {chapter.ChapterName}");
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
