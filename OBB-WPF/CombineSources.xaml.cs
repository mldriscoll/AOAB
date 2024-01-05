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
    /// Interaction logic for CombineSources.xaml
    /// </summary>
    public partial class CombineSources : Window
    {
        private readonly Source one;
        private readonly Source two;

        public CombineSources(Source one, Source two)
        {
            InitializeComponent();
            this.one = one;
            this.two = two;
        }

        private void AlternateSource_Click(object sender, RoutedEventArgs e)
        {
            one.Alternates.Add(two.File);
            foreach (var alt in two.Alternates) one.Alternates.Add(alt);
            DialogResult = true;
            this.Close();
        }

        private void ImageCombine_Click(object sender, RoutedEventArgs e)
        {
            if (one.OtherSide == null) one.OtherSide = two;
            else
            {
                one.OtherSide.Alternates.Add(two.File);
                foreach (var alt in two.Alternates) one.OtherSide.Alternates.Add(alt);
            }
            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
