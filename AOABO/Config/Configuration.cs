using AOABO.Omnibus;
using System.Runtime.Serialization.Json;

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
            using (var reader = new StreamReader("VolumeNames.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(VolumeName[]));
                VolumeNames = ((VolumeName[])deserializer.ReadObject(reader.BaseStream)).ToList();
            }

            using (var reader = new StreamReader("JSON\\folders.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(FolderName[]));
                var list = ((FolderName[])deserializer.ReadObject(reader.BaseStream)).ToList();

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
        }

        public static void ReloadVolumes()
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Volume[]));
            Volumes.Clear();
            using (var reader = new StreamReader("Volumes.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\Fanbooks.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\MangaP1.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\MangaP2.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\LNP1.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\LNP2.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\LNP3.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\LNP4.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
            using (var reader = new StreamReader("JSON\\LNP5.json"))
            {
                Volumes.AddRange(((Volume[])deserializer.ReadObject(reader.BaseStream)));
            }
        }

        public static void UpdateOptions()
        {
            bool finished = false;
            while (!finished)
            {
                Console.Clear();

                Console.WriteLine("Which setting would you like to change?");
                Console.WriteLine("0 - Omnibus Structure");
                Console.WriteLine("1 - Bonus Chapter Placement");
                Console.WriteLine("2 - Exclude Regular Chapters");
                Console.WriteLine("3 - Chapter Headers");
                Console.WriteLine("4 - Chapter Inserts");
                Console.WriteLine("5 - Galleries");
                Console.WriteLine("6 - Manga Chapters");
                Console.WriteLine("7 - Comfy Life Strips");
                Console.WriteLine("8 - Character Sheets");
                Console.WriteLine("9 - Maps");
                Console.WriteLine("a - Afterwords");
                Console.WriteLine("b - Internal Filenames");
                Console.WriteLine("c - Polls");
                Console.WriteLine("Press any other key to return to main menu");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case '0':
                        SetStructure();
                        break;
                    case '1':
                        SetBonusChapters();
                        break;
                    case '2':
                        SetRegularChapters();
                        break;
                    case '3':
                        SetChapterHeaders();
                        break;
                    case '4':
                        SetChapterInserts();
                        break;
                    case '5':
                        SetGallery();
                        break;
                    case '6':
                        SetMangaChapters();
                        break;
                    case '7':
                        SetComfyLifeChapters();
                        break;
                    case '8':
                        SetCharacterSheets();
                        break;
                    case '9':
                        SetMaps();
                        break;
                    case 'a':
                    case 'A':
                        SetAfterwords();
                        break;
                    case 'b':
                    case 'B':
                        SetFilenames();
                        break;
                    case 'c':
                    case 'C':
                        SetBool("Do you want to include the Character Polls?", x => Options.Polls = x);
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
            File.WriteAllText("options.txt", Options.ToString());
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

        private static void SetMaps()
        {
            Console.WriteLine();
            Console.WriteLine("Would you like to include maps? Y/N");
            var key = Console.ReadKey();
            Options.Maps = false;
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.Maps = true;
                    break;
            }
        }

        private static void SetCharacterSheets()
        {
            Console.WriteLine();
            Console.WriteLine("How many Character Sheets do you want included?");
            Console.WriteLine("0 - All of them.");
            Console.WriteLine("1 - Last one in each part.");
            Console.WriteLine("2 - None");
            Options.CharacterSheets = CharacterSheets.PerPart;
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '0':
                    Options.CharacterSheets = CharacterSheets.All;
                    break;
                case '2':
                    Options.CharacterSheets = CharacterSheets.None;
                    break;
            }
        }
        private static void SetGallery()
        {
            Console.WriteLine();
            Console.WriteLine("Which gallery do you want Bonus Images to be included in?");
            Console.WriteLine("0 - The Start of each Volume.");
            Console.WriteLine("1 - The End of each Volume.");
            Console.WriteLine("2 - None");
            Options.SplashImages = GallerySetting.Start;
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.SplashImages = GallerySetting.End;
                    break;
                case '2':
                    Options.SplashImages = GallerySetting.None;
                    break;
            }
            Console.WriteLine();
            Console.WriteLine("Which gallery do you want Chapter Images to be included in?");
            Console.WriteLine("0 - The Start of each Volume.");
            Console.WriteLine("1 - The End of each Volume.");
            Console.WriteLine("2 - None");
            Options.ChapterImages = GallerySetting.Start;
            key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.ChapterImages = GallerySetting.End;
                    break;
                case '2':
                    Options.ChapterImages = GallerySetting.None;
                    break;
            }
        }

        private static void SetChapterHeaders()
        {
            Console.WriteLine();
            Console.WriteLine("Would you like the chapter headers updated to match their titles in the index? Y/N");
            var key = Console.ReadKey();
            Options.UpdateChapterNames = false;
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.UpdateChapterNames = true;
                    break;
            }
        }

        private static void SetAfterwords()
        {
            Console.WriteLine();
            Options.AfterwordSetting = AfterwordSetting.None;
            Console.WriteLine("0 - Exclude Afterwords");
            Console.WriteLine("1 - Include Afterwords at the end of each volume");
            Console.WriteLine("2 - Include Afterwords at the end of the Omnibus");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.AfterwordSetting = AfterwordSetting.VolumeEnd;
                    break;
                case '2':
                    Options.AfterwordSetting = AfterwordSetting.OmnibusEnd;
                    break;
            }
        }

        private static void SetRegularChapters()
        {
            Console.WriteLine();
            Options.IncludeRegularChapters = true;
            Console.WriteLine("Exclude regular chapters Y/N?");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.IncludeRegularChapters = false;
                    break;
            }
        }

        private static void SetChapterInserts()
        {
            Console.WriteLine();
            Options.IncludeImagesInChapters = true;
            Console.WriteLine("Exclude Chapter Insert Images Y/N?");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.IncludeImagesInChapters = false;
                    break;
            }
        }

        private static void SetFilenames()
        {
            Console.WriteLine();
            Options.UseHumanReadableFileStructure = false;
            Console.WriteLine("Use human-readable file names inside the .epub? (May cause issues with iBooks.) Y/N?");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.UseHumanReadableFileStructure = true;
                    break;
            }
        }

        private static void SetBonusChapters()
        {
            Options.BonusChapterSetting = BonusChapterSetting.Chronological;
            Console.WriteLine();
            Console.WriteLine("0 - Place Bonus Chapters after the last chapter they overlap with.");
            Console.WriteLine("1 - Place Bonus Chapters at the end of the Volume");
            Console.WriteLine("2 - Leave out Bonus Chapters");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.BonusChapterSetting = BonusChapterSetting.EndOfBook;
                    break;
                case '2':
                    Options.BonusChapterSetting = BonusChapterSetting.LeaveOut;
                    break;
            }
        }

        private static void SetMangaChapters()
        {
            Options.MangaChapters = BonusChapterSetting.Chronological;
            Console.WriteLine();
            Console.WriteLine("0 - Place Manga Chapters after the last chapter they overlap with.");
            Console.WriteLine("1 - Place Manga Chapters at the end of the Volume");
            Console.WriteLine("2 - Leave out Manga Chapters");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.MangaChapters = BonusChapterSetting.EndOfBook;
                    break;
                case '2':
                    Options.MangaChapters = BonusChapterSetting.LeaveOut;
                    break;
            }
        }

        private static void SetComfyLifeChapters()
        {
            Options.ComfyLifeChapters = ComfyLifeSetting.VolumeEnd;
            Console.WriteLine();
            Console.WriteLine("0 - Place Comfy Life Chapters after the volume they were published with.");
            Console.WriteLine("1 - Place Comfy Life Chapters at the end of the omnibus.");
            Console.WriteLine("2 - Leave out Comfy Life Chapters");
            var key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case '1':
                    Options.ComfyLifeChapters = ComfyLifeSetting.OmnibusEnd;
                    break;
                case '2':
                    Options.ComfyLifeChapters = ComfyLifeSetting.None;
                    break;
            }
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