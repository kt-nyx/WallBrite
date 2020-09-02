using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

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

        private void Add_Files(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog to browse files
            OpenFileDialog dialog = new OpenFileDialog
            {
                // Filter dialog to only show supported image types (or all files)
                Filter = "Images|*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.exif; *.tiff" +
                            "|All Files|*.*",

                // Set dialog to select multiple files
                Multiselect = true
            };

            // If user clicked OK (not Cancel) in file dialog
            if (dialog.ShowDialog() == true)
            {
                // TODO: add try catch for possible exceptions
                // Create streams from selected files
                Stream[] fileStreams = dialog.OpenFiles();

                //
                foreach (Stream stream in fileStreams)
                {
                    WBImage image = new WBImage(stream);
                }
            }
        }

        private void Add_Folder(object sender, RoutedEventArgs e)
        {
            // Create FolderBrowserDialog to browse folders
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();

            // If user clicked OK (not Cancel) in folder dialog
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                // Get path string from selected path
                string folderPath = dialog.SelectedPath;

                // TODO: add try catch for possible exceptions of getfiles; pathtoolong??
                // Get paths of all supported image files in this folder and subfolders
                IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath,
                                                    "*.*",
                                                    SearchOption.AllDirectories)
                                     .Where(filePath => filePath.EndsWith(".jpg")
                                                        || filePath.EndsWith(".jpeg")
                                                        || filePath.EndsWith(".png")
                                                        || filePath.EndsWith(".gif")
                                                        || filePath.EndsWith(".bmp")
                                                        || filePath.EndsWith(".exif")
                                                        || filePath.EndsWith(".tiff"));

                // Loop over each filePath in the array of filePaths (over each file in
                // selected folder/subfolders)
                foreach (string filePath in filePaths)
                {
                    // TODO: add try catch for possible exceptions
                    // Open file stream for file at this path
                    FileStream stream = File.Open(filePath, FileMode.Open);

                    // Create WBImage for that file
                    WBImage image = new WBImage(stream);
                }
            }
        }
    }
}