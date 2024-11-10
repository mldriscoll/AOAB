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
using static OBB_WPF.Chapter;

namespace OBB_WPF.Editor
{
    /// <summary>
    /// Interaction logic for SubsectionControl.xaml
    /// </summary>
    public partial class SubsectionControl : UserControl
    {
        private readonly Action<SubsectionControl> remove;

        public SubsectionControl(SubSection section, Action<SubsectionControl> remove)
        {
            InitializeComponent();
            this.remove = remove;
            StartRow.Text = section.StartsAtLine;
            EndRow.Text = section.EndsAtLine;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            remove(this);
        }
    }
}
