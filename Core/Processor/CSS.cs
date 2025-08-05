namespace Core.Processor
{
    public class CSS
    {
        public string Contents { get; set; } = string.Empty;
        public List<string> OldNames { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;

        public string ReplacementName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return string.Empty;
                if (Name.StartsWith('.')) return Name.Substring(1);
                return Name;
            }
        }

        public override string ToString()
        {
            return $"{OldNames.Aggregate("-----\r\n" + Name, (all, current) => string.Concat(all, "\r\n", current))}\r\n{Contents}";
        }
    }
}