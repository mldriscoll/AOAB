using AOABO.Omnibus;
using Core.Downloads;
using System.Runtime.Serialization.Json;
using static Core.Downloads.LibraryResponse;

namespace AOABO.Config
{
    public static class Configuration
    {
        public static readonly List<Volume> Volumes;
        public static readonly List<VolumeName> VolumeNames;
        public static readonly Dictionary<string, string> FolderNames;
        public static VolumeOptions Options { get; set; }

        static Configuration()
        {
            Volumes = new List<Volume>();
            ReloadVolumes();
            using (var reader = new StreamReader("JSON\\VolumeNames.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(VolumeName[]));
                VolumeNames = (deserializer.ReadObject(reader.BaseStream) as VolumeName[])!.ToList();
            }

            using (var reader = new StreamReader("JSON\\folders.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(FolderName[]));
                var list = (deserializer.ReadObject(reader.BaseStream) as FolderName[])!.ToList();

                FolderNames = list.ToDictionary(x => x.Name, x => x.Folder);
            }

            if (File.Exists("options.txt"))
            {
                Options = new VolumeOptions(File.ReadAllText("options.txt"));
            }
            else
            {
                Options = new VolumeOptions();
            }

            if (File.Exists("options.json"))
            {
                using (var reader = new StreamReader("options.json")) {
                    var deserializer = new DataContractJsonSerializer(typeof(VolumeOptions));
                    Options = (deserializer.ReadObject(reader.BaseStream) as VolumeOptions)!;
                    Options.Upgrade();
                }
            }
        }

        public static void ReloadVolumes()
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Volume[]));
            Volumes.Clear();
            using (var reader = new StreamReader("JSON\\SideStories.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\Fanbooks.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\MangaP1.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\MangaP2.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\MangaP3.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\MangaP4.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\LNP1.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\LNP2.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\LNP3.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\LNP4.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\LNP5.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
            using (var reader = new StreamReader("JSON\\Hannelore.json"))
            {
                Volumes.AddRange((deserializer.ReadObject(reader.BaseStream) as Volume[])!);
            }
        }

        public static void UpdateOptions()
        {
            bool finished = false;
            while (!finished)
            {
                Console.Clear();

                Console.WriteLine("Which setting would you like to change?");
                Console.WriteLine($"0 - Omnibus Structure ({Options.OutputStructureSetting})");
                Console.WriteLine("1 - Chapter Settings");
                Console.WriteLine("2 - Image Settings");
                Console.WriteLine("3 - Extra Content Settings");
                Console.WriteLine($"4 - Human-Readable Internal Filenames ({Options.UseHumanReadableFileStructure})");
                Console.WriteLine("5 - Folder Settings");
                Console.WriteLine("Press any other key to return to main menu");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        SetStructure();
                        break;
                    case '1':
                        SetChapterSettings();
                        break;
                    case '2':
                        SetImageSettings();
                        break;
                    case '3':
                        SetExtraContentSettings();
                        break;
                    case '4':
                        SetBool("Use human-readable file names inside the .epub? (May cause issues with iBooks.)", x => Options.UseHumanReadableFileStructure = x);
                        break;
                    case '5':
                        SetFolderSettings();
                        break;
                    default:
                        finished = true;
                        break;
                }
            }
            
            if (File.Exists("options.txt"))
            {
                File.Delete("options.txt");
            }
            if (File.Exists("options.json"))
            {
                File.Delete("options.json");
            }
            var serializer = new DataContractJsonSerializer(typeof(VolumeOptions));
            using (var stream = File.Open("options.json", FileMode.Create))
            {
                serializer.WriteObject(stream, Options);
            }
        }

        private static void SetBool(string question, Action<bool> set)
        {
            Console.WriteLine();
            Console.WriteLine($"{question} Y/N");
            var key = Console.ReadKey();
            set(false);
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    set(true);
                    break;
            }
        }

