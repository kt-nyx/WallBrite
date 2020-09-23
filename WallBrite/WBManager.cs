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
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPIF_UPDATEINIFILE = 1;
        public const int SPIF_SENDCHANGE = 2;

        public static DateTime DarkestTime { get; set; }

        public static DateTime BrightestTime { get; set; }

        public static bool UsingRelativeChange { get; set; }


        public static void ManageWalls()
        {
            // Only do wallpaper management if there are wallpapers in the library
            if (WBLibrary.LibraryList.Count > 0) { 
                // Get current daylight value
                double currentDaylight = GetCurrentDaylightSetting();
                // Find image with brightness value closest to current daylight value
                WBImage closestImage =
                    WBLibrary.LibraryList.Aggregate((x, y) =>
                                                           Math.Abs(x.AverageBrightness - currentDaylight)
                                                           < Math.Abs(y.AverageBrightness - currentDaylight)
                                                           ? x : y);
                // Set wallpaper to this image
                SetWall(closestImage);
            }
        }

        public static double GetCurrentDaylightSetting()
        {
            // Get the current time in minutes since midnight
            double now = DateTime.Now.TimeOfDay.TotalMinutes;

            // Get BrightestTime in minutes since midnight
            double brightestInMins = BrightestTime.TimeOfDay.TotalMinutes;

            // Get DarkestTime in minutes since midnight
            double darkestInMins = DarkestTime.TimeOfDay.TotalMinutes;

            // If it is currently brightest time, return 1 (max brightness)
            if (now == brightestInMins)
            {
                return 1;
            }
            // If it is currently darkest time, return 0 (min brightness)
            else if (now == darkestInMins)
            {
                return 0;
            }

            // Total number of minutes in a day
            const double minutesInDay = 1439;

            // For use in case calculations
            double minutesCovered;
            double brightestToDarkest;
            bool invertedBrightness;

            // Case 1: Now is between brightest time and darkest time (respectively)
            if (brightestInMins < now && now < darkestInMins)
            {
                // Get total minutes covered since brightest time
                minutesCovered = now - brightestInMins;
                // Get total minutes between brightest and darkest times
                brightestToDarkest = darkestInMins - brightestInMins;

                // Since approaching the darkest time, brightness value will be inverted (should be
                // approaching 0 rather than 1
                invertedBrightness = true;
            }
            // Case 2: Now is between darkest time and brightest time (respectively)
            else if (darkestInMins < now && now < brightestInMins)
            {
                // Get remaining minutes before reaching the brightest time
                minutesCovered = now - darkestInMins;
                // Get total minutes between darkest and brightest times
                brightestToDarkest = brightestInMins - darkestInMins;

                // Since approaching the brightest time, brightness value will be proportional to the
                // percentage of the time already covered (should be approaching 1 rather than 0)
                invertedBrightness = false;
            }
            // Case 3: Now is before brightest time and brightest time is before darkest time
            else if (now < brightestInMins && brightestInMins < darkestInMins)
            {
                // In this case, the calculation must loop around the day (from darkest time to midnight,
                // then around again from midnight to now)
                minutesCovered = (minutesInDay - darkestInMins) + now;

                // In this case, the calculation must loop around the day (from darkest time to midnight,
                // then around again from midnight to brightest time)
                brightestToDarkest = (minutesInDay - darkestInMins) + brightestInMins;

                invertedBrightness = false;
            }
            // Case 4: Now is before darkest time and darkest time is before brightest time
            else if (now < darkestInMins && darkestInMins < brightestInMins)
            {
                minutesCovered = (minutesInDay - brightestInMins) + now;

                brightestToDarkest = (minutesInDay - brightestInMins) + darkestInMins;

                invertedBrightness = true;
            }
            // Case 5: Brightest time is before darkest time and darkest time is before now
            else if (brightestInMins < darkestInMins && darkestInMins < now)
            {
                minutesCovered = now - darkestInMins;

                brightestToDarkest = (minutesInDay - darkestInMins) + brightestInMins;

                invertedBrightness = false;
            }
            // Case 6: Darkest time is before brightest time and brightest time is before now
            else if (darkestInMins < brightestInMins && brightestInMins < now)
            {
                minutesCovered = now - brightestInMins;

                brightestToDarkest = (minutesInDay - brightestInMins) + darkestInMins;

                invertedBrightness = true;
            }
            else
            {
                throw new InvalidOperationException("The darkest time, brightest time, or current time is invalid");
            }

            // Get percent of time between brightest and darkest times which has already been covered
            double percentCovered = minutesCovered / brightestToDarkest;

            double brightnessSetting;
            // If brightness setting should be inverted relative to the percent covered, do so
            if (invertedBrightness)
            {
                brightnessSetting = 1 - percentCovered;
            }
            // Otherwise just set the brightness setting to the percent covered
            else
            {
                brightnessSetting = percentCovered;
            }

            return brightnessSetting;
        }

        private static void SetWall(WBImage image)
        {
            // TODO: add try catch
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, image.Path, SPIF_UPDATEINIFILE);
        }

        public static void AddFiles()
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
                    WBLibrary.AddImage(image, filePath);
                }
            }
        }

        public static void AddFolder()
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
                    WBLibrary.AddImage(image, filePath);
                }
            }
        }
    }
}