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
        public MainViewModel MainViewModel { get; private set; }

        public MainWindow(bool startingMinimized)
        {
            InitializeComponent();
            MainViewModel = new MainViewModel(this, startingMinimized, imageGrid);
            DataContext = MainViewModel;
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
    }
}