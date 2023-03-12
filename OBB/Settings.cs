using System.Runtime.Serialization.Json;

namespace OBB
{
    public static class Settings
    {
        public static ChapterSettings ChapterSettings { get; set; } = new ChapterSettings();
        public static ImageSettings ImageSettings { get; set; } = new ImageSettings();
        public static MiscSettings MiscSettings { get; set; } = new MiscSettings();

        public static void LoadChapterSettings()
        {
            if (File.Exists("ChapterSettings.json"))
            {
                using (var reader = new StreamReader("ChapterSettings.json"))
                {
                    var deserializer = new DataContractJsonSerializer(typeof(ChapterSettings));
                    ChapterSettings = (ChapterSettings)deserializer.ReadObject(reader.BaseStream);
                }
            }
        }
        public static void LoadImageSettings()
        {
            if (File.Exists("ImageSettings.json"))
            {
                using (var reader = new StreamReader("ImageSettings.json"))
                {
                    var deserializer = new DataContractJsonSerializer(typeof(ImageSettings));
                    ImageSettings = (ImageSettings)deserializer.ReadObject(reader.BaseStream);
                }
            }
        }
        public static void LoadMiscSettings()
        {
            if (File.Exists("MiscSettings.json"))
            {
                using (var reader = new StreamReader("MiscSettings.json"))
                {
                    var deserializer = new DataContractJsonSerializer(typeof(MiscSettings));
                    MiscSettings = (MiscSettings)deserializer.ReadObject(reader.BaseStream);
                }
            }
        }

        public static void SetChapterSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Chapter Settings");
                Console.WriteLine($"1 - Include Bonus Chapters ({ChapterSettings.IncludeBonusChapters})");
                Console.WriteLine($"2 - Include Extra Content ({ChapterSettings.IncludeExtraContent})");
                Console.WriteLine($"3 - Update Chapter Titles to match the index ({ChapterSettings.UpdateChapterTitles})");

                var line = Console.ReadLine();

                if (!int.TryParse(line, out var choice)) break;

                switch (choice)
                {
                    case 1:
                        SetBool("Do you want to include bonus chapters", x => ChapterSettings.IncludeBonusChapters = x);
                        break;
                    case 2:
                        SetBool("Do you want to include extra content", x => ChapterSettings.IncludeExtraContent = x);
                        break;
                    case 3:
                        SetBool("Do you want to update the in-text chapter titles to match the names in the index", x => ChapterSettings.UpdateChapterTitles = x);
                        break;
                    default:
                        break;
                }
            }

