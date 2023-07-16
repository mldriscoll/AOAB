// See https://aka.ms/new-console-template for more information

using Core;
using OBB;

Settings.LoadChapterSettings();
Settings.LoadImageSettings();
Settings.LoadMiscSettings();

#if DEBUG
// Copy JSON from the source folder
Directory.Delete("JSON", true);
Directory.CreateDirectory("JSON");
var json = Directory.GetFiles("../../../JSON", "*.json");
foreach(var file in json)
{
    var info = new FileInfo(file);
    File.Copy(file, $"JSON/{info.Name}");
}
#endif

while (true)
{
    Console.Clear();
    Console.WriteLine("1 - Create Omnibus");
    Console.WriteLine("2 - Update Chapter Settings");
    Console.WriteLine("3 - Update Image Settings");
    Console.WriteLine("4 - Update Miscellaneous Settings");
    Console.WriteLine("5 - Update JNC Login Details");
    Console.WriteLine("6 - Create .json from .epub");
    Console.WriteLine("7 - Hide Remaining Files on your edited volumes");
    Console.WriteLine("8 - Update Series List");
#if DEBUG
    Console.WriteLine("9 - Verify All JSON");
    Console.WriteLine("10 - Create Series Summary Table");
#endif

    var line = Console.ReadLine();

    if (!int.TryParse(line, out var choice)) break;

    switch (choice)
    {
        case 1:
            await Builder.SeriesLoop();
            break;
        case 2:
            Settings.SetChapterSettings();
            break;
        case 3:
            Settings.SetImageSettings();
            break;
        case 4:
            Settings.SetMiscSettings();
            break;
        case 5:
            await Login.FromConsole(new HttpClient());
            break;
        case 6:
            await JSONBuilder.ExtractJSON();
            break;
        case 7:
            var series7 = await JSONBuilder.GetSeries();
            Console.WriteLine("Please enter the name you edit under");
            var name = Console.ReadLine();
            foreach(var serie in series7)
            {
                serie.Volumes.Where(x => string.Equals(x.EditedBy, name, StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(x => x.ShowRemainingFiles = false);
            }
            await JSONBuilder.SaveSeries(series7);
            break;
        case 8:
            await NewVolumes.List();
            break;
#if DEBUG
        case 9:
            var series9 = await JSONBuilder.GetSeries();
            foreach (var serie in series9)
            {
                try
                {
                    await JSONBuilder.GetVolumes(serie.InternalName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(serie.InternalName);
                    Console.Write(ex.Message);
                }
            }
            Console.Write("Check Complete");
            Console.ReadKey();
            break;
        case 10:
            await JSONBuilder.GenerateTable();
            break;
#endif
        default:
            break;
    }
}


#if DEBUG
// Copy JSON back to source folder
Directory.Delete("../../../JSON", true);
Directory.CreateDirectory("../../../JSON");
json = Directory.GetFiles("JSON", "*.json");
foreach (var file in json)
{
    var info = new FileInfo(file);
    File.Copy(file, $"../../../JSON/{info.Name}");
}
#endif


