using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using ToastNotifications.Utilities;

namespace WallBrite
{
    public class ManagerViewModel : INotifyPropertyChanged
    {
        // DLL Import, method reference, and constants for setting desktop wallpaper
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private int _updateIntervalHours;
        private int _updateIntervalMins;
        private TimeSpan _checkInterval;

        private DispatcherTimer _updateTimer;
        private DispatcherTimer _progressTracker;

        private DateTime _brightestTime;
        private DateTime _darkestTime;

        private DateTime _nextUpdateTime;
        private DateTime _lastUpdateTime;

        private WBImage _currentImage;

        
        private string _wallpaperStyle;

        public DateTime DarkestTime
        {
            get { return _darkestTime; }
            set
            {
                _darkestTime = value;

                // Update current daylight value to reflect change
                DateTime now = DateTime.Now;
                CurrentDaylight = GetDaylightValue(now);

                CheckAndUpdate();

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

                CheckAndUpdate();
                

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
                    _checkInterval = new TimeSpan(UpdateIntervalHours, 1, 0);

                    // Notify change on mins
                    NotifyPropertyChanged("UpdateIntervalMins");
                }
                // Otherwise just set the hours
                else
                {
                    _checkInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);
                }

                // Run check
                CheckAndUpdate();

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
                _checkInterval = new TimeSpan(UpdateIntervalHours, UpdateIntervalMins, 0);

                // Run check
                CheckAndUpdate();

