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

        private void SortTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            library.SortTypeChanged(sender, imageGrid);
        }

        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
            library.SortDirectionChanged(sender, imageGrid);
        }

        private void Cool1Click(object sender, RoutedEventArgs e)
        {

        }
    }
}