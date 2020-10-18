using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WallBrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: ADD SAVING
            //TODO: add new, open, save library buttons
            //TODO: add Library header, make "library controls" non-bold
            //TODO: make autosave?

        //TODO: AUTOMATION UI CLEANUP
            //TODO: add help button(s)?
            //TODO: add alert when wallpaper did/did not change on update
            
        //TODO: MAKE INTO BACKGROUND PROCESS WHEN CLOSED
            //TODO: run in background
            //TODO: taskbar icon
                //TODO: taskbar icon options?
    
        //TODO: ADDFILES SPEED + CLARITY
            //TODO: add progress bar when adding files
            //TODO: add cancel button
            //TODO: improve efficiency
                //TODO: processing

        //TODO: IMPROVE SORT EFFICIENCY; lag here for some reason...

        //TODO: MAKE INSTALLER
            //TODO: remove fody dll thing?
        
        //TODO: add manual set wallpaper button

        private LibraryViewModel _library;
        private readonly ManagerViewModel _manager;

        public MainWindow()
        {
            _library = new LibraryViewModel();
            _manager = new ManagerViewModel(_library);
            InitializeComponent();
            DataContext = _library;
            BottomPanel.DataContext = _manager;
        }

        private void OpenLibrary(object sender, RoutedEventArgs e)
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
                Stream fileStream = dialog.OpenFile();

                var serializer = new JsonSerializer();

                using (var streamReader = new StreamReader(fileStream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    // Deserialize WBImage array from file
                    WBImage[] imageArray = (WBImage[]) serializer.Deserialize(jsonTextReader, typeof(WBImage[]));

                    // Create new library VM using image array
                    _library = new LibraryViewModel(imageArray);

                    // Reset data context
                    DataContext = _library;
                }
            }
        }

        //TODO: make into command
        private void SortTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            _library.SortTypeChanged(sender, imageGrid);
        }

        //TODO: make into command
        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _library.SortDirectionChanged(sender, imageGrid);
        }
    }
}