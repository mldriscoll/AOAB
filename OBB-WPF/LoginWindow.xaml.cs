using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly HttpClient client;

        public LoginWindow(HttpClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var login = await Login.FromUI(client, Username.Text, Password.Text);
            if (login != null)
            {
                DialogResult = true;
            }
            Close();
        }
    }
}
