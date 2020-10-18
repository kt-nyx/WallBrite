using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace WallBrite
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WBImage> LibraryList { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand EnableCommand { get; set; }
        public ICommand DisableCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand AddFilesCommand { get; set; }
        public ICommand AddFolderCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public LibraryViewModel()
        {
            // Create new empty library list
            LibraryList = new ObservableCollection<WBImage>();

            // Create commands
            CreateCommands();
        }

        public LibraryViewModel(WBImage[] libraryList)
        {
            // Create library list from given
            LibraryList = new ObservableCollection<WBImage>(libraryList);

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
        }

        // TODO: change placeholder path to a relative? path
        private void SaveLibrary()
        {
            File.WriteAllText(@"c:\cool.json", JsonConvert.SerializeObject(LibraryList, Formatting.Indented));
        }
        /// <summary>
        /// Adds given WBImage at given filePath to the library; or throws exception if image is
        /// already in the library
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when image being added is already in library (its path matches the path of another image 
        /// already in the library)
        /// </exception>
        public void AddImage(WBImage image, string filePath)
        {
            // Return false and don't add this image if it's already in the library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(filePath)))
            {
                throw new InvalidOperationException("This image is already in the library");
            }

            // Add the image to the library and return true (if it's not already in the library)
            LibraryList.Add(image);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public void RemoveImage(WBImage image)
        {
            LibraryList.Remove(image);
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
                RemoveImage(image);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        private void Enable(object collection) {
            // Cast the collection of controls to a collection of WBImages
            System.Collections.IList items = (System.Collections.IList)collection;
            var selectedImages = items.Cast<WBImage>();

            // Enable each WBImage in the collection
            foreach (WBImage image in selectedImages)
            {
                image.IsEnabled = true;
            }
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
                    AddImage(image, filePath);
                }
            }
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
                    AddImage(image, filePath);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="imageGrid"></param>
        public void SortTypeChanged (object sender, ListView imageGrid)
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
