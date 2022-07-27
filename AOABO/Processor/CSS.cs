namespace AOABO.Processor
{
    public class CSS
    {
        public string Contents { get; set; }
        public List<string> OldNames { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{OldNames.Aggregate("-----\r\n" + Name, (all, current) => string.Concat(all, "\r\n", current))}\r\n{Contents}";
        }
    }
}