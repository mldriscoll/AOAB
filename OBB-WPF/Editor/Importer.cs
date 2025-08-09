using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace OBB_WPF.Editor
{
    public static class Importer
    {
        private static readonly Regex chapterTitleRegex = new Regex("<h1>[\\s\\S]*?<\\/h1>");
        private static readonly Regex chapterSubTitleRegex = new Regex("<h2>[\\s\\S]*?<\\/h2>");
        public static Omnibus GenerateVolumeInfo(string inFolder, string series, string volumeName, int volOrder)
        {
            if (!Directory.Exists(inFolder)) return new Omnibus { };
            var ob = new Omnibus
            {
                Chapters = new System.Collections.ObjectModel.ObservableCollection<Chapter>
                {
                    new Chapter
                    {
                        Name = volumeName,
                        SortOrder = volOrder.ToString("000")
                    }
                }
            };
            int order = 1;

            try
            {
                order = ProcessFromNCX(inFolder.Replace("/", "\\"), volOrder, ob, order, $"{inFolder}\\item\\xhtml\\");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return ob;
        }

        private static int ProcessFromNCX(string inFolder, int volOrder, Omnibus ob, int order, string textFolder)
        {
            var oebps = Directory.GetDirectories(inFolder, "OEBPS", SearchOption.AllDirectories);
            var item = Directory.GetDirectories(inFolder, "item", SearchOption.AllDirectories);
            var contentFolder = oebps.Length > 0 ? oebps[0] : item[0];

            var xmlSerializer = new XmlSerializer(typeof(Package));
            var opfFile = Directory.GetFiles(inFolder, "*.opf", SearchOption.AllDirectories)[0];
            var content = (xmlSerializer.Deserialize(File.OpenRead(opfFile)) as Package)!;

            var ncxFile = Directory.GetFiles(inFolder, "*.ncx", SearchOption.AllDirectories)[0];
            var serializer = new XmlSerializer(typeof(NCX));
            var ncx = (serializer.Deserialize(File.OpenRead(ncxFile)) as NCX)!;

            var navMap = ncx.navMap.SelectMany(ParseNavMap);

            var currentChapter = ob.Chapters[0];
            int sourceOrder = 1;
            foreach (var line in content.Spine)
            {
                var match = navMap.FirstOrDefault(x => x.Id.EndsWith(line.Id)) 
                    ?? navMap.FirstOrDefault(x => x.Link.Source.EndsWith(line.Id))
                    ?? navMap.FirstOrDefault(y => y.Link.Source.EndsWith(content.Manifest.FirstOrDefault(x => x.Id.EndsWith(line.Id))?.Href ?? "..."));
                if (match != null)
                {
                    var sourceText = currentChapter.Sources.Aggregate(string.Empty, (str, source) => string.Concat(str, File.ReadAllText(source.RightURI)));

                    var title = chapterTitleRegex.Match(sourceText);
                    if (title.Success)
                    {
                        currentChapter.Name = title.Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty);
                    }

                    var chapterTitles = chapterTitleRegex.Matches(sourceText).Skip(1) //First Title should be the base chapter
                        .Union(chapterSubTitleRegex.Matches(sourceText)).OrderBy(x => x.Index);

                    Chapter? subChapter = null;
                    int subIndex = 1;
                    foreach(var titleMatch in chapterTitles.Where(x => !x.Value.Equals("<h2>I</h2>") && !x.Value.Equals("<h2>1</h2>")))
                    {
                        if (subChapter != null)
                        {
                            subChapter.EndsBeforeLine = titleMatch.Value;
                        }
                        else
                        {
                            currentChapter.EndsBeforeLine = titleMatch.Value;
                        }

                        subChapter = new Chapter
                        {
                            Name = titleMatch.Value.Replace("<h1>", string.Empty).Replace("</h1>", string.Empty).Replace("<h2>", string.Empty).Replace("</h2>", string.Empty),
                            StartsAtLine = titleMatch.Value,
                            SortOrder = currentChapter.SortOrder + subIndex.ToString("00"),
                            Sources = currentChapter.Sources
                        };
                        currentChapter.Chapters.Add(subChapter);
                        subIndex++;
                    }


                    currentChapter = new Chapter { Name = match.Label.Name, SortOrder = (volOrder * 100 + order).ToString("00000") };
                    ob.Chapters[0].Chapters.Add(currentChapter);
                    order++;
                }
                var manifestLine = content.Manifest.First(x => x.Id.Equals(line.Id));
                currentChapter.Sources.Add(new Source { File = $"{contentFolder}\\{manifestLine.Href}", SortOrder = $"{volOrder:000}{order:00}{sourceOrder:000}" });
                sourceOrder++;
            }

            return order;
        }

        public static IEnumerable<NCX.navPoint> ParseNavMap(NCX.navPoint navPoint)
        {
            yield return navPoint;
            if (navPoint.innerPoints != null)
            {
                foreach (var point in navPoint.innerPoints.SelectMany(ParseNavMap))
                {
                    yield return point;
                }
            }
        }
    }

    [Serializable, XmlRoot(ElementName = "package", Namespace = "http://www.idpf.org/2007/opf")]
    [XmlType("package")]
    public class Package
    {
        [XmlElement(ElementName = "metadata")]
        public Metadata Metadata { get; set; } = new Metadata();

        [XmlArray(ElementName = "manifest")]
        public List<Item> Manifest { get; set; } = new List<Item>();

        [XmlArray(ElementName = "spine")]
        public List<Itemref> Spine { get; set; } = new List<Itemref>();
    }
    public class Metadata
    {

    }

    [XmlType("item")]
    public class Item
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = String.Empty;
        [XmlAttribute("href")]
        public string Href { get; set; } = String.Empty;
    }

    [XmlType("itemref")]
    public class Itemref
    {
        [XmlAttribute("idref")]
        public string Id { get; set; } = String.Empty;
    }

    [Serializable, XmlRoot(ElementName = "ncx", Namespace = "http://www.daisy.org/z3986/2005/ncx/")]
    [XmlType("ncx")]
    public class NCX
    {
        public List<navPoint> navMap { get; set; } = new List<navPoint>();

        public class navPoint
        {
            [XmlAttribute("playOrder")]
            public int Order { get; set; } = 0;
            [XmlAttribute("id")]
            public string Id { get; set; } = string.Empty;

            [XmlElement("navLabel")]
            public NavLabel Label { get; set; } = new NavLabel { };

            [XmlElement("content")]
            public Content Link { get; set; } = new Content { };

            [XmlType("navLabel")]
            public class NavLabel
            {
                [XmlElement("text")]
                public string Name { get; set; } = string.Empty;
            }
            [XmlType("content")]
            public class Content
            {
                [XmlAttribute("src")]
                public string Source { get; set; } = string.Empty;
            }
            [XmlElement("navPoint")]
            public navPoint[] innerPoints { get; set; } = [];
        }
    }
}
