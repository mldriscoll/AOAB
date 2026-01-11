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

namespace OBB_WPF.Editor
{
    /// <summary>
    /// Interaction logic for TagsPopup.xaml
    /// </summary>
    public partial class TagsPopup : Window
    {
        private readonly Chapter chapter;

        public TagsPopup(Chapter chapter)
        {
            InitializeComponent();
            this.chapter = chapter;
            var tagNames = new TextBox[] { Tag1, Tag2, Tag3, Tag4, Tag5, Tag6, Tag7, Tag8, Tag9, Tag10 };
            var tagValues = new TextBox[] { TagValue1, TagValue2, TagValue3, TagValue4, TagValue5, TagValue6, TagValue7, TagValue8, TagValue9, TagValue10 };
            for(int i = 0; (i < chapter.Tags.Count) && (i < 11); i++)
            {
                tagNames[i].Text = chapter.Tags[i].Name;
                tagValues[i].Text = chapter.Tags[i].Value;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            chapter.Tags.Clear();
            if (!string.IsNullOrWhiteSpace(TagValue1.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag1.Text, Value = TagValue1.Text });
            if (!string.IsNullOrWhiteSpace(TagValue2.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag2.Text, Value = TagValue2.Text });
            if (!string.IsNullOrWhiteSpace(TagValue3.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag3.Text, Value = TagValue3.Text });
            if (!string.IsNullOrWhiteSpace(TagValue4.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag4.Text, Value = TagValue4.Text });
            if (!string.IsNullOrWhiteSpace(TagValue5.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag5.Text, Value = TagValue5.Text });
            if (!string.IsNullOrWhiteSpace(TagValue6.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag6.Text, Value = TagValue6.Text });
            if (!string.IsNullOrWhiteSpace(TagValue7.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag7.Text, Value = TagValue7.Text });
            if (!string.IsNullOrWhiteSpace(TagValue8.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag8.Text, Value = TagValue8.Text });
            if (!string.IsNullOrWhiteSpace(TagValue9.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag9.Text, Value = TagValue9.Text });
            if (!string.IsNullOrWhiteSpace(TagValue10.Text)) chapter.Tags.Add(new Chapter.Tag { Name = Tag10.Text, Value = TagValue10.Text });
            this.Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            chapter.Tags.Clear();
            this.Close();
        }
    }
}
