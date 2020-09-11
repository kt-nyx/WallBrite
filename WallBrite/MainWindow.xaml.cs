using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace WallBrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddFiles(object sender, RoutedEventArgs e)
        {
            WBManager.AddFiles();
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            WBManager.AddFolder();
        }
    }
}