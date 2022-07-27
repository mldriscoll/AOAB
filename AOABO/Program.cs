using AOABO;
using AOABO.Downloads;
using AOABO.Omnibus;

var executing = true;
HttpClient client = new HttpClient();

var login = await Login.FromFile(client);

while (executing)
{
    Console.Clear();

    Console.WriteLine("1 - Create an Ascendance of a Bookworm Omnibus");
    Console.WriteLine("2 - Set Login Details");

    if (login != null)
    {
        Console.WriteLine("3 - Download Updates");
    }

    var key = Console.ReadKey();

    switch (key.KeyChar, login != null)
    {
        case ('1', true):
        case ('1', false):
            OmnibusBuilder.BuildOmnibus();
            break;
        case ('2', true):
        case ('2', false):
            login = await Login.FromConsole(client);
            break;
        case ('3', true):
            await Downloader.DoDownloads(client, login.AccessToken);
            break;
        default:
            executing = false;
            break;
    };
}