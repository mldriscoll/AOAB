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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBB_WPF.Editor
{
    /// <summary>
    /// Interaction logic for SourceLineControl.xaml
    /// </summary>
    public partial class SourceLineControl : UserControl
    {
        private readonly string line1;
        private readonly int index;
        private readonly Action<string, int> setStart;
        private readonly Action<string, int> setEnd;

        public SourceLineControl(string line, int index, Action<string, int> setStart, Action<string, int> setEnd)
        {
            InitializeComponent();
            line1 = line;
            this.index = index;
            this.setStart = setStart;
            this.setEnd = setEnd;
            this.line.Text = line1;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            setStart(line1, index);
        }

        private void End_Click(object sender, RoutedEventArgs e)
        {
            setEnd(line1, index);
        }
    }
}
