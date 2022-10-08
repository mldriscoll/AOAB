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

namespace AOABO.OCR
{
    internal class OCR
    {
        static string overrideDirectory = Directory.GetCurrentDirectory() + "\\Overrides";
        static string tempDirectory = Directory.GetCurrentDirectory() + "\\temp";

        internal static async Task BuildOCROverrides(Login login)
        {
            if(!Directory.Exists(overrideDirectory)) Directory.CreateDirectory(overrideDirectory);
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
                string chapterContent = string.Empty;

                var OcrContent = new List<string>();
                foreach (var chapterFile in chapter.OriginalFilenames)
                {
                    try
                    {
                        if (OcrContent.Count > 0)
                        {
                            var last = OcrContent.Last();
                            OcrContent.Remove(last);

                            var newOcr = OCRX(chapterFile);

                            OcrContent.Add($"{last} {newOcr.First()}");

                            OcrContent.AddRange(newOcr.Skip(1));
                        }
                        else
                        {
                            OcrContent.AddRange(OCRX(chapterFile));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{ex.Message} while processing file {chapterFile}", ex);
                    }
                }

                var oContent = new List<string>();
                foreach (var line in OcrContent)
                {
                    var l = line;
                    foreach (var correction in chapter.OCR.Corrections)
                    {
                        l = l.Replace(correction.Original, correction.Replacement);
                    }

                    foreach (var italic in chapter.OCR.Italics)
                    {
                        l = l.Replace(italic.Start, $"<i>{italic.Start}").Replace(italic.End, $"{italic.End}</i>");
                    }
                    oContent.Add(l);
                }


                var content = oContent.Skip(1).Aggregate($"<h1>{OcrContent[0]}</h1>", (agg, s) => String.Concat(agg, $"\r\n<p>{s}</p>"));
                chapterContent = File.ReadAllText("OCR\\OCRTemplate.txt").Replace("[Content]", content);

                File.WriteAllText(overrideDirectory + "\\" + chapter.ChapterName + ".xhtml", chapterContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing chapter {chapter.ChapterName}");
                Console.WriteLine(ex.ToString());
            }
        }

        private static List<string> OCRX(string imageNumber)
        {
            List<string> text = new List<string>();
            var file = $"{tempDirectory}\\item\\image\\i-{imageNumber:000}.jpg";

            var Ocr = new IronTesseract();
            Ocr.Configuration.EngineMode = TesseractEngineMode.LstmOnly;
            using (var Input = new OcrInput(file))
            {
                var Result = Ocr.Read(Input);

                foreach (var block in Result.Blocks)
                {
                    foreach (var paragraph in block.Paragraphs)
                    {
                        var f = paragraph.Lines[0].Words[0].Characters[0].Font;
                        text.Add(paragraph.Lines.Aggregate(string.Empty, (s, a) => string.Concat(s, a, " ")));
                    }

                    if (!block.Equals(Result.Blocks.Last()))
                    {
                        text.Add("<br/><br/>");
                    }
                }

                Console.WriteLine(Result.Text);
            }
            return text;
        }
    }
}
