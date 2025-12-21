using AOABO.Chapters;
using AOABO.Config;
using AOABO.OCR;
using AOABO.Omnibus;
using Core;
using Core.Downloads;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

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

    if (year != null) ch.Year = int.Parse(year.Value);
    if (season != null) ch.Season = season.Value;
    if (source != null) ch.OriginalSource = source.Value;

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
    var chapters = Configuration.Volumes.SelectMany(x =>
    {
        var c = new List<AOABO.Chapters.Chapter>();
        c.AddRange(x.POVChapters);
        c.AddRange(x.MangaChapters);
        c.AddRange(x.BonusChapters);
        c.AddRange(x.Chapters);
        return c;
    }).OrderBy(x => (x is MoveableChapter xx) ? xx.EarlySortOrder : x.SortOrder).ToArray();

    //POV Chart
    var sb = new StringBuilder();
    sb.AppendLine("|Character|Chapter|Name|");
    sb.Append("|-|-|-|");
    string character = "";
    foreach(var chapter in chapters.Where(x => x is BonusChapter || x is POVChapter).OrderBy(x => x is BonusChapter c ? c.POV : ((POVChapter)x).POV))
    {
        if (chapter is BonusChapter b)
        {
            if (!string.IsNullOrWhiteSpace(b.POV))
            {
                if (string.Equals(character, b.POV))
                {
                    sb.AppendLine($"| |{b.Source}|**{b.ChapterName}**");
                }
                else
                {
                    character = b.POV;
                    sb.AppendLine("");
                    sb.AppendLine($"|{b.POV}|{b.Source}|**{b.ChapterName}**");
                }
            }
        }
        if (chapter is POVChapter p)
        {
            if (!string.IsNullOrWhiteSpace(p.POV))
            {
                if (string.Equals(character, p.POV))
                {
                    sb.AppendLine($"| |{p.GetVolumeName()}|*{p.ChapterName}*");
                }
                else
                {
                    character = p.POV;
                    sb.AppendLine("");
                    sb.AppendLine($"|{p.POV}|{p.GetVolumeName()}|*{p.ChapterName}*");
                }
            }
        }
    }

    await Task.WhenAll(
        File.WriteAllTextAsync("POVs.txt", sb.ToString()),

        //Chronological Chart P1
        PartChart(chapters, "PartOne.txt", partOne: true),
        //Chronological Chart P2
        PartChart(chapters, "PartTwo.txt", partTwo: true),
        //Chronological Chart P3
        PartChart(chapters, "PartThree.txt", partThree: true),
        //Chronological Chart P4
        PartChart(chapters, "PartFour.txt", partFour: true),
        //Chronological Chart P5
        PartChart(chapters, "PartFive.txt", partFive: true),
        //Chronological Chart Hannelore Y5
        PartChart(chapters, "Hannelore.txt", hannelore: true));
}

async Task PartChart(AOABO.Chapters.Chapter[] chapters, string name, bool partOne = false, bool partTwo = false, bool partThree = false, bool partFour = false, bool partFive = false, bool hannelore = false)
{
    var sb = new StringBuilder();
    sb.AppendLine("|Chapter|Name|POV|");
    sb.Append("|:-:|-|-|");
    int c = 1;
    string? volume = null;
    string? season = null;
    int year = 0;
    foreach (var chapter in chapters.Where(x => x.ProcessedInPartOne == partOne && x.ProcessedInPartTwo == partTwo && x.ProcessedInPartThree == partThree && x.ProcessedInPartFour == partFour && x.ProcessedInPartFive == partFive && x.ProcessedInHannelore == hannelore))
    {
        if (!string.Equals(volume, chapter.Volume))
        {
            sb.AppendLine();
            volume = chapter.Volume;
            c = 1;
        }
        

        if (chapter is BonusChapter b)
        {
            if (!string.Equals(season, b.EarlySeason))
            {
                sb.AppendLine($"|**Year {b.EarlyYear} {b.EarlySeason}**|||");
                season = b.EarlySeason;
                year = b.EarlyYear;
            }
            sb.AppendLine($"|{b.Source}|*{b.ChapterName}*|{b.POV}");
        }
        else if (chapter is POVChapter p)
        {
            if (!string.Equals(season, chapter.Season))
            {
                sb.AppendLine($"|**Year {chapter.Year} {chapter.Season}**|||");
                season = chapter.Season;
                year = chapter.Year;
            }
            sb.AppendLine($"|**{p.GetVolumeName()}**|**{p.ChapterName}**|{p.POV}");
        }
        else
        {
            if (!string.Equals(season, chapter.Season))
            {
                sb.AppendLine($"|**Year {chapter.Year} {chapter.Season}**|||");
                season = chapter.Season;
                year = chapter.Year;
            }
            sb.AppendLine($"|{chapter.GetVolumeName()}C{c}|{chapter.ChapterName}");
            c++;
        }
    }

    await File.WriteAllTextAsync(name, sb.ToString());
}

#endif