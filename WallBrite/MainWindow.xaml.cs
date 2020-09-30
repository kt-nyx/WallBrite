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

        public MainWindow()
        {
            library = new LibraryViewModel();
            InitializeComponent();
            DataContext = library;
        }

        private void AddFiles(object sender, RoutedEventArgs e)
        {
            library.AddFiles();
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            library.AddFolder();
        }

        // TODO: add alphabetical; move code
        private void SortTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;

            // Only do sort work if imageGrid already exists
            if (imageGrid != null)
            {
                // Get selected sort type
                string selected = box.SelectedValue.ToString();

                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(imageGrid.ItemsSource);

                // Get current direction of sort to be used (if there is one); default to ascending if none
                ListSortDirection direction;
                if (view.SortDescriptions.Count > 0)
                {
                    direction = view.SortDescriptions[0].Direction;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }

                // Clear current sort
                view.SortDescriptions.Clear();

                // Set appropriate sort
                if (selected.Equals("Brightness"))
                {
                    view.SortDescriptions.Add(new SortDescription("AverageBrightness", direction));
                }
                else if (selected.Equals("Date Added"))
                {
                    view.SortDescriptions.Add(new SortDescription("AddedDate", direction));
                }
                else if (selected.Equals("Enabled"))
                {
                    view.SortDescriptions.Add(new SortDescription("IsEnabled", direction));
                }
            }
        }

        // TODO: move code
        private void SortDirectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            // Only do sort work if imageGrid already exists
            if (imageGrid != null)
            {
                // Get selected sort type
                string selected = box.SelectedValue.ToString();

                // Get view for image grid
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(imageGrid.ItemsSource);

                // Get current direction of sort to be used (if there is one); default to ascending if none
                string currentSort;
                if (view.SortDescriptions.Count > 0)
                {
                    currentSort = view.SortDescriptions[0].PropertyName;
                }
                else
                {
                    currentSort = "DateAdded";
                }

                // Clear current sort
                view.SortDescriptions.Clear();

                // Set appropriate sort
                if (selected.Equals("Descending"))
                {
                    view.SortDescriptions.Add(new SortDescription(currentSort, ListSortDirection.Descending));
                }
                else if (selected.Equals("Ascending"))
                {
                    view.SortDescriptions.Add(new SortDescription(currentSort, ListSortDirection.Ascending));
                }
            }
        }

        private void Cool1Click(object sender, RoutedEventArgs e)
        {
            Manager.BrightestTime = new DateTime(1, 1, 1, 12, 0, 0);
            Manager.DarkestTime = new DateTime(1, 1, 1, 0, 0, 0);
            Manager.ManageWalls(library);
        }
    }
}