using Microsoft.Win32;
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

namespace OBB_WPF.Custom
{
    /// <summary>
    /// Interaction logic for CreateCustomSeries.xaml
    /// </summary>
    public partial class CreateCustomSeries : Window
    {
        public CreateCustomSeries()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(SeriesName.Text))
            {
                var filePopup = new OpenFileDialog();
                filePopup.Multiselect = true;
                filePopup.CheckFileExists = true;
                filePopup.InitialDirectory = MainWindow.Configuration.SourceFolder;

                if (filePopup.ShowDialog() ?? false)
                {
                    if (filePopup.FileNames.Count() > 1)
                    {
                        var series = new Library.Series { InternalName = SeriesName.Text, Name = SeriesName.Text };
                        foreach (var file in filePopup.FileNames)
                        {
                            var finfo = new FileInfo(file);
                            series.Volumes.Add(new Library.VolumeName
                            {
                                ApiSlug = finfo.Name,
                                FileName = file,
                                Title = finfo.Name,
                            });
                        }
                        MainWindow.CustomSeries.Add(series);
                        this.Close();
                    }
                }
                await JSON.Save("JSON\\CustomSeries.json", MainWindow.CustomSeries);
            }
        }
    }
}
