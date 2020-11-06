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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();

            // Check for any arguments
            if (e.Args.Length > 0)
            {
                // Check for start minimized argument
                if (Array.Exists(e.Args, element => element == "-minimized"))
                {
                    wnd.Hide();
                } else
                {
                    wnd.Show();
                }
            }
            // Default startup (no arguments)
            else
            {
                wnd.Show();
            }
        }
    }
}
