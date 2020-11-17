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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _mainWindow = new MainWindow();

            // Check for any arguments
            if (e.Args.Length > 0)
            {
                // Check for start minimized argument
                if (Array.Exists(e.Args, element => element == "-minimized"))
                {
                    _mainWindow.Hide();
                } else
                {
                    _mainWindow.Show();
                }
            }
            // Default startup (no arguments)
            else
            {
                _mainWindow.Show();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Save current library as last library before exiting
            _mainWindow.mainViewModel.Library.SaveLastLibrary();
        }


    }
}
