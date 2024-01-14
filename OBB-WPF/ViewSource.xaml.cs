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
    /// Interaction logic for ViewSource.xaml
    /// </summary>
    public partial class ViewSource : Window
    {
        public ViewSource(Source source)
        {
            InitializeComponent();

            if (File.Exists(source.File))
            {
                SourceBlock.Text = File.ReadAllText(source.File);
            }

            if (source.OtherSide != null)
            {
                if (File.Exists(source.OtherSide.File))
                {
                    SourceBlock.Text = string.Concat(SourceBlock.Text, File.ReadAllText(source.OtherSide.File));
                }
            }
        }
    }
}
