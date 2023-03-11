namespace Core.Processor
{
    public class CSS
    {
        public string Contents { get; set; } = string.Empty;
        public List<string> OldNames { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{OldNames.Aggregate("-----\r\n" + Name, (all, current) => string.Concat(all, "\r\n", current))}\r\n{Contents}";
        }
    }
}