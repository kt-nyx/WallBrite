using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media;
using System.IO;

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

        public ICommand UpdateCommand { get; set; }

        private int _updateIntervalHours;
        private int _updateIntervalMins;
        private TimeSpan UpdateInterval;

        private DispatcherTimer managerTimer;
        private DispatcherTimer timerTracker;
        private TimeSpan managerTimeRemaining;
        private DateTime managerStartTime;
        
        public DateTime DarkestTime { get; set; }

        public DateTime BrightestTime { get; set; }

        public int UpdateIntervalHours
        {
            get { return _updateIntervalHours; }
            set
            {
                _updateIntervalHours = value;

                // Prevent user from setting interval to 0 hours 0 mins
                if (_updateIntervalMins == 0)
                {
                    _updateIntervalMins = 1;
                    NotifyPropertyChanged("UpdateIntervalMins");
                    UpdateInterval = new TimeSpan(UpdateIntervalHours, 1, 0);
                }
                else {
                    UpdateInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);
                }
                
                NotifyPropertyChanged("UpdateIntervalHours");
            }
        }

        public int UpdateIntervalMins
        {
            get { return _updateIntervalMins; }
            set
            {
                _updateIntervalMins = value;
                UpdateInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);
                NotifyPropertyChanged("UpdateIntervalMins");
            }
        }

        public double CurrentDaylightPercent { get; set; }

        public string ProgressReport { get; private set; }

        public double Progress { get; set; }

        public BitmapImage ClosestImageThumb { get; private set; }

        public SolidColorBrush ClosestImageBack { get; private set; }

        public double ClosestImageBrightnessPercent { get; set; }

        public string ClosestImageFilename { get; set; }

        public ManagerViewModel(LibraryViewModel library)
        {
            // Create command(s)
            UpdateCommand = new RelayCommand((object s) => UpdateWall(library));

            // Set default property values
            // Update interval 30 mins, brightest time 1:00 PM, darkest time 11:00 PM
            UpdateIntervalHours = 0;
            UpdateIntervalMins = 1;
            DateTime now = DateTime.Now;
            BrightestTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
            DarkestTime = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);

            // Create timer thread for updating walls
            managerTimer = new DispatcherTimer();
            managerTimer.Interval = UpdateInterval;
            managerTimer.Tick += (object s, EventArgs a) => UpdateWall(library);

            // Create timer that keeps track of time remaining on this ^^^ timer (checks every second)
            timerTracker = new DispatcherTimer();
            timerTracker.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timerTracker.Tick += UpdateTimer;

            // Set start time as now and start the timers
            managerStartTime = DateTime.Now;
            managerTimer.Start();
            timerTracker.Start();
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            TimeSpan timeElapsed = DateTime.Now - managerStartTime;
            managerTimeRemaining = UpdateInterval.Subtract(timeElapsed);

            Progress = Math.Round(timeElapsed.TotalSeconds / UpdateInterval.TotalSeconds * 100);

            ProgressReport = managerTimeRemaining.ToString("%h") + " hr "
                             + managerTimeRemaining.ToString("%m") + " min " 
                             + managerTimeRemaining.ToString("%s") 
                             + " sec before next update";
        }

        private void UpdateWall(LibraryViewModel library)
        {
            // Only do wallpaper management if there are wallpapers in the library
            if (library.LibraryList.Count > 0)
            {
                // Get current daylight value
                double currentDaylight = UpdateCurrentDaylightSettings();
                CurrentDaylightPercent = Math.Round(currentDaylight * 100);
                // Find (enabled) image with brightness value closest to current daylight value
                WBImage closestImage =
                    library.LibraryList.Aggregate((x, y) =>
                                                           Math.Abs(x.AverageBrightness - currentDaylight)
                                                            < Math.Abs(y.AverageBrightness - currentDaylight)
                                                           && x.IsEnabled
                                                           ? x : y);
                // Set wallpaper to this image
                SetWall(closestImage);

                // Update current closest's thumb, background color, and front-end brightness value
                ClosestImageThumb = closestImage.Thumbnail;
                ClosestImageBack = closestImage.BackgroundColor;
                ClosestImageBrightnessPercent = Math.Round(closestImage.AverageBrightness * 100);
                ClosestImageFilename = Path.GetFileName(closestImage.Path);
            }

            // Restart the timer
            managerStartTime = DateTime.Now;
            managerTimer.Start();
        }

        private double UpdateCurrentDaylightSettings()
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

            return daylightSetting;
        }

        private void SetWall(WBImage image)
        {
            // TODO: add try catch
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, image.Path, SPIF_UPDATEINIFILE);
        }
        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}