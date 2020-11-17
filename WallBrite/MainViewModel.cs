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
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace WallBrite
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public LibraryViewModel Library { get; set; }
        public ManagerViewModel Manager { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand OpenWindowCommand { get; set; }

        private readonly MainWindow _window;
        private Notifier _notifier;

        public MainViewModel(MainWindow mainWindow)
        {
            Library = new LibraryViewModel();
            Manager = new ManagerViewModel(Library);
            _window = mainWindow;

            CreateCommands();
            CreateNotifier();
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
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 25,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.DisplayOptions.TopMost = true;

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        public void OpenLastLibrary()
        {
            string lastLibraryDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite\\lastLibrary";

            // Create folder for storing last library used (if folder doesn't exist already)
            if (!Directory.Exists(lastLibraryDirectory))
            {
                Directory.CreateDirectory(lastLibraryDirectory);
            }
            // Check if there is library file in the lastLib directory
            if (File.Exists(lastLibraryDirectory + "\\lastLibrary.json"))
            {
                // If so, then open that library file
                try
                {
                    using (FileStream fileStream = File.OpenRead(lastLibraryDirectory + "\\lastLibrary.json"))
                        OpenLibrary(fileStream);

                    _notifier.ShowInformation("Successfully loaded last used WallBrite library");
                } catch
                {
                    _notifier.ShowError("Failed to load last used WallBrite library");
                }

                
            }
        }


        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenWindow()
        {
            _window.Show();
        }


        private void NewLibrary()
        {
            Library = new LibraryViewModel();
            Manager.Library = Library;
            Manager.ResetTimers();
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
                OpenLibrary(fileStream);
            }
        }

        private void OpenLibrary(Stream fileStream)
        {
            var serializer = new JsonSerializer();

            // Use json serializer to read library file and create a new libary
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                // Deserialize WBImage array from file
                WBImage[] imageArray = (WBImage[])serializer.Deserialize(jsonTextReader, typeof(WBImage[]));

                // Create new library VM using image array
                Library = new LibraryViewModel(imageArray);

                // Update manager to use new library
                Manager.Library = Library;

                // Check for missing files in opened library
                Library.CheckMissing();
            }
            Manager.ResetTimers();
        }
    }
}
