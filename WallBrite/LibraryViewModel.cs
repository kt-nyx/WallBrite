using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;

namespace WallBrite
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WBImage> LibraryList { get; }

        public LibraryViewModel()
        {
            // Create new empty library list
            LibraryList = new ObservableCollection<WBImage>();
        }

        /// <summary>
        /// Adds given WBImage at given filePath to the library; or throws exception if image is
        /// already in the library
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when image being added is already in library (its path matches the path of another image 
        /// already in the library)
        /// </exception>
        public void AddImage(WBImage image, string filePath)
        {
            // Return false and don't add this image if it's already in the library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(filePath)))
            {
                throw new InvalidOperationException("This image is already in the library");
            }

            // Add the image to the library and return true (if it's not already in the library)
            LibraryList.Add(image);
        }

        public void AddFiles()
        {
            // TODO: add errors for files already existing in library (addfile returns false in this case)
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
                string[] fileNames = dialog.FileNames;

                // Create the WBImage object for each image file and add it to library
                for (int i = 0; i < fileStreams.Length; i++)
                {
                    Stream stream = fileStreams[i];
                    string filePath = fileNames[i];
                    WBImage image = new WBImage(stream, filePath);
                    AddImage(image, filePath);
                }
            }
        }
        public void AddFolder()
        {
            // TODO: add errors for files already existing in library (addfile returns false in this case)
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

                    // Create WBImage for that file and add it to library
                    WBImage image = new WBImage(stream, filePath);
                    AddImage(image, filePath);
                }
            }
        }
    }
}
