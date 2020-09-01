using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog to browse files
            OpenFileDialog dialog = new OpenFileDialog
            {

                // Filter dialog to only show supported image types (or all files)
                Filter = "Images|*.jpg; *.png; *.gif; *.bmp; *.exif; *.tiff" +
                            "|All Files|*.*",

                // Set dialog to select multiple files
                Multiselect = true
            };

            // If user clicked OK (not Cancel) in file dialog
            if (dialog.ShowDialog() == true) {
                // Create stream from selected file
                Stream[] fileStreams = dialog.OpenFiles();

                // 
                foreach (Stream stream in fileStreams)
                {
                    WBImage image = new WBImage(stream);
                }
                
            }
        }
    }
}
