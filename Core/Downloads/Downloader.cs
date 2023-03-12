using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Downloads
{
    public class Downloader
    {
        public async static Task DoDownloads(HttpClient client, string token, string inputFolder, IEnumerable<Name> names)
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
                        await doDownload(libraryBook, fileName, client, inputFolder);
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
                                    await doDownload(libraryBook, fileName, client, inputFolder);
                                }
                            }
                            else
                            {
                                await doDownload(libraryBook, fileName, client, inputFolder);
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

        private static async Task<LibraryResponse> GetLibrary(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var libraryCall = await client.GetAsync("https://labs.j-novel.club/app/v1/me/library?format=json");
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


        static Regex mangaSizeRegex = new Regex("\\?height=.*&");

        private async static Task doDownload(LibraryResponse.Book book, Name name, HttpClient client, string folder)
        {
            LibraryResponse.Book.Download download = null;
            if (book.downloads.Count < 1) return;

            Console.WriteLine($"Downloading {name.FileName}");

            if (book.downloads.Count > 1)
            {
                Console.WriteLine("Which version do you want to download?");
                for (int i = 0; i < book.downloads.Count; i++)
                {
                    Console.WriteLine($"{i} - {book.downloads[i].label}");
                }
                var str = Console.ReadKey().KeyChar.ToString();
                var key = int.Parse(str);

                download = book.downloads[key];

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
