namespace AOABO.Omnibus
{
    public class NavPoint
    {
        public string Label { get; set; } = String.Empty;
        public string Source { get; set; } = String.Empty;
        public int Id { get; set; }
        public int Tabs { get; set; }
        public List<NavPoint> navPoints { get; set; } = new List<NavPoint>();

        public int MaxTabs
        {
            get
            {
                return navPoints.Any() ?
                    navPoints.Max(x => x.MaxTabs)
                    : Tabs;
            }
        }

        public override string ToString()
        {
            foreach (var np in navPoints)
            {
                np.Tabs = Tabs + 1;
            }

            var shortSpace = "    ";
            for (int i = 0; i < Tabs; i++)
            {
                shortSpace = shortSpace + "  ";
            }

            return $"{shortSpace}<navPoint id=\"num_{Id}\" playOrder=\"{Id}\">\r\n"
                + $"{shortSpace}  <navLabel>\r\n"
                + $"{shortSpace}    <text>{Label}</text>\r\n"
                + $"{shortSpace}  </navLabel>\r\n"
                + $"{shortSpace}  <content src=\"{Source}\"/>\r\n"
                + navPoints.Aggregate(string.Empty, (agg, np) => string.Concat(agg, np.ToString(), "\r\n"))
                + $"{shortSpace}</navPoint>";
        }
    }
}