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
        //TODO: add new library button
        //TODO: make autosave?

        //TODO: AUTOMATION UI CLEANUP
        //TODO: add help button(s)?
        //TODO: add details for selected image on left panel?

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




        //FIXME: remove this reference and make sorttype events into commands inside the main vm
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            _mainViewModel = new MainViewModel();
            InitializeComponent();
            DataContext = _mainViewModel;
            BottomPanel.DataContext = _mainViewModel.Manager;
        }

        //TODO: make into command
        private void SortTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            _mainViewModel.Library.SortTypeChanged(sender, imageGrid);
        }

        //TODO: make into command
        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
           _mainViewModel.Library.SortDirectionChanged(sender, imageGrid);
        }
    }
}