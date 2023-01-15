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
            Console.Clear();
            Options.OutputStructure = OutputStructure.Flat;
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

            Options.UsePublishingOrder = false;
            Console.WriteLine();
            Console.WriteLine("Would you like to use the publishing order instead of the chronological order? Y/N");
            key = Console.ReadKey();
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.UsePublishingOrder = true;
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Would you like the chapter headers updated to match their titles in the index? Y/N");
            key = Console.ReadKey();
            Options.UpdateChapterNames = false;
            switch (key.KeyChar)
            {
                case 'y':
                case 'Y':
                    Options.UpdateChapterNames = true;
                    break;
            }

            if (File.Exists("options.txt"))
            {
                File.Delete("options.txt");
            }
            File.WriteAllText("options.txt", Options.ToString());
        }
    }
}