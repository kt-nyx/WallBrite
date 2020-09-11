using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WinForms = System.Windows.Forms;

namespace WallBrite
{
    public static class WBManager
    {
        // DLL Import, method reference, and constants for setting desktop wallpaper
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public const int SPI_SETDESKWALLPAPER = 20;

        public const int SPIF_UPDATEINIFILE = 1;

        public const int SPIF_SENDCHANGE = 2;

        public static DateTime DayStartCutoff { get; set; }
        public static DateTime DayEndCutoff { get; set; }
        public static int MinuteCheckFrequency { get; set; }
        public static float BrightnessCutoff { get; set; }
        public static bool UsingRelativeChange { get; set; }

        public static void ManageWalls()
        {
        }

        private static void SetWall(WBImage image)
        {
            // TODO: add try catch
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, image.Path, SPIF_UPDATEINIFILE);
        }

        public static void AddFiles()
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
                string[] fileNames = dialog.FileNames;

                // Create the WBImage object for each image file and add it to library
                for (int i = 0; i < fileStreams.Length; i++)
                {
                    Stream stream = fileStreams[i];
                    string filePath = fileNames[i];
                    WBImage image = new WBImage(stream, filePath);
                    WBLibrary.AddImage(image);
                }
            }
        }

        public static void AddFolder()
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

                    // Create WBImage for that file and add it to library
                    WBImage image = new WBImage(stream, filePath);
                    WBLibrary.AddImage(image);
                }
            }
        }
    }
}