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
            if (chapter.CType == Chapter.ChapterType.Bonus)
            {
                sb.AppendLine($"{prefix}{chapter.Name} [Bonus]");
            }
            if (chapter.CType == Chapter.ChapterType.NonStory)
            {
                sb.AppendLine($"{prefix}{chapter.Name} [Non-Story]");
            }
            if (chapter.CType == Chapter.ChapterType.Story)
            {
                sb.AppendLine($"{prefix}{chapter.Name}");
            }

            foreach(var subChapter in chapter.Chapters)
            {
                AddChapter(sb, subChapter, $"  {prefix}");
            }
        }
    }
}
