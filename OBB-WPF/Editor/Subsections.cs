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

namespace OBB_WPF.Editor
{
    /// <summary>
    /// Interaction logic for Subsections.xaml
    /// </summary>
    public partial class Subsections : Window
    {
        private readonly Chapter chapter;
        int startIndex = 0;
        int endIndex = 0;

        public Subsections(Chapter chapter)
        {
            InitializeComponent();

            string combinedSource = string.Empty;
            foreach (var source in chapter.Sources.OrderBy(x => x.SortOrder))
            {
                if (File.Exists(source.File))
                {
                    combinedSource = string.Concat(combinedSource, File.ReadAllText(source.File));
                }
            }

            int index = 0;
            var sourceLines = combinedSource.Split("\n");
            foreach (var line in sourceLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (!string.IsNullOrWhiteSpace(chapter.StartsAtLine) && line.Contains(chapter.StartsAtLine))
                    {
                        StartPoint.Text = line.Trim();
                        startIndex = index;
                    }

                    if (!string.IsNullOrWhiteSpace(chapter.EndsBeforeLine) && line.Contains(chapter.EndsBeforeLine))
                    {
                        EndPoint.Text = ((SourceLineControl)SourceLines.Items[SourceLines.Items.Count - 1]).line.Text;
                        endIndex = index;
                    }

                    var control = new SourceLineControl(line.Trim(), index, (string s, int i) => { StartPoint.Text = s; startIndex = i; }, (string s, int i) => { EndPoint.Text = s; endIndex = i; });
                    if (line.Contains("<h1>")) control.Background = Brushes.AliceBlue;
                    if (line.Contains("<h2>")) control.Background = Brushes.Aquamarine;
                    control.HorizontalAlignment = HorizontalAlignment.Stretch;
                    SourceLines.Items.Add(control);
                }
                index += line.Length + 1;
            }

            foreach (var section in chapter.SubSections)
            {
                SubsectionList.Items.Add(new SubsectionControl(section, (x) => { SubsectionList.Items.Remove(x); chapter.SubSections.Remove(section); }));
            }

            this.chapter = chapter;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StartPoint.Text) && string.IsNullOrWhiteSpace(EndPoint.Text))
            {
                this.Close();
                return;
            }

            var sub = new Chapter.SubSection { };

            if (!string.IsNullOrWhiteSpace(StartPoint.Text)) sub.StartsAtLine = StartPoint.Text;
            if (!string.IsNullOrWhiteSpace(EndPoint.Text)) sub.EndsAtLine = EndPoint.Text;

            sub.StartsAtIndex = startIndex;
            sub.EndsAtIndex = endIndex;

            chapter.SubSections.Add(sub);
            SubsectionList.Items.Add(new SubsectionControl(sub, (SubsectionControl c) => { SubsectionList.Items.Remove(c); chapter.SubSections.Remove(sub); }));

            chapter.StartsAtLine = String.Empty;
            chapter.EndsBeforeLine = String.Empty;
        }
    }
}