            using (var writer = new StreamWriter("ChapterSettings.json"))
            {
                var serializer = new DataContractJsonSerializer(typeof(ChapterSettings));
                serializer.WriteObject(writer.BaseStream, ChapterSettings);
            }
        }

        public static void SetImageSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Image Settings");
                Console.WriteLine($"1 - Include Bonus Art at the start of each volume ({ImageSettings.IncludeBonusArtAtStartOfVolume})");
                Console.WriteLine($"2 - Include Bonus Art at the end of each volume ({ImageSettings.IncludeBonusArtAtEndOfVolume})");
                Console.WriteLine($"3 - Include Chapter Inserts at the start of each volume ({ImageSettings.IncludeInsertsAtStartOfVolume})");
                Console.WriteLine($"4 - Include Chapter Inserts at the end of each volume ({ImageSettings.IncludeInsertsAtEndOfVolume})");
                Console.WriteLine($"5 - Include Inserts in Chapters ({ImageSettings.IncludeInsertsInChapters})");
                Console.WriteLine($"6 - Set a maximum height (in pixels) for images ({ImageSettings.MaxImageHeight})");
                Console.WriteLine($"7 - Set a maximum width (in pixels) for images ({ImageSettings.MaxImageWidth})");
                Console.WriteLine($"8 - Set the image quality for any resized images ({ImageSettings.ImageQuality})");
                var line = Console.ReadLine();

                if (!int.TryParse(line, out var choice)) break;

                switch (choice)
                {
                    case 1:
                        SetBool("Do you want to include cover/bonus art at the start of each volume", x => ImageSettings.IncludeBonusArtAtStartOfVolume = x);
                        break;
                    case 2:
                        SetBool("Do you want to include cover/bonus art at the end of each volume", x => ImageSettings.IncludeBonusArtAtEndOfVolume = x);
                        break;
                    case 3:
                        SetBool("Do you want to include chapter inserts at the start of each volume", x => ImageSettings.IncludeInsertsAtStartOfVolume = x);
                        break;
                    case 4:
                        SetBool("Do you want to include chapter inserts at the end of each volume", x => ImageSettings.IncludeInsertsAtEndOfVolume = x);
                        break;
                    case 5:
                        SetBool("Do you want to include chapter inserts in chapters", x => ImageSettings.IncludeInsertsInChapters = x);
                        break;
                    case 6:
                        SetNullableInt("Do you want to enforce a maximum image height?", "How many pixels tall should the limit be?", x => ImageSettings.MaxImageHeight = x, 1, null);
                        break;
                    case 7:
                        SetNullableInt("Do you want to enforce a maximum image width?", "How many pixels wide should the limit be?", x => ImageSettings.MaxImageWidth = x, 1, null);
                        break;
                    case 8:
                        SetInt("What quality do you want resized images to be saved with?", x => ImageSettings.ImageQuality = x, 1, 100);
                        break;
                }
            }

            using (var writer = new StreamWriter("ImageSettings.json"))
            {
                var serializer = new DataContractJsonSerializer(typeof(ImageSettings));
                serializer.WriteObject(writer.BaseStream, ImageSettings);
            }
        }

        public static void SetMiscSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Miscellaneous Settings");
                Console.WriteLine($"1 - Use Human Readable Filenames ({MiscSettings.UseHumanReadableFileNames})");
                Console.WriteLine($"2 - Remove Temporary Folder ({MiscSettings.RemoveTempFolder})");
                Console.WriteLine($"3 - Automatically Download Books ({MiscSettings.DownloadBooks})");
                Console.WriteLine($"4 - Source Folder ({MiscSettings.FolderDisplay(MiscSettings.InputFolder)})");
                Console.WriteLine($"5 - Output Folder ({MiscSettings.FolderDisplay(MiscSettings.OutputFolder)})");

                var line = Console.ReadLine();

                if (!int.TryParse(line, out var choice)) break;

                switch (choice)
                {
                    case 1:
                        SetBool("Do you want to use human readable filenames (which aren't supported by all epub readers) inside the epub file", x => MiscSettings.UseHumanReadableFileNames = x);
                        break;
                    case 2:
                        SetBool("Do you want to remove the temporary folder after creating the epub file", x => MiscSettings.RemoveTempFolder = x);
                        break;
                    case 3:
                        SetBool("Do you want to automatically download volumes from JNC", x => MiscSettings.DownloadBooks = x);
                        break;
                    case 4:
                        Console.Clear();
                        Console.WriteLine("Enter the new input folder.");
                        Console.WriteLine("This can be absolute or relative, leave blank for 'the folder this program is in'.");
                        MiscSettings.InputFolder = Console.ReadLine() ?? string.Empty;
                        break;
                    case 5:
                        Console.Clear();
                        Console.WriteLine("Enter the new output folder.");
                        Console.WriteLine("This can be absolute or relative, leave blank for 'the folder this program is in'.");
                        MiscSettings.OutputFolder = Console.ReadLine() ?? string.Empty;
                        break;
                    default:
                        break;
                }
            }

            using (var writer = new StreamWriter("MiscSettings.json"))
            {
                var serializer = new DataContractJsonSerializer(typeof(MiscSettings));
                serializer.WriteObject(writer.BaseStream, MiscSettings);
            }
        }

        private static void SetBool(string question, Action<bool> set)
        {
            Console.Clear();
            Console.WriteLine($"{question} Y/N?");

            var line = Console.ReadLine();

            if (line.Equals("y") || line.Equals("Y"))
            {
                set(true);
            }
            else
            {
                set(false);
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
    }
}
