using AOABO.Omnibus;

namespace AOABO.Config
{
    public class VolumeOptions
    {
        public VolumeOptions()
        {
            UpdateChapterNames = false;
            UsePublishingOrder = false;
            StartYear = 5;
            OutputStructure = OutputStructure.Volumes;
            OutputYearFormat = 0;
        }

        public VolumeOptions(string str)
        {
            var split = str.Split("\r\n");
            if (split.Length > 0)
            {
                UpdateChapterNames = bool.Parse(split[0]);
            }
            if (split.Length > 1)
            {
                UsePublishingOrder = bool.Parse(split[1]);
            }
            if (split.Length > 2)
            {
                OutputStructure = (OutputStructure)Enum.Parse(typeof(OutputStructure), split[2]);
            }
            if (split.Length > 3)
            {
                StartYear = int.Parse(split[3]);
            }
            if (split.Length > 4)
            {
                OutputYearFormat = int.Parse(split[4]);
            }
        }

        public override string ToString()
        {
            return $"{UpdateChapterNames}\r\n{UsePublishingOrder}\r\n{OutputStructure}\r\n{StartYear}\r\n{OutputYearFormat}";
        }

        public bool UpdateChapterNames { get; set; }
        public bool UsePublishingOrder { get; set; }
        public OutputStructure OutputStructure { get; set; }
        public int StartYear { get; set; }
        public int OutputYearFormat { get; set; }
    }
}