        private static void SetNullableInt(string question, string intQuestion, Action<int?> set, int? min, int? max)
        {
            Console.WriteLine();
            Console.WriteLine($"{question} Y/N");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    SetInt(intQuestion, x => set(x), min, max);
                    break;
                default:
                    set(null);
                    break;
            }
        }

        private static void SetInt(string question, Action<int> set, int? min, int? max)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"{question}");
                var str = Console.ReadLine();
                if (int.TryParse(str, out var value))
                {
                    if ((!min.HasValue || min <= value) && (!max.HasValue || max >= value))
                    {
                        set(value);
                        break;
                    }
                }
            }
        }

        private static void SetChapterSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"0 - Include Regular Chapters (Currently {Options.Chapter.IncludeRegularChapters})");
                Console.WriteLine($"1 - Include Bonus Chapters (Currently {Options.Chapter.BonusChapterSetting})");
                Console.WriteLine($"2 - Include Manga Chapters (Currently {Options.Chapter.MangaChapterSetting})");
                Console.WriteLine($"3 - Update Chapter Headers to match the names in the index (Currently {Options.Chapter.UpdateChapterNames})");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        SetBool("Include Regular Chapters (Myne POV + Prologues and Epilogues)", x => Options.Chapter.IncludeRegularChapters = x);
                        break;
                    case '1':
                        Console.WriteLine();
                        Console.WriteLine("0 - Place Bonus Chapters as subchapters of the last chapter they overlap with.");
                        Console.WriteLine("1 - Place Bonus Chapters after the last chapter they overlap with.");
                        Console.WriteLine("2 - Place Bonus Chapters at the end of the Volume");
                        Console.WriteLine("3 - Leave out Bonus Chapters");
                        key = Console.ReadKey();
                        switch (key.KeyChar)
                        {
                            case '0':
                                Options.Chapter.BonusChapter = BonusChapterSetting.SubChapter;
                                break;
                            case '1':
                                Options.Chapter.BonusChapter = BonusChapterSetting.Chronological;
                                break;
                            case '2':
                                Options.Chapter.BonusChapter = BonusChapterSetting.EndOfBook;
                                break;
                            case '3':
                                Options.Chapter.BonusChapter = BonusChapterSetting.LeaveOut;
                                break;
                        }
                        break;
                    case '2':
                        Options.Chapter.MangaChapters = BonusChapterSetting.Chronological;
                        Console.WriteLine();
                        Console.WriteLine("0 - Place Manga Chapters after the last chapter they overlap with.");
                        Console.WriteLine("1 - Place Manga Chapters at the end of the Volume");
                        Console.WriteLine("2 - Leave out Manga Chapters");
                        key = Console.ReadKey();
                        switch (key.KeyChar)
                        {
                            case '1':
                                Options.Chapter.MangaChapters = BonusChapterSetting.EndOfBook;
                                break;
                            case '2':
                                Options.Chapter.MangaChapters = BonusChapterSetting.LeaveOut;
                                break;
                        }
                        break;
                    case '3':
                        SetBool("Would you like the chapter headers updated to match their titles in the index?", x => Options.Chapter.UpdateChapterNames = x);
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SetImageSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"0 - Include/Exclude Chapter Inserts (Currently {Options.Image.IncludeImagesInChaptersSetting})");
                Console.WriteLine($"1 - Bonus Image Gallery (Currently {Options.Image.SplashImagesSetting})");
                Console.WriteLine($"2 - Chapter Insert Gallery (Currently {Options.Image.ChapterImagesSetting})");
                Console.WriteLine($"3 - Set Maximum Image Width (Currently {Options.Image.MaxWidthSetting})");
                Console.WriteLine($"4 - Set Maximum Image Height (Currently {Options.Image.MaxHeightSetting})");
                Console.WriteLine($"5 - Set Resized Image Quality (Currently {Options.Image.Quality})");
                Console.WriteLine($"6 - Set Manga Quality (Currently {Options.Image.MangaQualitySetting})");
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        SetBool("Include Chapter Insert Images", x => Options.Image.IncludeImagesInChapters = x);
                        break;
                    case '1':
                        Console.WriteLine();
                        Console.WriteLine("Which gallery do you want Bonus Images to be included in?");
                        Console.WriteLine("0 - The Start of each Volume.");
                        Console.WriteLine("1 - The End of each Volume.");
                        Console.WriteLine("2 - None");
                        Options.Image.SplashImages = GallerySetting.Start;
                        key = Console.ReadKey();
                        switch (key.KeyChar)
                        {
                            case '1':
                                Options.Image.SplashImages = GallerySetting.End;
                                break;
                            case '2':
                                Options.Image.SplashImages = GallerySetting.None;
                                break;
                        }
                        break;
                    case '2':
                        Console.WriteLine();
                        Console.WriteLine("Which gallery do you want Chapter Inserts to be included in?");
                        Console.WriteLine("0 - The Start of each Volume.");
                        Console.WriteLine("1 - The End of each Volume.");
                        Console.WriteLine("2 - None");
                        Options.Image.ChapterImages = GallerySetting.Start;
                        key = Console.ReadKey();
                        switch (key.KeyChar)
                        {
                            case '1':
                                Options.Image.ChapterImages = GallerySetting.End;
                                break;
                            case '2':
                                Options.Image.ChapterImages = GallerySetting.None;
                                break;
                        }
                        break;
                    case '3':
                        SetNullableInt("Do you want to enforce a maximum image width?", "How many pixels wide should the limit be?", x => Options.Image.MaxWidth = x, 1, null);
                        break;
                    case '4':
                        SetNullableInt("Do you want to enforce a maximum image height?", "How many pixels tall should the limit be?", x => Options.Image.MaxHeight = x, 1, null);
                        break;
                    case '5':
                        SetInt("Pick a new Image Quality (1-100, higher numbers produce better images and larger file sizes)", x => Options.Image.Quality = x, 1, 100);
                        break;
                    case '6':
                        Console.WriteLine("Which manga version do you want to download?");
                        Console.WriteLine("1 - Mobile");
                        Console.WriteLine("2 - Desktop");
                        Console.WriteLine("3 - 4k");
                        key = Console.ReadKey();

                        switch (key.KeyChar)
                        {
                            case '1':
                                Options.Image.MangaQuality = MangaQuality.Mobile;
                                break;
                            case '2':
                                Options.Image.MangaQuality = MangaQuality.Desktop;
                                break;
                            case '3':
                                Options.Image.MangaQuality = MangaQuality.FourK;
                                break;
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SetFolderSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"0 - Input Folder (Currently {Options.Folder.InputFolderSetting})");
                Console.WriteLine($"1 - Output Folder (Currently {Options.Folder.OutputFolderSetting})");
                Console.WriteLine($"2 - Delete the Temporary Folder once the omnibus is built? ({Options.Folder.DeleteTempFolder})");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        Console.WriteLine();
                        Console.WriteLine("Enter the new input folder.");
                        Console.WriteLine("This can be absolute or relative, leave blank for 'the folder this program is in'.");
                        Options.Folder.InputFolder = Console.ReadLine() ?? string.Empty;
                        break;
                    case '1':
                        Console.WriteLine();
                        Console.WriteLine("Enter the new output folder.");
                        Console.WriteLine("This can be absolute or relative, leave blank for 'the folder this program is in'.");
                        Options.Folder.OutputFolder = Console.ReadLine() ?? string.Empty;
                        break;
                    case '2':
                        SetBool("Delete the Temporary Folder once the omnibus is built?", x => Options.Folder.DeleteTempFolder = x);
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SetExtraContentSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"0 - Include Comfy Life Chapters ({Options.Extras.ComfyLife.Summary})");
                Console.WriteLine($"1 - Include Character Sheets ({Options.Extras.CharacterSheetSetting.Summary})");
                Console.WriteLine($"2 - Include Maps ({Options.Extras.MapSetting.Summary})");
                Console.WriteLine($"3 - Include Afterwords ({Options.Extras.AfterwordSetting.Summary})");
                Console.WriteLine($"4 - Include Polls ({Options.Extras.PollSetting.Summary})");
                Console.WriteLine($"5 - Include POV Chapter collection ({Options.Collection.POVChapterOrderingSetting})");
                Console.WriteLine($"6 - Include Q&As ({Options.Extras.QNASetting.Summary})");
                Console.WriteLine($"7 - Include Drama CDs ({Options.Extras.DramaCDSetting.Summary}");
                Console.WriteLine($"8 - Include Misc Fanbook Content ({Options.Extras.FanbookMiscSetting.Summary})");
                Console.WriteLine($"X - Set Chapter Type Order");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        SetExtraChapterSetting(Options.Extras.ComfyLife, "Comfy Life Chapters");
                        break;
                    case '1':
                        SetExtraChapterSetting(Options.Extras.CharacterSheetSetting, "Character Sheets");
                        break;
                    case '2':
                        SetExtraChapterSetting(Options.Extras.MapSetting, "Maps");
                        break;
                    case '3':
                        SetExtraChapterSetting(Options.Extras.AfterwordSetting, "Afterwords");
                        break;
                    case '4':
                        SetExtraChapterSetting(Options.Extras.PollSetting, "Character Polls");
                        break;
                    case '5':
                        SetBool("Do you want to include a collection of the POV chapters?", x => Options.Collection.POVChapterCollection = x);
                        if (Options.Collection.POVChapterCollection)
                        {
                            SetBool("Do you want the POV chapters ordered by POV character?", x => Options.Collection.POVChapterOrdering = x);
                        }
                        break;
                    case '6':
                        SetExtraChapterSetting(Options.Extras.QNASetting, "Q&As");
                        break;
                    case '7':
                        SetExtraChapterSetting(Options.Extras.DramaCDSetting, "Drama CD Writeups");
                        break;
                    case '8':
                        SetExtraChapterSetting(Options.Extras.FanbookMiscSetting, "Remaining Fanbook Content");
                        break;
                    case 'X':
                        char currentPrefix = 'a';
                        List<VolumeOptions.ChapterSetting> list = [ 
                            new VolumeOptions.ChapterSetting{ Name = "Main Story" }
                        ];
                        if (Options.Collection.POVChapterCollection) list.Add(new VolumeOptions.ChapterSetting { Name = "POV Collection" });
                        if (Options.Extras.ComfyLife.Included) list.Add(Options.Extras.ComfyLife);
                        if (Options.Extras.MapSetting.Included) list.Add(Options.Extras.MapSetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.CharacterSheetSetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.AfterwordSetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.PollSetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.QNASetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.DramaCDSetting);
                        if (Options.Extras.CharacterSheetSetting.Included) list.Add(Options.Extras.FanbookMiscSetting);
                        list = [.. list.OrderBy(x => x.Name)];
                        while (list.Count > 1) currentPrefix = SetOrder(currentPrefix, list);

                        var last = list.First();
                        switch (last.Name)
                        {
                            case "Main Story":
                                break;
                            case "POV Collection":
                                Options.Collection.Prefix = $"{currentPrefix}";
                                break;
                            default:
                                last.PositionPrefix = $"{currentPrefix}";
                                break;
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SetExtraChapterSetting(VolumeOptions.ChapterSetting setting, string label)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Include {label}:");
                Console.WriteLine($"1 - In their original position");
                Console.WriteLine($"2 - In their original volume");
                Console.WriteLine($"3 - In their original part");
                Console.WriteLine($"4 - At the Omnibus level");
                Console.WriteLine($"5 - Leave Out");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        setting.Position = VolumeOptions.ChapterSetting.PositionEnum.Original;
                        setting.Included = true;
                        return;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        setting.Position = VolumeOptions.ChapterSetting.PositionEnum.Volume;
                        setting.Included = true;
                        return;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        setting.Position = VolumeOptions.ChapterSetting.PositionEnum.Part;
                        setting.Included = true;
                        return;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        setting.Position = VolumeOptions.ChapterSetting.PositionEnum.Omnibus;
                        setting.Included = true;
                        return;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        setting.Included = false;
                        return;
                }
            }
        }

        private static char SetOrder(char prefix, List<VolumeOptions.ChapterSetting> options)
        {
            Console.Clear();
            Console.WriteLine("Select sections in the order they should appear");
            for (int i = 1; i <= options.Count; i++)
            {
                Console.WriteLine($"{i} - {options[i - 1].Name}");
            }

            if (int.TryParse(Console.ReadLine(), out var num))
            {
                if ((num < 1) || (num > options.Count)) return prefix;
                var selection = options[num - 1];
                switch (selection.Name)
                {
                    case "Main Story":
                        options.Remove(selection);
                        return 'n';
                    case "POV Collection":
                        Options.Collection.Prefix = $"{prefix}";
                        options.Remove(selection);
                        return (char)(prefix + 1);
                    default:
                        selection.PositionPrefix = $"{prefix}";
                        options.Remove(selection);
                        return (char)(prefix + 1);
                }
            }

            return prefix;
        }

        private static void SetStructure()
        {
            Options.OutputStructure = OutputStructure.Flat;
            Console.WriteLine();
            Console.WriteLine("How should the output file be structured?");
            Console.WriteLine("0: Flat structure");
            Console.WriteLine("1: By Part");
            Console.WriteLine("2: By Part and Volume");
            Console.WriteLine("3: By Season");
            var key = Console.ReadKey();
            Console.WriteLine();
            IFolder folder = new BasicFolder();
            switch (key.KeyChar)
            {
                case '1':
                    Options.OutputStructure = OutputStructure.Parts;
                    break;
                case '2':
                    Options.OutputStructure = OutputStructure.Volumes;
                    break;
                case '3':
                    Options.OutputStructure = OutputStructure.Seasons;
                    Console.WriteLine();
                    Options.StartYear = -100;
                    while (Options.StartYear == -100)
                    {
                        Console.WriteLine("Which year should be used for the story beginning (Myne is 5 at the start of P1V1)?");
                        var yearinput = Console.ReadLine();
                        if (int.TryParse(yearinput, out var y))
                            Options.StartYear = y;
                    }

                    Console.WriteLine("How would you like the years formatted? (Entering nothing will give you plain numbers. To give actual labels, enter any text with a 0 where you want the year to be.)");
                    Console.WriteLine("0 - XX");
                    Console.WriteLine("1 - Year XX");

                    var subKey = Console.ReadKey();
                    switch (subKey.KeyChar)
                    {
                        case '1':
                            Options.OutputYearFormat = 1;
                            break;
                        default:
                            Options.OutputYearFormat = 0;
                            break;
                    }
                    break;
            }
        }
    }
}