﻿using System;
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
    /// Interaction logic for PickEditorNameWindow.xaml
    /// </summary>
    public partial class PickEditorNameWindow : Window
    {
        public PickEditorNameWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(EditorName.Text))
            {
                MainWindow.Configuration.EditorName = EditorName.Text;
                await JSON.Save("Configuration.json", MainWindow.Configuration);
                this.Close();
            }
        }
    }
}
