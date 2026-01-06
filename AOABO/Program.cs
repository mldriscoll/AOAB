using AOABO.Config;
using AOABO.OCR;
using AOABO.Omnibus;
using Core;
using Core.Downloads;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

var executing = true;
HttpClient client = new HttpClient();

var login = await Login.FromFile(client);

while (executing)
{
    Console.Clear();

    Console.WriteLine("1 - Create an Ascendance of a Bookworm Omnibus");
    Console.WriteLine("2 - Update Omnibus Creation Settings");
    Console.WriteLine("3 - Set Login Details");

    if (login != null)
    {
        Console.WriteLine("4 - Download Updates");
        Console.WriteLine("5 - OCR Manga Bonus Written Chapters");
    }

#if DEBUG
    Console.WriteLine("6 - Import JSON");
    Console.WriteLine("7 - Create Tables");
#endif

    var key = Console.ReadKey();

    switch (key.KeyChar, login != null)
    {
        case ('1', true):
        case ('1', false):
            await OmnibusBuilder.BuildOmnibus();
            break;
        case ('2', true):
        case ('2', false):
            Configuration.UpdateOptions();
            break;
        case ('3', true):
        case ('3', false):
            login = await Login.FromConsole(client);
            break;
        case ('4', true):
            var inputFolder = string.IsNullOrWhiteSpace(Configuration.Options.Folder.InputFolder) ? Directory.GetCurrentDirectory() :
                Configuration.Options.Folder.InputFolder.Length > 1 && Configuration.Options.Folder.InputFolder[1].Equals(':') ? Configuration.Options.Folder.InputFolder : Directory.GetCurrentDirectory() + "\\" + Configuration.Options.Folder.InputFolder;
            await Downloader.DoDownloads(client, login!.AccessToken, inputFolder, Configuration.VolumeNames.Select(x => new Name { ApiSlug = x.ApiSlug, FileName = x.FileName, Quality = x.Quality! }), Configuration.Options.Image.MangaQuality);
            break;
        case ('5', true):
            await OCR.BuildOCROverrides(login!);
            break;
#if DEBUG
        case ('6', true):
        case ('6', false):
            await ImportJSON();
            break;
        case ('7', true):
        case ('7', false):
            await CreateTables();
            break;
#endif
        default:
            executing = false;
            break;
    };
}

#if DEBUG

async Task ImportJSON()
{
    var sourceFile = "..\\..\\..\\..\\OBB-WPF\\JSON\\ascendance-of-a-bookworm.json";
    var targetFile = "..\\..\\..\\JSON\\ascendance-of-a-bookworm.json";
    var secondTargetFile = "JSON\\ascendance-of-a-bookworm.json";

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Omnibus omnibus;
    using (var obStream = File.OpenRead(sourceFile))
    {
        omnibus = await JsonSerializer.DeserializeAsync<Omnibus>(obStream, options) ?? throw new Exception("Failed to load Omnibus configuration");
    }

    foreach (var chap in omnibus.Chapters)
    {
        RedoNumbering(chap);
        RedoSource(chap);
        ReadTags(chap);
    }

    if (File.Exists(targetFile))
        File.Delete(targetFile);

    using (var obStream = File.OpenWrite(targetFile))
    {
        await JsonSerializer.SerializeAsync(obStream, omnibus, options);
    }

    if (File.Exists(secondTargetFile))
        File.Delete(secondTargetFile);

    using (var obStream = File.OpenWrite(secondTargetFile))
    {
        await JsonSerializer.SerializeAsync(obStream, omnibus, options);
    }
}

void RedoNumbering(AOABO.Omnibus.Chapter ch)
{
    ch.SortOrder = string.Concat("M", ch.SortOrder);
    foreach (var chap in ch.Chapters) RedoNumbering(chap);
}

void RedoSource(AOABO.Omnibus.Chapter ch)
{
    foreach(var source in ch.Sources)
    {
        if ((source.OtherSide != null) && (!string.IsNullOrWhiteSpace(source.OtherSide.File)))
            AdjustSourceString(source.OtherSide);

        if (!string.IsNullOrWhiteSpace(source.File))
            AdjustSourceString(source);
    }

    foreach (var chap in ch.Chapters) RedoSource(chap);
}

void ReadTags(AOABO.Omnibus.Chapter ch)
{
    var year = ch.Tags.FirstOrDefault(x => x.Name.Equals("Year", StringComparison.InvariantCultureIgnoreCase));
    var season = ch.Tags.FirstOrDefault(x => x.Name.Equals("Season", StringComparison.InvariantCultureIgnoreCase));
    var source = ch.Tags.FirstOrDefault(x => x.Name.Equals("Source", StringComparison.InvariantCultureIgnoreCase));
    var set = ch.Tags.FirstOrDefault(x => x.Name.Equals("Set", StringComparison.InvariantCultureIgnoreCase));


    if (year != null) ch.Year = int.Parse(year.Value);
    if (season != null) ch.Season = season.Value;
    if (source != null) ch.OriginalSource = source.Value;
    if (set != null) ch.Set = set.Value;

    if (ch.CType == AOABO.Omnibus.Chapter.ChapterType.Map) ch.Set = ch.Name;

    foreach (var chap in ch.Chapters) ReadTags(chap);
}

