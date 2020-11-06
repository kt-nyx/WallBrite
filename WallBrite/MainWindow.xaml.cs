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
        //TODO: AUTOMATION UI CLEANUP
        //TODO: add help button(s)?
        //TODO: add details for selected image on left panel?

        //TODO: attempt to open last used library on launch (setting)
        //TODO: run on startup setting

        //TODO: MAKE INTO BACKGROUND PROCESS WHEN CLOSED
        //TODO: taskbar icon options?

        //TODO: IMPROVE SORT EFFICIENCY; lag here for some reason...

        //TODO: MAKE INSTALLER
        //TODO: remove fody dll thing



        //FIXME: remove this reference and make sorttype events into commands inside the main vm
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            _mainViewModel = new MainViewModel(this);
            InitializeComponent();
            DataContext = _mainViewModel;

            //TODO: remove
            BottomPanel.DataContext = _mainViewModel.Manager;
        }

        //TODO: find way to move this?
        /// <summary>
        /// Minimizes to tray when trying to close application (user exits from tray icon menu instead)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            // setting cancel to true will cancel the close request
            // so the application is not closed
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
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