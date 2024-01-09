using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for SourcePreview.xaml
    /// </summary>
    public partial class SourcePreview : UserControl
    {
        private string lSource;
        public string LeftSource {
            get { return lSource; }
            set {
                lSource = $"file://{Environment.CurrentDirectory}\\{value}";
            }
        }
        public static readonly DependencyProperty LeftSourceProperty =
            DependencyProperty.Register(
                "LeftSource",
                typeof(string),
                typeof(SourcePreview));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == LeftSourceProperty)
            {
                LeftSource = (string)e.NewValue;
            }
            if (e.Property == RightSourceProperty)
            {
                RightSource = (string)e.NewValue;
            }
        }

        string rSource;
        public string RightSource
        {
            get { return rSource; }
            set
            {
                rSource = $"file://{Environment.CurrentDirectory}\\{value}";
            }
        }
        public static readonly DependencyProperty RightSourceProperty =
            DependencyProperty.Register(
                "RightSource",
                typeof(string),
                typeof(SourcePreview));

        public string SortOrder { get; set; }
        public static readonly DependencyProperty SortOrderProperty = 
            DependencyProperty.Register(
                "SortOrder",
                typeof(string),
                typeof(SourcePreview));

        public SourcePreview()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await Left.EnsureCoreWebView2Async();
            Left.CoreWebView2.Navigate(lSource);
            if (lSource.EndsWith("blank"))
            {
                LeftColumn.Width = new GridLength(0, GridUnitType.Pixel);
            }
            await Right.EnsureCoreWebView2Async();
            Right.CoreWebView2.Navigate(rSource); 
            if (rSource.EndsWith("blank"))
            {
                RightColumn.Width = new GridLength(0, GridUnitType.Pixel);
            }
        }
    }
}
