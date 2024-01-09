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
    /// Interaction logic for CreateLink.xaml
    /// </summary>
    public partial class CreateLink : Window
    {
        private readonly Chapter chapter;

        public CreateLink(Chapter chapter)
        {
            InitializeComponent();
            this.chapter = chapter;
            Title = $"Add a hyperlink in {chapter.Name}";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chapter.LinkedChapters.Add(new Link
            {
                OriginalLink = Link.Text,
                Target = Target.Text
            });
            Close();
        }
    }
}
