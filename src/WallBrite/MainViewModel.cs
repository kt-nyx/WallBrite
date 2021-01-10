using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace WallBrite
{
    /// <summary>
    /// The main view model; represents the main window and higher-level operations; tracks the currently
    /// used Manager and Library and pulls them from files on startup if possible; also manages toast
    /// notifs via the notifier
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MainWindow _window;
        private readonly ListView _imageGrid;
        private Notifier _notifier;


        public LibraryViewModel Library { get; set; }
        public ManagerViewModel Manager { get; set; }

        // Commands + PropertyChangedEvent
        public ICommand OpenCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand OpenWindowCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Creates new MainViewModel; attempts to pull Library and Manager from file and creates default
        /// ones if either fails
        /// </summary>
        /// <param name="mainWindow">Main display window</param>
        /// <param name="startingMinimized">True when starting minimized (ie from Windows startup), false
        /// otherwise; will only show window after startup if this is set true</param>
        /// <param name="imageGrid">Front-end ListView element used for displaying images on main window</param>
        public MainViewModel(MainWindow mainWindow, bool startingMinimized, ListView imageGrid)
        {
            // Create notifier for sending toast notifs
            CreateNotifier();

            // Only show window if not starting minimized
            if (!startingMinimized)
                mainWindow.Show();

            // Try to pull library from last lib file
            LibraryViewModel libraryFromFile = GetLibraryFromLastLibFile(startingMinimized);

            // If successful, use the library from file
            if (libraryFromFile != null)
            {
                Library = libraryFromFile;
                Library.ImageGrid = imageGrid;
            }
            // Otherwise create new empty library
            else
            {
                Library = new LibraryViewModel(Manager, _notifier, imageGrid);
            }

            // Try to pull new manager from settings file
            ManagerViewModel managerFromFile = GetManagerFromSettingsFile(startingMinimized);

            // If successful, use the manager from file
            if (managerFromFile != null)
                Manager = managerFromFile;
            // If unsuccessful (e.g. no settings file exists), just create new manager with default settings
            else
                Manager = new ManagerViewModel(Library);

            // Link library and manager after creation
            Library.Manager = Manager;
            Manager.Library = Library;

            // Check and update before displaying
            Manager.CheckAndUpdate();

            // Store window and image grid element
            _window = mainWindow;
            _imageGrid = imageGrid;

            CreateCommands();
        }

        /// <summary>
        /// Attempts to pull manager settings from AppData file and returns ManagerViewModel with those
        /// settings if successful
        /// </summary>
        /// <param name="startingMinimized">True if starting app minimized, false otherwise; if true, will
        /// display success/failure toast notifs on loading settings file</param>
        /// <returns>ManagerViewModel with settings from AppData file if successful; null if unsuccessful</returns>
        public ManagerViewModel GetManagerFromSettingsFile(bool startingMinimized)
        {
            // Pull AppData directory (will differ depending on user + drive)
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Check if there is settings file in the AppData directory; if not then skip loading settings file
            if (File.Exists(wallBriteAppDataDirectory + "\\Settings.json"))
            {
                // If file exists, try to open it
                try
                {
                    ManagerViewModel newManager;

                    // Open the file and create newManager using setings from it
                    using (FileStream fileStream = File.OpenRead(wallBriteAppDataDirectory + "\\Settings.json"))
                        newManager = new ManagerViewModel(Library, GetManagerSettingsFromStream(fileStream));

                    // Show success toast notif if not starting minimized
                    if (!startingMinimized)
                        _notifier.ShowInformation("Successfully loaded last used WallBrite settings");

                    return newManager;
                }
                // If file fails to open, send failure toast notif and return null
                catch (IOException)
                {
                    // Show failure toast notif not starting minimized
                    if (!startingMinimized)
                        _notifier.ShowError("Failed to load last used WallBrite settings: file corrupted or in use by another program");

                    return null;
                }
            }

            // If file doesn't exist, return null
            return null;
        }

        /// <summary>
        /// Attempts to pull library from LastLibrary file in AppData folder and returns LibraryViewModel
        /// representing that library if successful
        /// </summary>
        /// <param name="startingMinimized">True if starting app minimized, false otherwise; if true, will
        /// display success/failure toast notifs on loading library file</param>
        /// <returns>LibraryViewModel with library from AppData file if successful; null if not</returns>
        public LibraryViewModel GetLibraryFromLastLibFile(bool startingMinimized)
        {
            // Pull AppData directory (will differ depending on user + drive)
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Check if there is library file in the AppData directory; if not then skip loading the last used lib
            if (File.Exists(wallBriteAppDataDirectory + "\\LastLibrary.json"))
            {
                // If file exits, try to open it
                try
                {
                    LibraryViewModel newLibrary;

                    // Open the file and create newLibrary from it
                    using (FileStream fileStream = File.OpenRead(wallBriteAppDataDirectory + "\\LastLibrary.json"))
                        newLibrary = GetLibraryFromStream(fileStream);

                    // Show success toast notif if not starting minimized
                    if (!startingMinimized)
                        _notifier.ShowInformation("Successfully loaded last used WallBrite library");

                    return newLibrary;
                }
                // If file fails to open, send failure toast notif and return null
                catch (IOException)
                {
                    // Show failure toast notif if not starting minimized
                    if (!startingMinimized)
                        _notifier.ShowError("Failed to load last used WallBrite library: file corrupted or in use by another program");

                    return null;
                }
            }

            // If file doesn't exist, return null
            return null;
        }

        /// <summary>
        /// Creates commands used from front-end buttons
        /// </summary>
        private void CreateCommands()
        {
            OpenCommand = new RelayCommand((object s) => OpenLibrary());
            NewCommand = new RelayCommand((object s) => NewLibrary());
            ExitCommand = new RelayCommand((object s) => Exit());
            OpenWindowCommand = new RelayCommand((object s) => OpenWindow());
        }

        /// <summary>
        /// Creates notifier used for sending toast notifs; in top-right
        /// </summary>
        private void CreateNotifier()
        {
            // Create notifier for use with toast notifications
            _notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: Corner.TopRight,
                    offsetX: 8,
                    offsetY: 23);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(5),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.DisplayOptions.TopMost = true;

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        /// <summary>
        /// Exits application; used from front-end taskbar button command
        /// </summary>
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Opens main window; used from front-end taskbar button command
        /// </summary>
        private void OpenWindow()
        {
            _window.Show();
        }

        /// <summary>
        /// Creates a new empty library and links the current Manager to it
        /// </summary>
        private void NewLibrary()
        {
            Library = new LibraryViewModel(Manager, _notifier, _imageGrid);
            Manager.Library = Library;

            // Reset timers and set progress report (progress bar message) to blank
            Manager.ResetTimers();
            Manager.ProgressReport = null;
        }

        /// <summary>
        /// Attempts to open library from user file selection dialog; used from front-end button command
        /// Will send toast notifs on success or failure
        /// </summary>
        private void OpenLibrary()
        {
            // Create OpenFileDialog to browse files
            OpenFileDialog dialog = new OpenFileDialog
            {
                // Filter dialog to only show supported image types (or all files)
                Filter = "WallBrite Library Files|*.json" +
                            "|All Files|*.*",
            };

            // If user clicked OK (not Cancel) in file dialog
            if (dialog.ShowDialog() == true)
            {
                // Try to open file
                try
                {
                    // Open selected file, create library from the stream, and set Library to it
                    using (Stream fileStream = dialog.OpenFile())
                        Library = GetLibraryFromStream(fileStream);

                    // Show success toast notif
                    _notifier.ShowInformation("Successfully loaded WallBrite library");

                    // Update Library's imageGrid element
                    Library.ImageGrid = _imageGrid;

                    // Link Manager to new Library and check+update to reflect changes
                    Manager.Library = Library;
                    Manager.CheckAndUpdate();
                }
                // If file fails to open, send failure toast notif
                catch (IOException)
                {
                    // Show failure toast notif
                    _notifier.ShowError("Failed to load last used WallBrite library: file corrupted or in use by another program");
                }
            }
        }

        /// <summary>
        /// Returns LibraryViewModel represented by given Stream
        /// </summary>
        /// <param name="fileStream">Stream representing library</param>
        /// <returns>LibraryViewModel represented by given Stream</returns>
        private LibraryViewModel GetLibraryFromStream(Stream fileStream)
        {
            var serializer = new JsonSerializer();

            // Use json serializer to read library file and create a new library
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                // Deserialize WBImage array from file
                WBImage[] imageArray = (WBImage[])serializer.Deserialize(jsonTextReader, typeof(WBImage[]));

                // Create new library VM using image array
                LibraryViewModel newLibrary = new LibraryViewModel(imageArray, Manager, _notifier, _imageGrid);

                // Check for missing files in opened library
                newLibrary.CheckMissing();

                return newLibrary;
            }
        }

        /// <summary>
        /// Returns ManagerSettings represented by given Stream
        /// </summary>
        /// <param name="fileStream">Stream representing settings</param>
        /// <returns>ManagerSerttings represented by given Stream</returns>
        private ManagerSettings GetManagerSettingsFromStream(Stream fileStream)
        {
            var serializer = new JsonSerializer();

            // Use json serializer to read settings file and create new ManagerSettings object
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                ManagerSettings settings = (ManagerSettings)serializer.Deserialize(jsonTextReader, typeof(ManagerSettings));
                return settings;
            }
        }
    }
}