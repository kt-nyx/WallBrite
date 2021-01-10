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
using ToastNotifications.Messages;
using WinForms = System.Windows.Forms;

namespace WallBrite
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Collection of WBImages in this library; most operations/checks are done on this collection
        /// </summary>
        public ObservableCollection<WBImage> LibraryList { get; private set; }

        /// <summary>
        /// Progress during add file operation (0% - 100%); determines progress on front-end progress bar
        /// </summary>
        public double AddProgress { get; set; }

        /// <summary>
        /// Text displaying progress during add file operation; displays inside front-end progress bar
        /// </summary>
        public string AddProgressReport { get; set; }

        /// <summary>
        /// Whether library is empty (i.e. whether LibraryList is empty)
        /// </summary>
        public bool IsEmpty { get; private set; }

        /// <summary>
        /// Front-end ListView where images are displayed
        /// </summary>
        public ListView ImageGrid { get; set; }

        /// <summary>
        /// Sort types (Brightness, Date Added, Enabled) used for ImageGrid on front-end
        /// </summary>
        public List<string> SortTypes { get; set; }

        /// <summary>
        /// Sort directions (Ascending, Descending) used for ImageGrid on front-end
        /// </summary>
        public List<string> SortDirections { get; set; }

        public ManagerViewModel Manager { get; set; }

        /// <summary>
        /// Currently used sort type (Brightness, Date Added, Enabled) used for ImageGrid on front-end
        /// </summary>
        public string SortType
        {
            get { return _sortType; }
            set
            {
                if (_sortType != value)
                {
                    _sortType = value;

                    // Update image grid to reflect sort type change
                    SortTypeChanged(_sortType);
                }
            }
        }

        /// <summary>
        /// Currently used sort direction (Ascending, Descending) used for ImageGrid on front-end
        /// </summary>
        public string SortDirection
        {
            get { return _sortDirection; }
            set
            {
                if (_sortDirection != value)
                {
                    _sortDirection = value;

                    // Update image grid to reflect sort direction change
                    SortDirectionChanged(_sortDirection);
                }
            }
        }

        // Commands + PropertyChangedEvent
        public ICommand EnableCommand { get; set; }

        public ICommand DisableCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand AddFilesCommand { get; set; }
        public ICommand AddFolderCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _sortType;
        private string _sortDirection;

        private readonly Notifier _notifier;
        private AddFileProgressViewModel _addProgressViewModel;
        private BackgroundWorker _worker;

        /// <summary>
        /// Creates new empty LibraryViewModel using given ManagerViewModel, Notifier, and imageGrid element
        /// </summary>
        /// <param name="manager">ManagerViewModel to be linked to this library</param>
        /// <param name="notifier">Notifier used to send toast notifs (from main VM)</param>
        /// <param name="imageGrid">ListView element used to display image grid on front-end</param>
        public LibraryViewModel(ManagerViewModel manager, Notifier notifier, ListView imageGrid)
        {
            // Create new empty library list
            LibraryList = new ObservableCollection<WBImage>();

            // This library will be empty (it's new)
            IsEmpty = true;

            // Link the manager and set the notifier and ImageGrid
            Manager = manager;
            _notifier = notifier;
            ImageGrid = imageGrid;

            // Create SortTypes and SortDirections
            SortTypes = new List<string>(){"Brightness",
                                           "Date Added",
                                           "Enabled"};
            SortDirections = new List<string>(){"Descending",
                                                "Ascending"};

            // Create commands
            CreateCommands();
        }

        /// <summary>
        /// Creates new LibraryViewModel using given libraryList, ManagerViewModel, Notifier, and imageGrid element
        /// </summary>
        /// <param name="libraryList">Array of WBImages to be represented by this library</param>
        /// <param name="manager">ManagerViewModel to be linked to this library</param>
        /// <param name="notifier">otifier used to send toast notifs (from main VM)</param>
        /// /// <param name="imageGrid">ListView element used to display image grid on front-end</param>
        public LibraryViewModel(WBImage[] libraryList, ManagerViewModel manager, Notifier notifier, ListView imageGrid)
        {
            // Create library list from given array
            LibraryList = new ObservableCollection<WBImage>(libraryList);

            // Set empty fla true if this library is empty; otherwise false
            if (LibraryList.Count < 1)
                IsEmpty = true;
            else
            {
                IsEmpty = false;
            }

            // Link the manager and set the notifier and ImageGrid
            Manager = manager;
            _notifier = notifier;
            ImageGrid = imageGrid;

            // Create SortTypes and SortDirections
            SortTypes = new List<string>(){"Brightness",
                                           "Date Added",
                                           "Enabled"};
            SortDirections = new List<string>(){"Descending",
                                                "Ascending"};

            // Set default sort type and direction
            SortType = "Date Added";
            SortDirection = "Ascending";

            // Create commands
            CreateCommands();
        }

        /// <summary>
        /// Checks for any missing images in library (i.e. image exists in library but not on disk); gives
        /// user message box notif and returns true if missing image(s) found; returns false otherwise
        /// </summary>
        /// <returns>True if missing image(s) found, false otherwise</returns>
        public bool CheckMissing()
        {
            List<string> missingImages = new List<string>();

            // Loop over each WBImage in library
            foreach (WBImage image in LibraryList.ToList())
            {
                // Check if image does not exist at WBImage's associated path (or no permissions to that file)
                if (!File.Exists(image.Path))
                {
                    // Add this image's file name to the list of missing images
                    missingImages.Add(Path.GetFileName(image.Path));

                    // Remove this WBImage from the library
                    LibraryList.Remove(image);
                }
            }

            // If missing images found, show message box explaining this to user and return true
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

            // If no missing images found, return false
            return false;
        }

        /// <summary>
        /// Saves last used (currently in used) library to file in AppData folder
        /// </summary>
        public void SaveLastLibrary()
        {
            // Pull AppData directory (will differ depending on user + drive)
            string wallBriteAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallBrite";

            // Create folder in AppData if folder doesn't exist already
            if (!Directory.Exists(wallBriteAppDataDirectory))
            {
                Directory.CreateDirectory(wallBriteAppDataDirectory);
            }

            // Create library file in directory (if it doesn't exist already) and save current library to it
            File.WriteAllText(wallBriteAppDataDirectory + "\\LastLibrary.json", JsonConvert.SerializeObject(LibraryList, Formatting.Indented));
        }

        /// <summary>
        /// Creates commands used from front-end buttons
        /// </summary>
        private void CreateCommands()
        {
            EnableCommand = new RelayCommand(Enable);
            DisableCommand = new RelayCommand(Disable);
            RemoveCommand = new RelayCommand(Remove);
            SaveCommand = new RelayCommand((object s) => SaveLibrary());
            AddFilesCommand = new RelayCommand((object s) => AddFiles());
            AddFolderCommand = new RelayCommand((object s) => AddFolder());
            CancelCommand = new RelayCommand((object s) => CancelAdd());
        }

        /// <summary>
        /// Enable given collection of controls (representing WBImages) in image rotation
        /// </summary>
        /// <param name="collection">Collection of controls representing WBImages to be enabled (object type since coming from command)</param>
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
            Manager.CheckAndUpdate();
        }

        /// <summary>
        /// Enable given collection of controls (representing WBImages) in image rotation
        /// </summary>
        /// <param name="collection">Collection of controls representing WBImages to be enabled (object type since coming from command)</param>
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
            Manager.CheckAndUpdate();
        }

        /// <summary>
        /// Remove given collection of controls (representing WBImages) from library; used with front-end button command
        /// </summary>
        /// <param name="collection">Collection of controls representing WBImages to be removed (object type since coming from command)</param>
        private void Remove(object collection)
        {
            // Cast the collection of controls to a collection of WBImages
            System.Collections.IList items = (System.Collections.IList)collection;
            var selectedImages = items.Cast<WBImage>();

            // Remove each WBImage in the collection from the library list
            foreach (WBImage image in selectedImages.ToList())
            {
                LibraryList.Remove(image);
            }

            // Set empty flag if library is empty after removal
            if (LibraryList.Count < 1)
                IsEmpty = true;

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            Manager.CheckAndUpdate();
        }

        /// <summary>
        /// Saves current library via user save-file dialog; also saves it as last used in AppData file; used
        /// via front-end command button
        /// </summary>
        private void SaveLibrary()
        {
            // Create file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "WallBrite Library File (*.json)|*.json"
            };

            // Open file dialog and only save if 'Save' is clicked
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(LibraryList, Formatting.Indented));
            }

            // Save this as last library as well
            SaveLastLibrary();
        }

        /// <summary>
        /// Adds image files to library with user open-file dialog; uses background worker to add the files
        /// and uses this thread to display the add operation's progress to user
        /// </summary>
        private void AddFiles()
        {
            // Create OpenFileDialog to browse files
            OpenFileDialog dialog = new OpenFileDialog
            {
                // Filter dialog to only show supported image types
                Filter = "Images|*.jpg; *.jpeg; *.png; *.gif; *.bmp; *.exif; *.tiff",

                // Set dialog to select multiple files
                Multiselect = true
            };

            // If user clicked OK (not Cancel) in file dialog
            if (dialog.ShowDialog() == true)
            {
                // Create AddFileProgressModel for the progress bar
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

        /// <summary>
        /// Adds image files to library with user open-folder dialog; uses background worker to add the files
        /// and uses this thread to display the add operation's progress to user
        /// </summary>
        private void AddFolder()
        {
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

        /// <summary>
        /// Sends AddWork the filePaths needed to do add operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFilesWork(object sender, DoWorkEventArgs e)
        {
            // Get filepaths from OpenFileDialog
            OpenFileDialog dialog = (OpenFileDialog)e.Argument;
            List<string> filePaths = new List<string>(dialog.FileNames);

            // Do actual adding in generalized function using these filePaths
            AddWork(sender, filePaths);
        }

        /// <summary>
        /// Pulls image files from given folder (and sub-folders), then sends the filePaths to AddWork
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFolderWork(object sender, DoWorkEventArgs e)
        {
            // Get folder path string from selected path
            WinForms.FolderBrowserDialog dialog = (WinForms.FolderBrowserDialog)e.Argument;
            string folderPath = dialog.SelectedPath;

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

            // Do actual adding in generalized function using these filePaths
            AddWork(sender, filePaths);
        }

        /// <summary>
        /// Generalized function working with both adding files and adding folders; adds image files from
        /// given filePaths list to the library
        /// </summary>
        /// <param name="sender">BackgroundWorker running the task</param>
        /// <param name="filePaths">List of filepaths for image files to be added to library</param>
        private void AddWork(object sender, List<string> filePaths)
        {
            // For keeping track of progress
            int numFiles = filePaths.Count;
            int progress;

            // Create the WBImage object for each image file and add it to library
            for (int i = 0; i < filePaths.Count; i++)
            {
                // If user cancelled then exit loop and return before adding next file
                if (_worker.CancellationPending == true) return;

                // Get stream, path, and filename for current file
                string filePath = filePaths[i];
                string fileName = Path.GetFileName(filePath);

                // Create progress string to be shown in progress bar
                string progressString = string.Format("{0}|{1}|{2}",
                                                        fileName,
                                                        i + 1,
                                                        numFiles);

                // Get current progress as percentage of total files
                progress = Convert.ToInt32((double)i / numFiles * 100);

                // Report progress and current file being worked on
                (sender as BackgroundWorker).ReportProgress(progress, progressString);

                // Try to add current file to the library
                try
                {
                    // Create the bitmap in the scope of the background worker (this is the performance intensive
                    // task for this whole process)
                    using (Bitmap bitmap = new Bitmap(File.Open(filePaths[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
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
                // If file fails to open, notify user
                catch (IOException)
                {
                    // Show failure toast notif
                    _notifier.ShowError(string.Format("Failed to add {0} to library; file corrupted or in use by another program",
                                                        fileName));
                }
                // FIXME: find a way to overcome this horrific memory allocation conundrum!
                // If memory allocation for Bitmap fails, notify user that image file is too big
                catch (OutOfMemoryException)
                {
                    // Show failure toast notif
                    _notifier.ShowError(string.Format("Failed to add {0} to library; image file is too big!",
                                                        fileName));
                }
            }
        }

        /// <summary>
        /// Update progress of add operation; changes front-end progress bar percentage and string
        /// displayed inside the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateAddProgress(object sender, ProgressChangedEventArgs e)
        {
            // Update progress percentage
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

        /// <summary>
        /// Closes add operation progress window and notifies user that operation is complete (if this is
        /// their first add, i.e. adding files to empty library); also saves the new library to AppData file
        /// and updates the Manager to reflect changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            _addProgressViewModel.CloseWindow();

            // If this is the first add (i.e. starting with empty library) give 'all set' notification to
            // user, set default sort and reset the empty flag
            if (IsEmpty)
            {
                _notifier.ShowSuccess("All set! WallBrite will now synchronize your wallpapers with daylight levels.");
                SortType = "Brightness";
                SortDirection = "Descending";
                IsEmpty = false;
            }

            // Save changes to last library file
            SaveLastLibrary();

            // Update manager
            Manager.CheckAndUpdate();
        }

        /// <summary>
        /// Cancels the add operation and notifies user if at least one add in the operation completed
        /// successfully; also saves the new library to AppData file and updates the Manager to reflect changes
        /// </summary>
        private void CancelAdd()
        {
            _worker.CancelAsync();

            // If this is the first add (i.e. starting with empty library) and operation successfully managed
            // to add at least one file, give 'all set' notification to user, set default sort and reset the
            // empty flag
            if (IsEmpty && LibraryList.Count > 0)
            {
                _notifier.ShowSuccess("All set! WallBrite will now synchronize your wallpapers with daylight levels.");
                SortType = "Brightness";
                SortDirection = "Descending";
                IsEmpty = false;
            }

            // Save any changes to last library file
            SaveLastLibrary();

            // Update manager
            Manager.CheckAndUpdate();
        }

        /// <summary>
        /// Adds given WBImage at given filePath to the library if it is not already in library
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        private void AddImage(WBImage image)
        {
            // Give user message and don't add this image if it is already in library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(image.Path)))
            {
                _notifier.ShowInformation(string.Format("{0} was not added since it is already in the library",
                                                         Path.GetFileName(image.Path)));
            }
            // Otherwise add the image to the library
            else
            {
                LibraryList.Add(image);
            }
        }

        /// <summary>
        /// Updates ListView element displaying images (ImageGrid) to reflect sort type change
        /// </summary>
        /// <param name="selected">Sort type selected by user; either
        /// "Brightness",
        /// "Date Added",
        /// or "Enabled"</param>
        private void SortTypeChanged(string selected)
        {
            // Only do sort work if ImageGrid already exists
            if (ImageGrid != null)
            {
                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ImageGrid.ItemsSource);

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
        /// Updates ListView element displaying images (ImageGrid) to reflect sort direction change
        /// </summary>
        /// <param name="selected">Sort direction selected by user; either
        /// "Ascending"
        /// or "Descending"</param>
        private void SortDirectionChanged(string selected)
        {
            // Only do sort work if ImageGrid already exists
            if (ImageGrid != null)
            {
                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ImageGrid.ItemsSource);

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