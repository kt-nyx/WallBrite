using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace WallBrite
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private readonly MainWindow _window;
        private Notifier _notifier;
        private readonly ListView _imageGrid;

        public LibraryViewModel Library { get; set; }
        public ManagerViewModel Manager { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand OpenCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand OpenWindowCommand { get; set; }
        


        public MainViewModel(MainWindow mainWindow, bool startingMinimized, ListView imageGrid)
        {
            CreateNotifier();

            if (!startingMinimized)
                mainWindow.Show();

            _imageGrid = imageGrid;

            // Try to pull library from last lib file
            LibraryViewModel libraryFromFile = GetLibraryFromLastLibFile(startingMinimized);

            // If successful, use the library from file
            if (libraryFromFile != null) {
                Library = libraryFromFile;
                Library.ImageGrid = _imageGrid;
            }
            // Otherwise create new empty library
            else
            {
                Library = new LibraryViewModel(Manager, _notifier, _imageGrid);
            }

            // Try to pull new manager from settings file
            ManagerViewModel managerFromFile = GetManagerFromSettingsFile(startingMinimized);

            // If successful, use the manager from file
            if (managerFromFile != null)
                Manager = managerFromFile;
            // If unsuccessful (e.g. no settings file exists), just create new manager with default settings
            else
                Manager = new ManagerViewModel(Library, _notifier);

            // Update after both manager and library created
            Library.UpdateManager(Manager);
            Manager.UpdateLibrary(Library);

            // Check and update before displaying
            Manager.CheckAndUpdate();

            _window = mainWindow;

            CreateCommands();
        }

        private void CreateCommands()
        {
            OpenCommand = new RelayCommand((object s) => OpenLibrary());
            NewCommand = new RelayCommand((object s) => NewLibrary());
            ExitCommand = new RelayCommand((object s) => Exit());
            OpenWindowCommand = new RelayCommand((object s) => OpenWindow());
        }

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

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void OpenWindow()
        {
            _window.Show();
        }

        private void NewLibrary()
        {
            Library = new LibraryViewModel(Manager, _notifier, _imageGrid);
            Manager.UpdateLibrary(Library);
            Manager.ResetTimers();
        }

        public ManagerViewModel GetManagerFromSettingsFile(bool startingMinimized)
        {
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Check if there is settings file in the appdata directory; if not just skip loading settings file
            if (File.Exists(wallBriteAppDataDirectory + "\\Settings.json"))
            {
                // If so, then open that settings file
                try
                {
                    ManagerViewModel newManager;
                    using (FileStream fileStream = File.OpenRead(wallBriteAppDataDirectory + "\\Settings.json"))
                        newManager = new ManagerViewModel(Library, OpenManagerSettingsFile(fileStream), _notifier);

                    // Only show notification if not starting minimized (will crash otherwise since notifier can't tie to any window)
                    if (!startingMinimized)
                        _notifier.ShowInformation("Successfully loaded last used WallBrite settings");

                    return newManager;
                }
                catch (IOException)
                {
                    // Only show notification if not starting minimized (will crash otherwise since notifier can't tie to any window)
                    if (!startingMinimized)
                        _notifier.ShowError("Failed to load last used WallBrite settings: file corrupted or in use by another program");

                    return null;
                }
            }
            return null;
        }

        public LibraryViewModel GetLibraryFromLastLibFile(bool startingMinimized)
        {
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Check if there is library file in the appdata directory; if not just skip loading the last used lib
            if (File.Exists(wallBriteAppDataDirectory + "\\LastLibrary.json"))
            {
                // If so, then open that library file
                try
                {
                    LibraryViewModel newLibrary;
                    using (FileStream fileStream = File.OpenRead(wallBriteAppDataDirectory + "\\LastLibrary.json"))
                        newLibrary = OpenLibraryFromStream(fileStream);

                    // Only show notification if not starting minimized (will crash otherwise since notifier can't tie to any window)
                    if (!startingMinimized)
                        _notifier.ShowInformation("Successfully loaded last used WallBrite library");

                    return newLibrary;
                }
                catch (IOException)
                {
                    // Only show notification if not starting minimized (will crash otherwise since notifier can't tie to any window)
                    if (!startingMinimized)
                        _notifier.ShowError("Failed to load last used WallBrite library: file corrupted or in use by another program");

                    return null;
                }
            }
            return null;
        }

        private void OpenLibrary()
        {
            // TODO: add errors for files already existing in library (addfile returns false in this case)
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
                // TODO: add try catch for possible exceptions
                // Create stream from selected file
                using (Stream fileStream = dialog.OpenFile())
                    // Open library using this stream
                    Library = OpenLibraryFromStream(fileStream);

                Library.ImageGrid = _imageGrid;

                Manager.UpdateLibrary(Library);
                Manager.CheckAndUpdate();
            }
        }

        private LibraryViewModel OpenLibraryFromStream(Stream fileStream)
        {
            var serializer = new JsonSerializer();

            // Use json serializer to read library file and create a new library
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                // Deserialize WBImage array from file
                WBImage[] imageArray = (WBImage[])serializer.Deserialize(jsonTextReader, typeof(WBImage[]));

                // Create new library VM using image array
                LibraryViewModel newLibrary = new LibraryViewModel(imageArray, Manager, _notifier);

                // Check for missing files in opened library
                newLibrary.CheckMissing();

                return newLibrary;
            }
        }

        private ManagerSettings OpenManagerSettingsFile(Stream fileStream)
        {
            var serializer = new JsonSerializer();

            // Use json serializer to read library file and create a new library
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                ManagerSettings settings = (ManagerSettings)serializer.Deserialize(jsonTextReader, typeof(ManagerSettings));
                return settings;
            }
        }
    }
}
