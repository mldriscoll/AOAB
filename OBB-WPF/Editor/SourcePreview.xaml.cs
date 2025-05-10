using CefSharp.DevTools.CSS;
using CefSharp.DevTools.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OBB_WPF
{
    /// <summary>
    /// Interaction logic for SourcePreview.xaml
    /// </summary>
    public partial class SourcePreview : UserControl
    {
        private string lSource = "about:blank";
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
                Left.Address = LeftSource;
            }
            if (e.Property == RightSourceProperty)
            {
                RightSource = (string)e.NewValue;
                Right.LoadUrl(RightSource);
            }
        }

        string rSource = "about:blank";
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

        public string SortOrder { get; set; } = String.Empty;
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
            await Left.WaitForInitialLoadAsync();
            await Right.WaitForInitialLoadAsync();

            if (!lSource.EndsWith("blank"))
            {
                Left.Address = lSource;
                await Left.WaitForNavigationAsync();
            }

            if (!rSource.EndsWith("blank"))
            {
                Right.Address = rSource;
                Right.LoadUrl(RightSource);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
