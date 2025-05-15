namespace AOABO.Config
{
    public class OCRSettings {
        public List<Correction> Corrections { get; set; } = new List<Correction>();
        public List<Italics> Italics { get; set; } = new List<Italics>();
        public string? Header { get; set; }
        public bool Crop { get; set; } = false;
        public int HeaderLines { get; set; } = 1;
    }
}