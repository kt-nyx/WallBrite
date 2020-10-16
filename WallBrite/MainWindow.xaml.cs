using System;
using System.ComponentModel;
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

        private readonly LibraryViewModel library;
        private readonly ManagerViewModel manager;

        public MainWindow()
        {
            library = new LibraryViewModel();
            manager = new ManagerViewModel(library);
            InitializeComponent();
            DataContext = library;
            BottomPanel.DataContext = manager;
        }

        //TODO: make into command
        private void SortTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            library.SortTypeChanged(sender, imageGrid);
        }

        //TODO: make into command
        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
            library.SortDirectionChanged(sender, imageGrid);
        }
    }
}