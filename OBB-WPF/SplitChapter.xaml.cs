using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for SplitChapter.xaml
    /// </summary>
    public partial class SplitChapter : Window
    {
        private readonly Chapter chapter;

        public SplitChapter(Chapter chapter)
        {
            InitializeComponent();

            foreach (var source in chapter.Sources)
            {
                if (File.Exists(source.File))
                {
                    SourceBlock.Text = string.Concat(SourceBlock.Text, File.ReadAllText(source.File));
                }
            }

            this.chapter = chapter;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newChapter = new Chapter
            {
                CType = chapter.CType,
                Name = chapter.Name,
                SortOrder = chapter.SortOrder + "x",
                Sources = new System.Collections.ObjectModel.ObservableCollection<Source>( chapter.Sources),
            };
            chapter.Chapters.Add(newChapter);
            if (!string.IsNullOrWhiteSpace(CutPoint.Text))
            {
                chapter.EndsBeforeLine = CutPoint.Text;
                newChapter.StartsAtLine = CutPoint.Text;
            }
            this.Close();
        }
    }
}
