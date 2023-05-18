using Core;
using Core.Downloads;
using OBB.JSONCode;
using System.Runtime.Serialization.Json;
using System.Text.Json;

namespace OBB
{
    public static class NewVolumes
    {
        public static async Task List()
        {
            Console.Clear();

            List<Series> seriesList;
            using (var reader = new StreamReader("JSON\\Series.json"))
            {
                var deserializer = new DataContractJsonSerializer(typeof(Series[]));
                seriesList = ((Series[])deserializer.ReadObject(reader.BaseStream)).ToList();
            }

            LibraryResponse? library = null;
            Login login = null;
            if (Settings.MiscSettings.DownloadBooks)
            {
                using (var client = new HttpClient())
                {
                    login = await Login.FromFile(client);
                    login = login ?? await Login.FromConsole(client);

                    if (login != null)
                    {
                        library = await Downloader.GetLibrary(client, login.AccessToken);
                    }
                }
            }

            if (library == null) return;

            foreach (var book in library.books.Where(x => x.downloads.Any() 
                && !x.volume.slug.StartsWith("ascendance-of-a-bookworm")
                && !x.volume.slug.StartsWith("seriously-seeking-sister")))
            {
                if (!seriesList.Any(x => x.Volumes.Any(y => y.ApiSlug.Equals(book.volume.slug))))
                {
                    Console.Clear();
                    Console.WriteLine(book.volume.slug);

                    Console.WriteLine("Add to series:");
                    var name = Console.ReadLine();
                    var series = seriesList.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (series == null)
                    {
                        Console.Write("Author Name:");
                        var author = Console.ReadLine();
                        Console.Write("Author Sort (Surname, Forename):");
                        var sort = Console.ReadLine().ToUpper();
                        series = new Series { Name = name, InternalName = name, Author = author, AuthorSort = sort, Volumes = new List<VolumeName>() };
                        File.WriteAllText($"JSON\\{name}.json", "[]");
                        seriesList.Add(series);
                    }
                    var volName = new VolumeName { ApiSlug = book.volume.slug, EditedBy = null, FileName = $"{book.volume.slug}.epub", ShowRemainingFiles = true };
                    series.Volumes.Add(volName);

                    if (Settings.MiscSettings.DownloadBooks)
                    {
                        using (var client = new HttpClient())
                        {
                            if (login != null)
                            {
                                await Downloader.DoDownloads(client, login.AccessToken, Settings.MiscSettings.InputFolder, new List<Name> { new Name { ApiSlug = volName.ApiSlug, FileName = volName.FileName, } });
                            }
                        }
                    }
                }
            }

            using (var writer = new StreamWriter("JSON\\Series.json"))
            {
                var serializer = new DataContractJsonSerializer(typeof(Series[]));

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await JsonSerializer.SerializeAsync(writer.BaseStream, seriesList.OrderBy(x => x.Name).ToArray(), options);
            }
        }
    }
}
