// See https://aka.ms/new-console-template for more information

using Core;
using OBB;

Settings.LoadChapterSettings();
Settings.LoadImageSettings();
Settings.LoadMiscSettings();

while (true)
{
    Console.Clear();
    Console.WriteLine("1 - Create Omnibus");
    Console.WriteLine("2 - Update Chapter Settings");
    Console.WriteLine("3 - Update Image Settings");
    Console.WriteLine("4 - Update Miscellaneous Settings");
    Console.WriteLine("5 - Update JNC Login Details");
    Console.WriteLine("6 - Create .json from .epub");
#if DEBUG
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
#if DEBUG
        case 10:
            await JSONBuilder.GenerateTable();
            break;
#endif
        default:
            break;
    }
}


