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
            //TODO: change progress bar to show PROGRESS TO NEXT ACTUALLLL UPDATE
                //TODO: simulate the change at the next update time?
                //TODO: store result to increase processing efficiency for next actual update?
            //TODO: change update to check?
            //TODO: add tooltip(s)
            //TODO: change layout to be less cluttered; use horizontal space
            //TODO: add alert when wallpaper did/did not change on update
            //TODO: add progress bar between brightest/darkest time of day
                //TODO: make them switch places depending on whether approaching brightest or darkest 
                //TODO: use invertedbrightness to check; change the name of this to approachingDark or
                //TODO: something
            //TODO: add NEXT wallpaper?
            
        //TODO: MAKE INTO BACKGROUND PROCESS WHEN CLOSED
            //TODO: run in background
            //TODO: taskbar icon
    
        //TODO: ADDFILES SPEED + CLARITY
            //TODO: add progress bar when adding files
            //TODO: add cancel button
            //TODO: improve efficiency
                //TODO: memory
                //TODO: processing

        //TODO: IMPROVE SORT EFFICIENCY; LAG HERE for some reason...

        //TODO: MAKE INSTALLER
            //TODO: remove fody dll thing?
        
        //TODO: add manual set wallpaper button

        //TODO: add 'disabled' text over disabled walls

        private LibraryViewModel library;
        private ManagerViewModel manager;

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