using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WallBrite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;

        /// <summary>
        /// Starts app minimized or with window open depending on whether -minimized flag is set (unless
        /// manually run through cmd, this is only set if running from windows startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Check for start minimized argument
            if (Array.Exists(e.Args, element => element == "-minimized"))
            {
                _mainWindow = new MainWindow(true);
                _mainWindow.Hide();
            } else
            {
                _mainWindow = new MainWindow(false);
            }
        }

        /// <summary>
        /// Saves last used library and current manager settings before exiting app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save current library as last library before exiting
            _mainWindow.MainViewModel.Library.SaveLastLibrary();
            // Also save current settings
            _mainWindow.MainViewModel.Manager.SaveSettings();
        }


    }
}
