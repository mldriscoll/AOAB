using AOABO;
using AOABO.Config;
using AOABO.OCR;
using AOABO.Omnibus;
using Core;
using Core.Downloads;
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
    Console.WriteLine("6 - Redo JSON Files");
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
            await Downloader.DoDownloads(client, login.AccessToken, inputFolder, Configuration.VolumeNames.Select(x => new Name { ApiSlug = x.ApiSlug, FileName = x.FileName, Quality = x.Quality }));
            break;
        case ('5', true):
            await OCR.BuildOCROverrides(login);
            break;
#if DEBUG
        case ('6', true):
        case ('6', false):
            await RedoJSON();
            break;
#endif
        default:
            executing = false;
            break;
    };
}

async Task RedoJSON()
{
    var bonusChapters = Configuration.Volumes.SelectMany(x => x.BonusChapters).GroupBy(x => x.Volume).ToDictionary(x => x.Key, x => x.ToArray());

    var fanbooks = new List<Volume>();
    var p1 = new List<Volume>();
    var p2 = new List<Volume>();
    var p3 = new List<Volume>();
    var p4 = new List<Volume>();
    var p5 = new List<Volume>();
    var mp1 = new List<Volume>();
    var mp2 = new List<Volume>();
    var mp3 = new List<Volume>();
    var ss = new List<Volume>();

    foreach(var vol in Configuration.Volumes)
    {
        switch (vol.InternalName)
        {
            case "FB1":
            case "FB2":
            case "FB3":
            case "FB4":
            case "FB5":
                fanbooks.Add(vol);
                break;
            case "LN0101":
            case "LN0102":
            case "LN0103":
                p1.Add(vol);
                break;
            case "LN0201":
            case "LN0202":
            case "LN0203":
            case "LN0204":
                p2.Add(vol);
                break;
            case "LN0301":
            case "LN0302":
            case "LN0303":
            case "LN0304":
            case "LN0305":
                p3.Add(vol);
                break;
            case "LN0401":
            case "LN0402":
            case "LN0403":
            case "LN0404":
            case "LN0405":
            case "LN0406":
            case "LN0407":
            case "LN0408":
            case "LN0409":
                p4.Add(vol);
                break;
            case "LN0501":
            case "LN0502":
            case "LN0503":
            case "LN0504":
            case "LN0505":
            case "LN0506":
            case "LN0507":
            case "LN0508":
            case "LN0509":
            case "LN0510":
            case "LN0511":
            case "LN0512":
                p5.Add(vol);
                break;
            case "M0101":
            case "M0102":
            case "M0103":
            case "M0104":
            case "M0105":
            case "M0106":
            case "M0107":
                mp1.Add(vol);
                break;
            case "M0201":
            case "M0202":
            case "M0203":
            case "M0204":
            case "M0205":
            case "M0206":
            case "M0207":
            case "M0208":
            case "M0209":
                mp2.Add(vol);
                break;
            case "M0301":
            case "M0302":
            case "M0303":
            case "M0304":
                mp3.Add(vol);
                break;
            case "RAS1":
            case "SS01":
            case "SS02":
                ss.Add(vol);
                break;
        }
    }

    await Save("Fanbooks", fanbooks);
    await Save("LNP1", p1);
    await Save("LNP2", p2);
    await Save("LNP3", p3);
    await Save("LNP4", p4);
    await Save("LNP5", p5);
    await Save("MangaP1", mp1);
    await Save("MangaP2", mp2);
    await Save("MangaP3", mp3);
    await Save("SideStories", ss);
}

async Task Save(string filename, List<Volume> vols)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    using (var writer = new StreamWriter($"JSON\\{filename}.json"))
    {
        await JsonSerializer.SerializeAsync(writer.BaseStream, vols, options: options);
    }
    using (var writer = new StreamWriter($"..\\..\\..\\JSON\\{filename}.json"))
    {
        await JsonSerializer.SerializeAsync(writer.BaseStream, vols, options: options);
    }
}