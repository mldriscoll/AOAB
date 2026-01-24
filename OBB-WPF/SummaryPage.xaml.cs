using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for SummaryPage.xaml
    /// </summary>
    public partial class SummaryPage : Window
    {
        public SummaryPage(Omnibus omnibus)
        {
            InitializeComponent();
            var str = new StringBuilder();
            if (omnibus.Cover != null)
            {
                str.AppendLine($"* Cover {omnibus.Cover.File}");
            }
            foreach(var chapter in omnibus.Chapters)
            {
                AddChapter(str, chapter, "* ");
            }
            SummaryBox.Text = str.ToString();
        }

        private void AddChapter(StringBuilder sb, Chapter chapter, string prefix)
        {
            var pov = string.IsNullOrWhiteSpace(chapter.POV) ? string.Empty : $" [{chapter.POV}]";
            var source = string.Empty;
            if (chapter.Tags.Any(x => x.Name.Equals("Source"))) source = string.Concat(" (", chapter.Tags.First(x => x.Name.Equals("Source")).Value, ")");
            switch (chapter.CType)
            {
                case Chapter.ChapterType.DramaCD:
                    sb.AppendLine($"{prefix}[Drama CD Recap] {chapter.Name}");
                    break;
                case Chapter.ChapterType.QnAs:
                    sb.AppendLine($"{prefix}[Q&A] {chapter.Name}");
                    break;
                case Chapter.ChapterType.Poll:
                    sb.AppendLine($"{prefix}[Character Poll] {chapter.Name}");
                    break;
                case Chapter.ChapterType.ComfyLife:
                    sb.AppendLine($"{prefix}[Comfy Life Manga] {chapter.Name}");
                    break;
                case Chapter.ChapterType.Story:
                case Chapter.ChapterType.Part:
                case Chapter.ChapterType.Volume:
                    sb.AppendLine($"{prefix}**{chapter.Name}**");
                    break;
                case Chapter.ChapterType.Bonus:
                    sb.AppendLine($"{prefix}[Bonus{source}] *{chapter.Name}{pov}*");
                    break;
                case Chapter.ChapterType.NonStory:
                    sb.AppendLine($"{prefix}[Non-Story] {chapter.Name}");
                    break;
                case Chapter.ChapterType.Map:
                    sb.AppendLine($"{prefix}[Map] {chapter.Name}");
                    break;
                case Chapter.ChapterType.CharacterSheet:
                    sb.AppendLine($"{prefix}[Character Sheet] {chapter.Name}");
                    break;
                case Chapter.ChapterType.Afterword:
                    sb.AppendLine($"{prefix}[Afterword] {chapter.Name}");
                    break;
                case Chapter.ChapterType.Fanbook:
                    sb.AppendLine($"{prefix}[Fanbook] {chapter.Name}");
                    break;
                case Chapter.ChapterType.MangaWritten:
                    sb.AppendLine($"{prefix}[Bonus OCR{source}] *{chapter.Name}{pov}*");
                    break;
                case Chapter.ChapterType.Covers:
                    sb.AppendLine($"{prefix}[Cover Art] {chapter.Name}");
                    break;
            }

            foreach(var subChapter in chapter.Chapters)
            {
                AddChapter(sb, subChapter, $"  {prefix}");
            }
        }
    }
}