                // Notify change on mins
                NotifyPropertyChanged("UpdateIntervalMins");
            }
        }

        public string WallpaperStyle
        {
            get { return _wallpaperStyle; }
            set { 
                _wallpaperStyle = value;

                // Update current wallpaper to reflect style change (if current wall has been set already)
                if (_currentImage != null)
                    SetWall(_currentImage, _wallpaperStyle);
            }
        }

        public LibraryViewModel Library { get; set; }
        public double CurrentDaylight { get; set; }
        public string ProgressReport { get; set; }
        public double Progress { get; set; }
        public BitmapImage CurrentWallThumb { get; private set; }
        public SolidColorBrush CurrentWallBack { get; private set; }
        public double CurrentWallBrightness { get; set; }
        public string CurrentWallFileName { get; set; }
        public List<string> WallpaperStyles { get; set; }
        public bool StartsOnStartup { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ManualSetCommand { get; set; }
        public ICommand StartupSetCommand { get; set; }

        public ManagerViewModel(LibraryViewModel library)
        {

            CreateCommands();

            // Set assigned library
            Library = library;

            // Set default property values
            // Update interval 30 mins, brightest time 1:00 PM, darkest time 11:00 PM
            _updateIntervalHours = 0;
            _updateIntervalMins = 1;
            _checkInterval = new TimeSpan(_updateIntervalMins, _updateIntervalHours, 0);
            DateTime now = DateTime.Now;
            _brightestTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
            _darkestTime = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
            StartsOnStartup = false;
            WallpaperStyle = "Fill";

            WallpaperStyles = new List<string>(){"Tiled",
                                                "Centered",
                                                "Stretched",
                                                "Fill",
                                                "Fit"};


            CreateTimers();
            CheckAndUpdate();
        }

        public ManagerViewModel(LibraryViewModel library, ManagerSettings settings)
        {
            CreateCommands();

            Library = library;

            // Pull settings from the settings object
            _updateIntervalHours = settings.UpdateIntervalHours;
            _updateIntervalMins = settings.UpdateIntervalMins;
            _checkInterval = new TimeSpan(_updateIntervalHours, _updateIntervalMins, 0);
            _brightestTime = settings.BrightestTime;
            _darkestTime = settings.DarkestTime;
            StartsOnStartup = settings.StartsOnStartup;
            WallpaperStyle = settings.WallpaperStyle;

            WallpaperStyles = new List<string>(){"Tiled",
                                                "Centered",
                                                "Stretched",
                                                "Fill",
                                                "Fit"};

            CreateTimers();
            CheckAndUpdate();
        }


        private void CreateCommands()
        {
            // Create commands
            ManualSetCommand = new RelayCommand(Set);
            StartupSetCommand = new RelayCommand((object s) => SetStartupKey());
        }

        private void CreateTimers()
        {
            // Create timer thread for updating walls
            _updateTimer = new DispatcherTimer { Interval = _checkInterval };
            _updateTimer.Tick += (object s, EventArgs a) => CheckAndUpdate();

            // Create timer that keeps track of time remaining on this ^^^ timer (checks every 1/2 second)
            _progressTracker = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
            _progressTracker.Tick += (object s, EventArgs a) => UpdateTimerProgress();

            // Set start time as now and start the timers
            _lastUpdateTime = DateTime.Now;
            _updateTimer.Start();
            _progressTracker.Start();
        }
        public void ResetTimers()
        {
            _updateTimer.Stop();
            _progressTracker.Stop();
            _nextUpdateTime = DateTime.MinValue;
            _lastUpdateTime = DateTime.MinValue;
            Progress = 0;
            _updateTimer.Start();
            _progressTracker.Start();
        }

        private void SetStartupKey()
        {
            // Add or remove the registry key to start on startup depending on new flag setting
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (StartsOnStartup)
                rk.SetValue("WallBrite", System.Reflection.Assembly.GetExecutingAssembly().Location + " -minimized");
            else
                rk.DeleteValue("WallBrite", false);
        }

        public void SaveSettings()
        {
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Create folder for storing settings (if folder doesn't exist already)
            if (!Directory.Exists(wallBriteAppDataDirectory))
            {
                Directory.CreateDirectory(wallBriteAppDataDirectory);
            }

            // Create settings object to be serialized
            ManagerSettings settings = new ManagerSettings
            {
                UpdateIntervalHours = _updateIntervalHours,
                UpdateIntervalMins = _updateIntervalMins,
                BrightestTime = _brightestTime,
                DarkestTime = _darkestTime,
                StartsOnStartup = StartsOnStartup,
                WallpaperStyle = WallpaperStyle
            };

            // Create settings file in directory and serialize settings object to it
            File.WriteAllText(wallBriteAppDataDirectory + "\\Settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        private void UpdateTimerProgress()
        {
            if (_nextUpdateTime != DateTime.MinValue)
            {
                DateTime now = DateTime.Now;

                // Find time elapsed since last update
                TimeSpan timeElapsed = now - _lastUpdateTime;

                // Get total length of time between last update and next update
                TimeSpan totalTime = _nextUpdateTime - _lastUpdateTime;

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

        public bool CheckAndUpdate()
        {
            bool changed = false;

            // Check for change in wall
            WBImage image = CheckforChange();

            // If there is change, update the wall to the new one
            if (image != null)
            {
                SetWall(image, WallpaperStyle);
                changed = true;
            }


            // Find the next time when wallpaper will actually change
            _nextUpdateTime = FindNextUpdateTime();

            // Restart the timer if a valid next update time was found
            if (_nextUpdateTime != new DateTime())
            {
                _updateTimer.Interval = _nextUpdateTime.Subtract(DateTime.Now);
                _updateTimer.Start();
            }
            // Otherwise set the timers to blank
            else
            {
                ResetTimers();
            }

            return changed;
        }

        /// <summary>
        /// Checks whether there has been enough of a change in daylight setting to warrant a wallpaper change, and if there has been,
        /// returns the new wallpaper image
        /// </summary>
        /// <returns>
        /// WBImage: the new wallpaper, if there has been enough change in daylight setting to warrant a wallpaper change
        /// null: if there has not been enough change in daylight setting to warrant wallpaper change;
        ///         or there are no images in library
        /// </returns>
        private WBImage CheckforChange()
        {
            // Only update the wall if there is at least one image in library to work with; otherwise return null
            if (Library.LibraryList.Count > 0)
            {
                // First check for (and remove) any missing images
                Library.CheckMissing();

                DateTime now = DateTime.Now;

                // Find closest image to current time's daylight value
                CurrentDaylight = GetDaylightValue(now);
                WBImage checkImage = FindClosestImage(now);

                // If image is different from current, return it
                if (checkImage != _currentImage)
                {
                    return checkImage;
                }
                else return null;
            }
            // If no images in library, just return null
            return null;
        }

        private DateTime FindNextUpdateTime()
        {
            // Only do checks if there's at least 2 images in library; otherwise wallpaper will not change
            if (Library.LibraryList.Count > 1)
            {
                // Set last update time to now if it has not been set yet
                if (_lastUpdateTime == new DateTime())
                    _lastUpdateTime = DateTime.Now;

                // Get first check time
                DateTime nextCheckTime = _lastUpdateTime.AddMinutes(_checkInterval.TotalMinutes);

                // Continue looping until intervals have looped around back to the same time on the next day
                // I.e. the check time has the same date as the timer's start time, or the checktime has looped
                // over to the next day but is still before the timer's start time on that day
                // I.e. exits when check time passes the timer's start time but on the following day
                while (nextCheckTime.Date == _lastUpdateTime.Date
                        || (nextCheckTime.Date == _lastUpdateTime.AddDays(1).Date
                              && nextCheckTime.TimeOfDay.CompareTo(_lastUpdateTime.TimeOfDay) <= 0))
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
                        nextCheckTime = nextCheckTime.AddMinutes(_checkInterval.TotalMinutes);
                    }
                }

                // If looped all the way around with no update, then there won't ever be any actual wall change
                // with the current library/automation settings
                // In this case, update progress report to reflect this and return a basic DateTime
                ProgressReport = "Wallpaper will not change with current settings";
                return new DateTime();
            } 
            // If < 2 images in library, wallpaper will not change
            else
            {
                ProgressReport = "Wallpaper will not change with current settings";
                return new DateTime();
            }
        }

        private WBImage FindClosestImage(DateTime time)
        {
            // Only search for closest if there are images in the library
            if (Library.LibraryList.Count > 0)
            {
                // Get current daylight value
                double daylightAtTime = GetDaylightValue(time);
                // Find (enabled) image with brightness value closest to current daylight value
                WBImage closestImage =
                    Library.LibraryList.Aggregate((x, y) =>
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
                //TODO: add try-catch for this somewhere
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
            // In this case, brightest time of day is equal to darkest; just set daylight % to 0
            else
            {
                return 0;
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

        private void Set(object collection)
        {
            // Cast selected image and set it as wall
            System.Collections.IList items = (System.Collections.IList)collection;
            WBImage selectedImage = (WBImage)items[0];
            SetWall(selectedImage, WallpaperStyle);
        }

        private void SetWall(WBImage image, string style)
        {
            // Taken partly from https://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net,
            // partly from https://stackoverflow.com/questions/19989906/how-to-set-wallpaper-style-fill-stretch-according-to-windows-version

            // Open registry key used to set wallpaper style
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            // Set appropriate values on key depending on wallpaper style choice
            if (style == "Tiled")
            {
                key.SetValue(@"WallpaperStyle", "0");
                key.SetValue(@"TileWallpaper", "1");
            }
            else if (style == "Centered")
            {
                key.SetValue(@"WallpaperStyle", "0");
                key.SetValue(@"TileWallpaper", "0");
            }
            else if (style == "Stretched")
            {
                key.SetValue(@"WallpaperStyle", "2");
                key.SetValue(@"TileWallpaper", "0");
            }
            else if (style == "Fit")
            {
                key.SetValue(@"WallpaperStyle", "6");
                key.SetValue(@"TileWallpaper", "0");
            }
            else if (style == "Fill")
            {
                key.SetValue(@"WallpaperStyle", "10");
                key.SetValue(@"TileWallpaper", "0");
            }

            // Set the wallpaper via system parameter
            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                image.Path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            // Update current closest's thumb, background color, and front-end brightness value
            CurrentWallThumb = image.Thumbnail;
            CurrentWallBack = image.BackgroundColor;
            CurrentWallBrightness = image.AverageBrightness;
            CurrentWallFileName = Path.GetFileName(image.Path);
            _currentImage = image;

            _lastUpdateTime = DateTime.Now;
        }

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}