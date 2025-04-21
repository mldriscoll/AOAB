using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Downloads
{
    public class Downloader
    {
        public async static Task DoDownloads(HttpClient client, string token, string inputFolder, IEnumerable<Name> names, MangaQuality mangaQuality)
        {
            Console.Clear();
            Console.WriteLine("Downloading Updated .epub files");
            Console.WriteLine("Unfortunately, the j-novel.club api does not track manga updates, so manga will only be downloaded if a file is missing.");
            Console.WriteLine();

            var library = await GetLibrary(client, token);
            var epubs = Directory.GetFiles(inputFolder, "*.epub");
            foreach (var fileName in names)
            {
                try
                {
                    var match = fileName.NameMatch(epubs);
                    var libraryBook = library.books.FirstOrDefault(x => x.volume.slug.Equals(fileName.ApiSlug, StringComparison.InvariantCultureIgnoreCase));
                    if (libraryBook == null) continue;
                    if (string.IsNullOrEmpty(match))
                    {
                        await doDownload(libraryBook, fileName, client, inputFolder, mangaQuality);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(libraryBook.lastDownload)) continue;
                        else
                        {
                            DateTime downloaded = DateTime.Parse(libraryBook.lastDownload);
                            DateTime updated = DateTime.Parse(libraryBook.lastUpdated);
                            var finfo = new FileInfo(match);
                            var diff = finfo.LastWriteTime.Subtract(downloaded);
                            var isLastDownload = diff.TotalSeconds < 30 && diff.TotalSeconds > -30;

                            if (downloaded > updated && isLastDownload) continue;

                            if (!isLastDownload)
                            {
                                Console.WriteLine($"File {fileName.FileName} may have been modified both locally and on j-novel club. Do you wish to update this file, Y/N?");
                                var yn = Console.ReadKey();
                                Console.WriteLine();
                                if (yn.KeyChar.Equals('y') || yn.KeyChar.Equals('Y'))
                                {
                                    await doDownload(libraryBook, fileName, client, inputFolder, mangaQuality);
                                }
                            }
                            else
                            {
                                await doDownload(libraryBook, fileName, client, inputFolder, mangaQuality);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static async Task<LibraryResponse> GetLibrary(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var libraryCall = await client.GetAsync("https://labs.j-novel.club/app/v2/me/library?format=json");
            LibraryResponse? library;
            using (var loginStream = await libraryCall.Content.ReadAsStreamAsync())
            {
                var deserializer = new DataContractJsonSerializer(typeof(LibraryResponse));
                library = deserializer.ReadObject(loginStream) as LibraryResponse;
            }

            if (library == null) throw new Exception("Failed to load j-novel.club library");

            client.DefaultRequestHeaders.Authorization = null;

            return library;
        }

        public static async Task<SeriesList> GetSeriesList(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            SeriesList list = new SeriesList { series = new List<SeriesListResponse>() };
            bool cont = true;
            int skip = 0;
            while (cont)
            {
                var libraryCall = await client.GetAsync($"https://labs.j-novel.club/app/v2/series?format=json&skip={skip}");

                using (var loginStream = await libraryCall.Content.ReadAsStreamAsync())
                {
                    var deserializer = new DataContractJsonSerializer(typeof(SeriesList));
                    var l = deserializer.ReadObject(loginStream) as SeriesList;

                    if (l.series.Count < 50)
                    {
                        cont = false;
                    }

                    skip += l.series.Count;
                    list.series.AddRange(l.series);
                }
            }

            if (list == null) throw new Exception("Failed to load j-novel.club series");

            client.DefaultRequestHeaders.Authorization = null;

            return list;
        }

        public static async Task<SeriesVolumesResponse> GetSeries(HttpClient client, string slug)
        {
            var libraryCall = await client.GetAsync($"https://labs.j-novel.club/app/v2/series/{slug}/volumes?format=json");
            SeriesVolumesResponse? library;
            using (var loginStream = await libraryCall.Content.ReadAsStreamAsync())
            {
                var deserializer = new DataContractJsonSerializer(typeof(SeriesVolumesResponse));
                library = deserializer.ReadObject(loginStream) as SeriesVolumesResponse;
            }

            if (library == null) throw new Exception("Failed to load j-novel.club library");

            return library;
        }


        static Regex mangaSizeRegex = new Regex("\\?height=.*&");

        private async static Task doDownload(LibraryResponse.Book book, Name name, HttpClient client, string folder, MangaQuality mangaQuality)
        {
            LibraryResponse.Book.Download? download = null;
            if (book.downloads.Count < 1) return;

            Console.WriteLine($"Downloading {name.FileName}");

            if (book.downloads.Count > 1)
            {
                switch (mangaQuality)
                {
                    case MangaQuality.Mobile:
                        download = book.downloads.FirstOrDefault(x => x.label.EndsWith("(Mobile)"));
                        break;
                    case MangaQuality.Desktop:
                        download = book.downloads.FirstOrDefault(x => x.label.EndsWith("(Desktop)"));
                        break;
                    case MangaQuality.FourK:
                        download = book.downloads.FirstOrDefault(x => x.label.EndsWith("(4K)"));
                        break;
                }
                if (download == null)
                {
                    Console.WriteLine("Which version do you want to download?");
                    for (int i = 0; i < book.downloads.Count; i++)
                    {
                        Console.WriteLine($"{i} - {book.downloads[i].label}");
                    }
                    var str = Console.ReadKey().KeyChar.ToString();
                    var key = int.Parse(str);

                    download = book.downloads[key];
                }

                var size = mangaSizeRegex.Match(download.link).ToString().Replace("?height=", string.Empty).Replace("&", string.Empty);
                name.FileName = string.Format(name.FileName, size);
            }
            else
            {
                download = book.downloads[0];
            }

            using (var stream = await client.GetStreamAsync(download.link))
            {
                var fileName = $"{folder}\\{name.FileName}";
                if (File.Exists(fileName)) File.Delete(fileName);
                using (var fileStream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        public async static Task DownloadSpecificVolume(string slug, string token, string fileName, HttpClient client)
        {
            var library = await GetLibrary(client, token);

            var libraryBook = library.books.FirstOrDefault(x => x.volume.slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase));

            if (libraryBook == null) return;

            var download = libraryBook.downloads.Last();

            using (var stream = await client.GetStreamAsync(download.link))
            {
                using (var filestream = File.OpenWrite(fileName))
                {
                    await stream.CopyToAsync(filestream);
                }
            }
        }
    }
}
