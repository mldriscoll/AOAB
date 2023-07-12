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

            Login? login = null;
            if (Settings.MiscSettings.DownloadBooks)
            {
                Dictionary<SeriesListResponse, List<SeriesVolumeResponse>> dict = null;
                using (var client = new HttpClient())
                {
                    login = await Login.FromFile(client);
                    login = login ?? await Login.FromConsole(client);

                    if (login != null)
                    {
                        var list = await Downloader.GetSeriesList(client, login.AccessToken);

                        dict = list.series.ToDictionary(x => x, x => Downloader.GetSeries(client, x.slug).Result.volumes);
                    }
                }

                if (dict != null)
                    foreach (var serie in dict.Where(x => !x.Key.type.Equals("manga", StringComparison.InvariantCultureIgnoreCase)
                        && !x.Key.title.Contains("ascendance of a bookworm", StringComparison.InvariantCultureIgnoreCase)
                        ))
                    {
                        var series = seriesList.FirstOrDefault(x => x.ApiSlugs.Any(y => y.Slug.Equals(serie.Key.slug, StringComparison.InvariantCultureIgnoreCase)));

                        if (series == null && serie.Value.Count > 1)
                        {
                            Console.WriteLine($"Series {serie.Key.title} cannot be found. If it already exists, enter its internal name now:");
                            var line = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                series = seriesList.FirstOrDefault(x => x.InternalName.Equals(line, StringComparison.InvariantCultureIgnoreCase));

                                // Assign the API Slug so it won't ask you again
                                if (series != null)
                                {
                                    series.ApiSlugs.Add(new SeriesSlug { Order = series.ApiSlugs.Count + 1, Slug = serie.Key.slug });
                                }
                            }
                        }

                        if (series != null)
                        {
                            var order = 100 * (series.ApiSlugs.FirstOrDefault(x => x.Slug.Equals(serie.Key.slug, StringComparison.InvariantCultureIgnoreCase))?.Order ?? 1);

                            series.Volumes.AddRange(serie.Value.Where(x => !series.Volumes.Any(y => y.ApiSlug.Equals(x.slug, StringComparison.OrdinalIgnoreCase))).ToList().Select(x => new VolumeName
                            {
                                ApiSlug = x.slug,
                                EditedBy = null,
                                FileName = $"{x.slug}.epub",
                                ShowRemainingFiles = true,
                                Order = order + x.number,
                                Published = DateOnly.FromDateTime(DateTime.Parse(x.publishing)).ToString("yyyy-MM-dd")
                            }));
                        }
                        else if (serie.Value.Count > 1)
                        {
                            series = new Series
                            {
                                ApiSlugs = new List<SeriesSlug> { new SeriesSlug { Order = 1, Slug = serie.Key.slug } },
                                Author = serie.Value.First().creators.First(x => x.role.Equals("AUTHOR")).name,
                                AuthorSort = serie.Value.First().creators.First(x => x.role.Equals("AUTHOR")).name.Split(' ').Reverse().Aggregate((str, agg) => string.Concat(str, ", ", agg)).Trim().ToUpper(),
                                InternalName = serie.Key.slug,
                                Name = serie.Key.title,
                                Volumes = serie.Value.Select(x => new VolumeName
                                {
                                    ApiSlug = x.slug ?? string.Empty,
                                    EditedBy = null,
                                    FileName = $"{x.slug}.epub",
                                    ShowRemainingFiles = true,
                                    Order = 100 + x.number,
                                    Published = DateOnly.FromDateTime(DateTime.Parse(x.publishing)).ToString("yyyy-MM-dd"),
                                    Title = x.title ?? string.Empty
                                }).ToList()
                            };

                            seriesList.Add(series);

                            File.WriteAllText($"JSON\\{series.InternalName}.json", "[]");
                        }
                    }
            }

            seriesList = seriesList.OrderBy(x => x.Name).ToList();

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
