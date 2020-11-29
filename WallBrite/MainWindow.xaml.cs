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

        //TODO: attempt to open last used library on launch

        //TODO: MAKE INSTALLER
        //TODO: remove fody dll thing



        //FIXME: remove this reference and make sorttype events into commands inside the main vm
        public MainViewModel MainViewModel { get; private set; }

        public MainWindow(bool startingMinimized)
        {
            MainViewModel = new MainViewModel(this, startingMinimized);
            InitializeComponent();
            DataContext = MainViewModel;

            //TODO: remove
            BottomPanel.DataContext = MainViewModel.Manager;
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
            MainViewModel.Library.SortTypeChanged(sender, imageGrid);
        }

        //TODO: make into command
        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainViewModel.Library.SortDirectionChanged(sender, imageGrid);
        }
    }
}