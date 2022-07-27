namespace AOABO.Config
{
    public class Volume
    {
        public string? InternalName { get; set; }

        public override string ToString()
        {
            return InternalName ?? string.Empty;
        }

        public bool ProcessedInPartOne { get; set; } = false;
        public bool ProcessedInPartTwo { get; set; } = false;
        public bool ProcessedInPartThree { get; set; } = false;
        public bool ProcessedInPartFour { get; set; } = false;
        public bool ProcessedInPartFive { get; set; } = false;

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}