using JR.Utils.GUI.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using WinForms = System.Windows.Forms;

namespace WallBrite
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WBImage> LibraryList { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand EnableCommand { get; set; }
        public ICommand DisableCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand AddFilesCommand { get; set; }
        public ICommand AddFolderCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public double AddProgress { get; set; }
        public string AddProgressReport { get; set; }

        private AddFileProgressViewModel _addProgressViewModel;
        private readonly Notifier _notifier;
        private BackgroundWorker _worker;

        private ManagerViewModel _manager;

        public LibraryViewModel(ManagerViewModel manager, Notifier notifier)
        {
            // Create new empty library list
            LibraryList = new ObservableCollection<WBImage>();

            _manager = manager;

            _notifier = notifier;

            // Create commands
            CreateCommands();
        }

        public LibraryViewModel(WBImage[] libraryList, ManagerViewModel manager, Notifier notifier)
        {
            // Create library list from given
            LibraryList = new ObservableCollection<WBImage>(libraryList);

            _manager = manager;

            _notifier = notifier;

            // Create commands
            CreateCommands();
        }

        private void CreateCommands()
        {
            EnableCommand = new RelayCommand(Enable);
            DisableCommand = new RelayCommand(Disable);
            RemoveCommand = new RelayCommand(Remove);
            AddFilesCommand = new RelayCommand((object s) => AddFiles());
            AddFolderCommand = new RelayCommand((object s) => AddFolder());
            SaveCommand = new RelayCommand((object s) => SaveLibrary());
            CancelCommand = new RelayCommand((object s) => CancelAdd());
        }

        public bool CheckMissing()
        {
            List<string> missingImages = new List<string>();
            // Loop over each image in library
            foreach (WBImage image in LibraryList.ToList())
            {
                // Check if image does not exist at WBImage's path (or no permissions to that file)
                if (!File.Exists(image.Path))
                {
                    // Add this image's file name to the list of missing images
                    missingImages.Add(Path.GetFileName(image.Path));

                    // Remove this WBImage from the library
                    LibraryList.Remove(image);
                }
            }

            // If missing images found, show message box explaining this to user
            if (missingImages.Count > 0)
            {
                string message = "The following image files are missing and were removed from the library:\n\n";
                foreach (string filename in missingImages)
                {
                    message += filename + "\n";
                }
                message += "\nThese files may have been moved or deleted, or WallBrite may not have permission to access them. If you're sure " +
                    "the files are still where they were, try running WallBrite as administrator to make sure it has permissions to access them.";

                FlexibleMessageBox.Show(message, "Missing Files", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                return true;
            }

            return false;
        }

        public void UpdateManager(ManagerViewModel manager)
        {
            _manager = manager;
        }

        public void SaveLastLibrary()
        {
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Create folder for storing last library used (if folder doesn't exist already)
            if (!Directory.Exists(wallBriteAppDataDirectory))
            {
                Directory.CreateDirectory(wallBriteAppDataDirectory);
            }

            // Create library file in directory and save current library to it
            File.WriteAllText(wallBriteAppDataDirectory + "\\LastLibrary.json", JsonConvert.SerializeObject(LibraryList, Formatting.Indented));
        }

        // TODO: change placeholder path to a relative? path
        private void SaveLibrary()
        {
            // Create file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "WallBrite Library File (*.json)|*.json"
            };

            // Open file dialof and only save if 'Save' is selected on the dialog
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(LibraryList, Formatting.Indented));
            }

            // Save this as last library as well
            SaveLastLibrary();
        }

        /// <summary>
        /// Adds given WBImage at given filePath to the library; or throws exception if image is
        /// already in the library
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        public void AddImage(WBImage image)
        {
            // Give user message and don't add this image to library if it is already in library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(image.Path)))
            {
                _notifier.ShowInformation(string.Format("{0} was not added since it is already in the library",
                                                         Path.GetFileName(image.Path)));
                return;
            }

            // Add the image to the library and return true (if it's not already in the library)
            LibraryList.Add(image);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collection"></param>
        private void Remove(object collection)
        {
            // Cast the collection of controls to a collection of WBImages
            System.Collections.IList items = (System.Collections.IList)collection;
            var selectedImages = items.Cast<WBImage>();

            // Remove each WBImage in the collection from the library
            foreach (WBImage image in selectedImages.ToList())
            {
                LibraryList.Remove(image);
            }

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            _manager.CheckAndUpdate();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collection"></param>
        private void Enable(object collection)
        {
            // Cast the collection of controls to a collection of WBImages
            System.Collections.IList items = (System.Collections.IList)collection;
            var selectedImages = items.Cast<WBImage>();

            // Enable each WBImage in the collection
            foreach (WBImage image in selectedImages)
            {
                image.IsEnabled = true;
            }

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            _manager.CheckAndUpdate();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collection"></param>
        private void Disable(object collection)
        {
            // Cast the collection of controls to a collection of WBImages
            System.Collections.IList items = (System.Collections.IList)collection;
            var selectedImages = items.Cast<WBImage>();

            // Disable each WBImage in the collection
            foreach (WBImage image in selectedImages)
            {
                image.IsEnabled = false;
            }

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            _manager.CheckAndUpdate();
        }

        private void CancelAdd()
        {
            _worker.CancelAsync();

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            _manager.CheckAndUpdate();
        }

        /// <summary>
        ///
        /// </summary>
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
                // Create viewmodel (i.e. window) for the progress bar
                _addProgressViewModel = new AddFileProgressViewModel(this);

                // Create background worker that will add the files
                _worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                _worker.DoWork += AddFilesWork;
                _worker.ProgressChanged += UpdateAddProgress;
                _worker.RunWorkerCompleted += AddComplete;

                // Run worker
                _worker.RunWorkerAsync(dialog);
            }
        }

        private void AddFilesWork(object sender, DoWorkEventArgs e)
        {
            // TODO: add try catches for possible exceptions

            OpenFileDialog dialog = (OpenFileDialog)e.Argument;

            // Create streams from selected files
            List<Stream> fileStreams = dialog.OpenFiles().ToList();
            List<string> filePaths = new List<string>(dialog.FileNames);

            // Do actual adding in generalized function
            AddWork(sender, fileStreams, filePaths);
        }

        private void AddWork(object sender, List<Stream> fileStreams, List<string> filePaths)
        {
            // For keeping track of progress
            int numFiles = filePaths.Count;
            int progress;

            // Create the WBImage object for each image file and add it to library
            for (int i = 0; i < fileStreams.Count; i++)
            {
                // If user cancelled then exit loop and return before adding next file
                if (_worker.CancellationPending == true) return;

                // Get stream, path, and filename for current file
                Stream stream = fileStreams[i];
                string filePath = filePaths[i];
                string fileName = Path.GetFileName(filePath);

                string progressString = string.Format("{0}|{1}|{2}",
                                                       fileName,
                                                       i + 1,
                                                       numFiles);

                // Get current progress as percentage of total files
                progress = Convert.ToInt32((double)i / numFiles * 100);

                // Report progress and current file being worked on
                (sender as BackgroundWorker).ReportProgress(progress, progressString);

                // Creates the bitmap in the scope of the background worker (this is the performance intensive
                // task for this whole process)
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    // Create the WBImage for this file and add it to the results list (in the main thread
                    // so that it doesn't get upset that UI-relevant objects were created in the background thread)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WBImage image = new WBImage(bitmap, filePath);
                        AddImage(image);
                    });
                }
            }
        }

        private void UpdateAddProgress(object sender, ProgressChangedEventArgs e)
        {
            // Update front-end progress percentage
            AddProgress = e.ProgressPercentage;

            // Parse arguments from UserState
            string[] args = (e.UserState as string).Split('|');
            string currentFile = args[0];
            string fileNumber = args[1];
            string numFiles = args[2];

            // Update front-end progress string
            AddProgressReport = string.Format("Adding file {0} of {1}: {2}",
                                                fileNumber,
                                                numFiles,
                                                currentFile);
        }

        private void AddComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            _addProgressViewModel.CloseWindow();

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            _manager.CheckAndUpdate();
        }

        /// <summary>
        ///
        /// </summary>
        public void AddFolder()
        {
            // TODO: add errors for files already existing in library (addfile returns false in this case)
            // Create FolderBrowserDialog to browse folders
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();

            // If user clicked OK (not Cancel) in folder dialog
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                // Create viewmodel (i.e. window) for the progress bar
                _addProgressViewModel = new AddFileProgressViewModel(this);

                // Create background worker that will add the files
                _worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                _worker.DoWork += AddFolderWork;
                _worker.ProgressChanged += UpdateAddProgress;
                _worker.RunWorkerCompleted += AddComplete;

                // Run worker
                _worker.RunWorkerAsync(dialog);
            }
        }

        private void AddFolderWork(object sender, DoWorkEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = (WinForms.FolderBrowserDialog)e.Argument;
            // Get path string from selected path
            string folderPath = dialog.SelectedPath;

            // TODO: add try catch for possible exceptions of getfiles; pathtoolong??
            // Get paths of all supported image files in this folder and subfolders
            List<string> filePaths = Directory.EnumerateFiles(folderPath,
                                                "*.*",
                                                SearchOption.AllDirectories)
                                 .Where(filePath => filePath.EndsWith(".jpg")
                                                    || filePath.EndsWith(".jpeg")
                                                    || filePath.EndsWith(".png")
                                                    || filePath.EndsWith(".gif")
                                                    || filePath.EndsWith(".bmp")
                                                    || filePath.EndsWith(".exif")
                                                    || filePath.EndsWith(".tiff"))
                                 .ToList();

            List<Stream> fileStreams = new List<Stream>();
            // Loop over each filePath in the array of filePaths (over each file in
            // selected folder/subfolders)
            foreach (string filePath in filePaths)
            {
                // TODO: add try catch for possible exceptions
                // Open file stream for file at this path
                FileStream file = File.OpenRead(filePath);
                fileStreams.Add(file);
            }

            AddWork(sender, fileStreams, filePaths);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="imageGrid"></param>
        public void SortTypeChanged(object sender, ListView imageGrid)
        {
            ComboBox box = (ComboBox)sender;

            // Only do sort work if imageGrid already exists
            if (imageGrid != null)
            {
                // Get selected sort type
                string selected = box.SelectedValue.ToString();

                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(imageGrid.ItemsSource);

                // Get current direction of sort to be used (if there is one); default to ascending if none
                ListSortDirection direction;
                if (view.SortDescriptions.Count > 0)
                {
                    direction = view.SortDescriptions[0].Direction;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }

                // Clear current sort
                view.SortDescriptions.Clear();

                // Set appropriate sort
                if (selected.Equals("Brightness"))
                {
                    view.SortDescriptions.Add(new SortDescription("AverageBrightness", direction));
                }
                else if (selected.Equals("Date Added"))
                {
                    view.SortDescriptions.Add(new SortDescription("AddedDate", direction));
                }
                else if (selected.Equals("Enabled"))
                {
                    view.SortDescriptions.Add(new SortDescription("IsEnabled", direction));
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="imageGrid"></param>
        public void SortDirectionChanged(object sender, ListView imageGrid)
        {
            ComboBox box = (ComboBox)sender;
            // Only do sort work if imageGrid already exists
            if (imageGrid != null)
            {
                // Get selected sort type
                string selected = box.SelectedValue.ToString();

                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(imageGrid.ItemsSource);

                // Get current direction of sort to be used (if there is one); default to ascending if none
                string currentSort;
                if (view.SortDescriptions.Count > 0)
                {
                    currentSort = view.SortDescriptions[0].PropertyName;
                }
                else
                {
                    currentSort = "DateAdded";
                }

                // Clear current sort
                view.SortDescriptions.Clear();

                // Set appropriate sort
                if (selected.Equals("Descending"))
                {
                    view.SortDescriptions.Add(new SortDescription(currentSort, ListSortDirection.Descending));
                }
                else if (selected.Equals("Ascending"))
                {
                    view.SortDescriptions.Add(new SortDescription(currentSort, ListSortDirection.Ascending));
                }
            }
        }
    }
}