void AdjustSourceString(Source source)
{
    foreach (var name in Configuration.VolumeNames)
    {
        if (source.File.Contains($"\\{name.ApiSlug}\\"))
            source.File = source.File.Replace($"ascendance-of-a-bookworm\\{name.ApiSlug}\\", $"{name.InternalName}\\");
    }

    List<string> alternates = [];
    foreach (var alt in source.Alternates)
    {
        var match = Configuration.VolumeNames.FirstOrDefault(x => alt.Contains($"\\{x.ApiSlug}\\"));
        if (match != null)
        {
            alternates.Add(alt.Replace($"ascendance-of-a-bookworm\\{match.ApiSlug}\\", $"{match.InternalName}\\"));
        }
        else
        {
            alternates.Add(alt);
        }
    }

    source.Alternates = alternates;
}

async Task CreateTables()
{
    Omnibus omnibus;
    using (var obStream = File.OpenRead("JSON\\ascendance-of-a-bookworm.json"))
    {
        var obSerializer = new DataContractJsonSerializer(typeof(Omnibus));
        var obj = obSerializer.ReadObject(obStream) ?? throw new Exception("Failed to load Omnibus configuration");
        omnibus = (Omnibus)obj;
    }

    //POV Chart
    var sb = new StringBuilder();
    sb.AppendLine("|Character|Chapter|Name|");
    sb.Append("|-|-|-|");
    string character = "";
    foreach (var chapter in BuildChapterList(omnibus).Where(x => !string.IsNullOrWhiteSpace(x.POV)).OrderBy(x => x.POV))
    {
        if (chapter.CType == AOABO.Omnibus.Chapter.ChapterType.Story)
        {
            if (string.Equals(character, chapter.POV))
            {
                sb.AppendLine($"| |{chapter.OriginalSource}|*{chapter.Name}*");
            }
            else
            {
                character = chapter.POV;
                sb.AppendLine("");
                sb.AppendLine($"|{chapter.POV}|{chapter.OriginalSource}|*{chapter.Name}*");
            }
        }
        else
        {
            if (string.Equals(character, chapter.POV))
            {
                sb.AppendLine($"| |{chapter.OriginalSource}|**{chapter.Name}**");
            }
            else
            {
                character = chapter.POV;
                sb.AppendLine("");
                sb.AppendLine($"|{chapter.POV}|{chapter.OriginalSource}|**{chapter.Name}**");
            }
        }
    }

    await Task.WhenAll(
        File.WriteAllTextAsync("POVs.txt", sb.ToString()),

        //Chronological Chart P1
        PartChart(omnibus, "PartOne.txt", partOne: true),
        //Chronological Chart P2
        PartChart(omnibus, "PartTwo.txt", partTwo: true),
        //Chronological Chart P3
        PartChart(omnibus, "PartThree.txt", partThree: true),
        //Chronological Chart P4
        PartChart(omnibus, "PartFour.txt", partFour: true),
        //Chronological Chart P5
        PartChart(omnibus, "PartFive.txt", partFive: true),
        //Chronological Chart Hannelore Y5
        PartChart(omnibus, "Hannelore.txt", hannelore: true));
}

static List<AOABO.Omnibus.Chapter> BuildChapterList(ChapterHolder ch)
{
    var results = new List<AOABO.Omnibus.Chapter>();
    foreach (var chapter in ch.Chapters.OrderBy(x => x.SortOrder))
    {
        results.Add(chapter);
        results.AddRange(BuildChapterList(chapter));
    }
    return results;
}

async Task PartChart(Omnibus ob, string name, bool partOne = false, bool partTwo = false, bool partThree = false, bool partFour = false, bool partFive = false, bool hannelore = false)
{
    var sb = new StringBuilder();
    sb.AppendLine("|Chapter|Name|POV|");
    sb.Append("|:-:|-|-|");
    int c = 1;
    string? season = null;
    int year = 0;

    var parts = ob.Chapters.Where(x => x.CType == AOABO.Omnibus.Chapter.ChapterType.Part).ToArray();
    var part = 
        partOne ? parts[0]
        : partTwo ? parts[1]
        : partThree ? parts[2]
        : partFour ? parts[3]
        : partFive ? parts[4]
        : hannelore ? parts[5]
        : throw new NotImplementedException();

    foreach (var vol in part.Chapters.Where(x => x.CType == AOABO.Omnibus.Chapter.ChapterType.Volume))
    {
        sb.AppendLine();
        c = 1;
        
        foreach (var chapter in vol.Chapters)
        {
            if (chapter.CType == AOABO.Omnibus.Chapter.ChapterType.Story)
            {
                if (chapter.Season != null)
                {
                    if (chapter.Year.HasValue)
                        year = chapter.Year.Value;
                    if (!string.Equals(season, chapter.Season))
                    {
                        season = chapter.Season;
                        sb.AppendLine($"|**Year {year} {season}**|||");
                    }
                }

                if (string.IsNullOrWhiteSpace(chapter.POV))
                {
                    sb.AppendLine($"|{vol.Name} Chapter {c}|{chapter.Name}");
                    c++;
                }
                else
                {
                    sb.AppendLine($"|**{vol.Name}**|**{chapter.Name}**|{chapter.POV}");
                }

                foreach(var bonusChapter in chapter.Chapters.Where(x => x.CType == AOABO.Omnibus.Chapter.ChapterType.Bonus))
                {
                    sb.AppendLine($"|{bonusChapter.OriginalSource}|*{bonusChapter.Name}*|{bonusChapter.POV}");
                }
            }
        }
    }

    await File.WriteAllTextAsync(name, sb.ToString());
}

#endif