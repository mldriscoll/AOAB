using AOABO.Omnibus;
using System.Runtime.Serialization.Json;

namespace AOABO.Config
{
    public static class Configuration
    {
        public static readonly List<Volume> Volumes;
        public static readonly List<VolumeName> VolumeNames;
        public static VolumeOptions Options { get; set; }

        static Configuration()
        {
            using (var reader = new StreamReader("Volumes.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Volume[]));
                Volumes = ((Volume[])deserializer.ReadObject(reader.BaseStream)).ToList();
            }
            using (var reader = new StreamReader("VolumeNames.json"))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(VolumeName[]));
                VolumeNames = ((VolumeName[])deserializer.ReadObject(reader.BaseStream)).ToList();
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

        public static void UpdateOptions()
        {
            bool finished = false;
            while (!finished)
            {
                Console.Clear();

                Console.WriteLine("Which setting would you like to change?");
                Console.WriteLine("0 - Omnibus Structure");
                Console.WriteLine("1 - Bonus Chapter Placement");
                //Console.WriteLine("2 - Exclude Regular Chapters");
                Console.WriteLine("3 - Chapter Headers");
                //Console.WriteLine("4 - Chapter Inserts");
                //Console.WriteLine("5 - Afterwords");
                Console.WriteLine("6 - Internal Filenames");
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
                    //case '2':
                    //    SetRegularChapters();
                    //    break;
                    case '3':
                        SetChapterHeaders();
                        break;
                    //case '4':
                    //    SetChapterInserts();
                    //    break;
                    //case '5':
                    //    SetAfterwords();
                    //    break;
                    case '6':
                        SetFilenames();
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
                    Options.UseHumanReadableFileStructure = false;
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
            int? baseYear = null;
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
                    while (baseYear == null)
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