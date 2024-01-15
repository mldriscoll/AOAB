namespace OBB_WPF
{
    public static class ImportOld
    {
        public static async Task Import(Omnibus omnibus, string file)
        {
            var volumes = await JSON.Load<List<Volume>>(file);
            var nob = new Omnibus();
            foreach (var volume in volumes)
            {
                var sources = omnibus.AllSources(omnibus.Name + "\\" + volume.InternalName + "\\").ToList();
                foreach (var s in sources) omnibus.UnusedSources.Add(s);
                foreach (var chapter in volume.Chapters) await ImportChapter(nob, chapter, OBB_WPF.Chapter.ChapterType.Story, sources);
                foreach (var chapter in volume.BonusChapters) await ImportChapter(nob, chapter, OBB_WPF.Chapter.ChapterType.Bonus, sources);
                foreach (var chapter in volume.ExtraContent) await ImportChapter(nob, chapter, OBB_WPF.Chapter.ChapterType.NonStory, sources);

                await ImportChapter(nob, new Chapter { ChapterName = volume.InternalName, Chapters = new List<Chapter> { new Chapter { ChapterName = "Gallery", OriginalFilenames = volume.Gallery.SelectMany(x => x.SplashImages).ToList() } } }, OBB_WPF.Chapter.ChapterType.Bonus, sources);
            }
            omnibus.Combine(nob);
            omnibus.RemoveDupesFromUnusedList();
            omnibus.RemoveEmpties();
        }

        private static async Task ImportChapter(ChapterHolder holder, Chapter chapter, OBB_WPF.Chapter.ChapterType type, List<Source> Sources)
        {
            if (!string.IsNullOrWhiteSpace(chapter.SubFolder))
            {
                var firstslash = chapter.SubFolder.IndexOf('\\');
                var split = firstslash == -1 ? chapter.SubFolder : chapter.SubFolder.Split('\\')[0];
                chapter.SubFolder = chapter.SubFolder.Replace(split, string.Empty);
                var first = split.IndexOf('-');
                var newChapter = new Chapter
                {
                    SortOrder = split.Substring(0, first),
                    ChapterName = split.Substring(first),
                    Chapters = new List<Chapter> { chapter },
                };

                await ImportChapter(holder, newChapter, type, Sources);
            }
            else
            {
                var nchap = holder.Chapters.FirstOrDefault(x => x.SortOrder.Equals(chapter.SortOrder) && x.Name.Equals(chapter.ChapterName));
                if (nchap == null)
                {
                    nchap = new OBB_WPF.Chapter
                    {
                        CType = type,
                        Name = chapter.ChapterName,
                        SortOrder = chapter.SortOrder,
                    };
                    holder.Chapters.Add(nchap);
                }



                foreach (var file in chapter.OriginalFilenames)
                {
                    nchap.Sources.Add(Sources.FirstOrDefault(x => x.File.EndsWith($"{file}.xhtml")));
                }

                foreach (var splash in chapter.SplashPages)
                {
                    var right = nchap.Sources.FirstOrDefault(x => x.File.EndsWith($"{splash.Right}.xhtml"));
                    var left = nchap.Sources.FirstOrDefault(x => x.File.EndsWith($"{splash.Left}.xhtml"));
                    nchap.Sources.Remove(left);
                    right.OtherSide = left;
                }

                foreach (var subChapter in chapter.Chapters)
                {
                    await ImportChapter(nchap, subChapter, type, Sources);
                }
            }
        }

        class Volume
        {
            public string InternalName { get; set; } = string.Empty;

            public List<Chapter> Chapters { get; set; } = new List<Chapter>();
            public List<Chapter> BonusChapters { get; set; } = new List<Chapter>();
            public List<Chapter> ExtraContent { get; set; } = new List<Chapter>();
            public List<Gallery> Gallery { get; set; } = new List<Gallery>();
        }

        class Gallery : Chapter
        {
            public List<string> SplashImages { get; set; } = new List<string>();
            public List<string> ChapterImages { get; set; } = new List<string>();
            public string? LateSubFolder { get; set; }
        }

        class Chapter
        {
            public string ChapterName { get; set; } = string.Empty;
            public string SortOrder { get; set; } = string.Empty;
            public string SubFolder { get; set; } = string.Empty;
            public List<string> OriginalFilenames { get; set; } = new List<string>();
            public List<SplashPage> SplashPages { get; set; } = new List<SplashPage>();

            public List<Chapter> Chapters { get; set; } = new List<Chapter>();

            public List<ChapterSplit> Splits { get; set; } = new List<ChapterSplit>();

            public List<Replacements> Replacements { get; set; } = new List<Replacements>();

            public List<string> LinkedChapters { get; set; } = new List<string>();

            public bool KeepFirstSplitSection { get; set; } = true;
        }

        public class SplashPage
        {
            public string Left { get; set; } = string.Empty;
            public string Right { get; set; } = string.Empty;
        }
        public class ChapterSplit
        {
            public string Name { get; set; } = string.Empty;
            public string SplitLine { get; set; } = string.Empty;
            public string SortOrder { get; set; } = string.Empty;
            public string SubFolder { get; set; } = string.Empty;
        }
        public class Replacements
        {
            public string Original { get; set; } = string.Empty;
            public string Replacement { get; set; } = string.Empty;
        }
    }
}
