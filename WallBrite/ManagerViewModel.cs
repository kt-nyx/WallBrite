using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    public class ManagerViewModel : INotifyPropertyChanged
    {
        // DLL Import, method reference, and constants for setting desktop wallpaper
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPIF_UPDATEINIFILE = 1;
        public const int SPIF_SENDCHANGE = 2;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime DarkestTime { get; set; }

        public DateTime BrightestTime { get; set; }

        public bool UsingRelativeChange { get; set; }

        public double CurrentDaylight { get; set; }

        public string ProgressReport { get; set; }

        public double Progress { get; set; }

        public BitmapImage CurrentClosestImageThumb { get; private set; }

        public void ManageWalls(LibraryViewModel library)
        {
            // Only do wallpaper management if there are wallpapers in the library
            if (library.LibraryList.Count > 0)
            {
                // Get current daylight value
                CurrentDaylight = UpdateCurrentDaylightSettings();
                // Find (enabled) image with brightness value closest to current daylight value
                WBImage closestImage =
                    library.LibraryList.Aggregate((x, y) =>
                                                           Math.Abs(x.AverageBrightness - CurrentDaylight)
                                                            < Math.Abs(y.AverageBrightness - CurrentDaylight)
                                                           && x.IsEnabled
                                                           ? x : y);
                // Set wallpaper to this image
                SetWall(closestImage);

                // Update current closest's thumb
                CurrentClosestImageThumb = closestImage.Thumbnail;
            }
        }

        public double UpdateCurrentDaylightSettings()
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

            double daylightSetting;
            // If brightness setting should be inverted relative to the percent covered, do so
            if (invertedBrightness)
            {
                daylightSetting = 1 - percentCovered;
            }
            // Otherwise just set the brightness setting to the percent covered
            else
            {
                daylightSetting = percentCovered;
            }

            // If approaching darkest time, update progress report to match
            if (invertedBrightness)
            {
                ProgressReport = Math.Round(percentCovered * 100) + "% of time covered before next darkest time";
            }
            // If approaching brightest time, update progress report to match
            else
            {
                ProgressReport = Math.Round(percentCovered * 100) + "% of time covered before next brightest time";
            }

            // Update front-end progress value
            Progress = percentCovered;

            return daylightSetting;
        }

        private void SetWall(WBImage image)
        {
            // TODO: add try catch
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, image.Path, SPIF_UPDATEINIFILE);
        }
    }
}