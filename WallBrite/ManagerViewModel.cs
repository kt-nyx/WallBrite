using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private readonly DispatcherTimer checkTimer;
        private readonly DispatcherTimer timerTracker;
        private DateTime checkTimerStart;

        private DateTime _brightestTime;
        private DateTime _darkestTime;

        private DateTime NextUpdateTime;

        private WBImage _currentImage;

        private readonly LibraryViewModel library;

        public DateTime DarkestTime
        {
            get { return _darkestTime; }
            set
            {
                _darkestTime = value;

                // Update current daylight value to reflect change
                DateTime now = DateTime.Now;
                CurrentDaylight = GetDaylightValue(now);

                // Only update next update time if there are at least two wallpapers
                if (library.LibraryList.Count > 1)
                {
                    // Update the update time (lol)
                    NextUpdateTime = FindNextUpdateTime();
                }

                // Notify change on brightest time
                NotifyPropertyChanged("DarkestTime");
            }
        }

        public DateTime BrightestTime
        {
            get { return _brightestTime; }
            set
            {
                _brightestTime = value;

                // Update current daylight value to reflect change
                DateTime now = DateTime.Now;
                CurrentDaylight = GetDaylightValue(now);

                // Only update next update time if there are at least two wallpapers
                if (library.LibraryList.Count > 1)
                {
                    // Update the update time (lol)
                    NextUpdateTime = FindNextUpdateTime();
                }

                // Notify change on brightest time
                NotifyPropertyChanged("BrightestTime");
            }
        }

        public int UpdateIntervalHours
        {
            get { return _updateIntervalHours; }
            set
            {
                _updateIntervalHours = value;

                // Prevent user from setting interval to 0 hours if already at 0 mins
                if (_updateIntervalMins == 0)
                {
                    // Manually set mins to 1
                    _updateIntervalMins = 1;

                    // Create new update interval with new hours and 1 min
                    UpdateInterval = new TimeSpan(UpdateIntervalHours, 1, 0);

                    // Notify change on mins
                    NotifyPropertyChanged("UpdateIntervalMins");
                }
                // Otherwise just set the hours
                else
                {
                    UpdateInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);
                }

                // Only update next update time if there are at least two wallpapers
                if (library.LibraryList.Count > 1)
                {
                    // Update the update time (lol)
                    NextUpdateTime = FindNextUpdateTime();
                }

                // Notify change on hours
                NotifyPropertyChanged("UpdateIntervalHours");
            }
        }

        public int UpdateIntervalMins
        {
            get { return _updateIntervalMins; }
            set
            {
                _updateIntervalMins = value;

                // Create new interval with previously set hours and new mins
                UpdateInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);

                // Only update next update time if there are at least two wallpapers
                if (library.LibraryList.Count > 1)
                {
                    // Update the update time (lol)
                    NextUpdateTime = FindNextUpdateTime();
                }

                // Notify change on mins
                NotifyPropertyChanged("UpdateIntervalMins");
            }
        }

        public double CurrentDaylight { get; set; }

        public string ProgressReport { get; private set; }

        public double Progress { get; set; }

        public BitmapImage CurrentWallThumb { get; private set; }

        public SolidColorBrush CurrentWallBack { get; private set; }

        public double CurrentWallBrightness { get; set; }

        public string CurrentWallFileName { get; set; }

        public ManagerViewModel(LibraryViewModel library)
        {
            // Create command(s)
            UpdateCommand = new RelayCommand((object s) => CheckAndSetCurrentWall());

            // Set assigned library
            this.library = library;

            // Set default property values
            // Update interval 30 mins, brightest time 1:00 PM, darkest time 11:00 PM
            UpdateIntervalHours = 0;
            UpdateIntervalMins = 1;
            DateTime now = DateTime.Now;
            BrightestTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
            DarkestTime = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);

            // Create timer thread for updating walls
            checkTimer = new DispatcherTimer { Interval = UpdateInterval };
            checkTimer.Tick += (object s, EventArgs a) => CheckAndSetCurrentWall();

            // Create timer that keeps track of time remaining on this ^^^ timer (checks every 1/2 second)
            timerTracker = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            timerTracker.Tick += (object s, EventArgs a) => UpdateTimerProgress();

            // Set start time as now and start the timers
            checkTimerStart = DateTime.Now;
            checkTimer.Start();
            timerTracker.Start();
        }

        private void UpdateTimerProgress()
        {
            if (NextUpdateTime != DateTime.MinValue)
            {
                DateTime now = DateTime.Now;

                // Find time elapsed since timer started
                TimeSpan timeElapsed = now - checkTimerStart;

                // Get total length of time between timer's start and next actual wall update
                TimeSpan totalTime = NextUpdateTime - checkTimerStart;

                // Set time remaining to time of next update minus the time already elapsed
                TimeSpan timeRemaining = totalTime - timeElapsed;

                // Get time elapsed as a percentage of total time before next update
                Progress = timeElapsed.TotalSeconds / totalTime.TotalSeconds;

                // Create string to be used in front-end progress bar
                ProgressReport = timeRemaining.ToString("%h") + " hr "
                                 + timeRemaining.ToString("%m") + " min "
                                 + timeRemaining.ToString("%s")
                                 + " sec before next wallpaper change";
            }
        }

        private void CheckAndSetCurrentWall()
        {
            DateTime now = DateTime.Now;

            // Only update the wall if there is at least one image in libraryto work with
            if (library.LibraryList.Count > 0)
            {
                // Find closest image to current time's daylight value
                CurrentDaylight = GetDaylightValue(now);
                WBImage closestImage = FindClosestImage(now);
                if (closestImage != null && closestImage != _currentImage)
                {
                    // Set wallpaper to this image
                    SetWall(closestImage);

                    // Update current closest's thumb, background color, and front-end brightness value
                    CurrentWallThumb = closestImage.Thumbnail;
                    CurrentWallBack = closestImage.BackgroundColor;
                    CurrentWallBrightness = closestImage.AverageBrightness;
                    CurrentWallFileName = Path.GetFileName(closestImage.Path);
                    _currentImage = closestImage;

                    // Only change the next (actual) update time if there are at least two images in lib
                    // i.e. only when there is a possible other wallpaper to switch to at update time
                    if (library.LibraryList.Count > 1)
                    {
                        // Find the next time when wallpaper will actually change
                        NextUpdateTime = FindNextUpdateTime();
                    }
                }

                // Restart the timer
                checkTimerStart = DateTime.Now;
                checkTimer.Start();
            }
        }

        private DateTime FindNextUpdateTime()
        {
            DateTime nextCheckTime = checkTimerStart.AddMinutes(UpdateInterval.TotalMinutes);

            // Continue looping until intervals have looped around back to the same time on the next day
            // I.e. the check time has the same date as the timer's start time, or the checktime has looped
            // over to the next day but is still before the timer's start time on that day
            // I.e. exits when check time passes the timer's start time but on the following day
            while (nextCheckTime.Date == checkTimerStart.Date
                    || (nextCheckTime.Date == checkTimerStart.AddDays(1).Date
                          && nextCheckTime.TimeOfDay.CompareTo(checkTimerStart.TimeOfDay) <= 0))
            {
                // Next image that will be set as wall when brightness is checked

                WBImage nextCheckImage = FindClosestImage(nextCheckTime);

                // Return if the image will actually be different at this check time (i.e. it will actually
                // update to a different wall when checked)
                if (_currentImage != nextCheckImage)
                {
                    return nextCheckTime;
                }
                // Otherwise start the loop again, checking at the time after the next interval
                else
                {
                    nextCheckTime = nextCheckTime.AddMinutes(UpdateInterval.TotalMinutes);
                }
            }

            // If looped all the way around with no update, then there won't ever be any actual wall change
            // with the current library/automation settings
            // Occurs when only one image in library, for example
            throw new InvalidOperationException("Can't find next update time; the wallpaper will not update" +
                "with current library/automation settings");
        }

        private WBImage FindClosestImage(DateTime time)
        {
            // Only search for closest if there are images in the library
            if (library.LibraryList.Count > 0)
            {
                // Get current daylight value
                double daylightAtTime = GetDaylightValue(time);
                // Find (enabled) image with brightness value closest to current daylight value
                WBImage closestImage =
                    library.LibraryList.Aggregate((x, y) =>
                                                           (Math.Abs(x.AverageBrightness - daylightAtTime)
                                                            < Math.Abs(y.AverageBrightness - daylightAtTime)
                                                              || !y.IsEnabled)
                                                           && x.IsEnabled
                                                           ? x : y);
                return closestImage;
            }
            // Otherwise throw an exception
            else
            {
                throw new InvalidOperationException("Can't find closest image in library at given time; " +
                                                    "No images in library");
            }
        }

        private double GetDaylightValue(DateTime time)
        {
            // Get the current time in minutes since midnight
            double now = time.TimeOfDay.TotalMinutes;

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
            bool approachingDarkest;

            // Case 1: Now is between brightest time and darkest time (respectively)
            if (brightestInMins < now && now < darkestInMins)
            {
                // Get total minutes covered since brightest time
                minutesCovered = now - brightestInMins;
                // Get total minutes between brightest and darkest times
                brightestToDarkest = darkestInMins - brightestInMins;

                // Since approaching the darkest time, brightness value will be inverted (should be
                // approaching 0 rather than 1
                approachingDarkest = true;
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
                approachingDarkest = false;
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

                approachingDarkest = false;
            }
            // Case 4: Now is before darkest time and darkest time is before brightest time
            else if (now < darkestInMins && darkestInMins < brightestInMins)
            {
                minutesCovered = (minutesInDay - brightestInMins) + now;

                brightestToDarkest = (minutesInDay - brightestInMins) + darkestInMins;

                approachingDarkest = true;
            }
            // Case 5: Brightest time is before darkest time and darkest time is before now
            else if (brightestInMins < darkestInMins && darkestInMins < now)
            {
                minutesCovered = now - darkestInMins;

                brightestToDarkest = (minutesInDay - darkestInMins) + brightestInMins;

                approachingDarkest = false;
            }
            // Case 6: Darkest time is before brightest time and brightest time is before now
            else if (darkestInMins < brightestInMins && brightestInMins < now)
            {
                minutesCovered = now - brightestInMins;

                brightestToDarkest = (minutesInDay - brightestInMins) + darkestInMins;

                approachingDarkest = true;
            }
            else
            {
                throw new InvalidOperationException("The darkest time, brightest time, or current time is invalid");
            }

            // Get percent of time between brightest and darkest times which has already been covered
            double percentCovered = minutesCovered / brightestToDarkest;

            double daylightSetting;
            // If brightness setting should be inverted relative to the percent covered, do so
            if (approachingDarkest)
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