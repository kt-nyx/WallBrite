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
            Manager.AddFiles(library);
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            Manager.AddFolder(library);
        }

        // TODO: add alphabetical; move code, add comments
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

        // TODO: move code, add comments, add sort order backend
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

        // TODO: move refresh code to a place that makes more sense
        //private void RefreshImageGrid()
        //{
        //    // Only refresh if images exist in library
        //    if (WBLibrary.LibraryList.Count > 0)
        //    {
        //        // Clear grid of images before repopulating
        //        imageGrid.Children.Clear();
        //        imageGrid.RowDefinitions.Clear();
        //        imageGrid.ColumnDefinitions.Clear();

        //        // Create first row before populating
        //        RowDefinition firstRow = new RowDefinition
        //        {
        //            Height = new GridLength(240)
        //        };
        //        imageGrid.RowDefinitions.Add(firstRow);

        //        // Maintain count of current row and column in grid for image placement
        //        int row = 0;
        //        int column = 0;

        //        // Total available space for the grid (the total window space - the side panel)
        //        double availableGridWidth = MainPanel.ActualWidth - SidePanel.ActualWidth;

        //        // Total width currently taken up by all grid columns; for use in resizing grid to fit window
        //        int totalWidth = 0;

        //        // Bool for special case when rendering first image
        //        bool firstImage = true;

        //        // Loop over each image in the library
        //        //foreach (WBImage wbImage in WBLibrary.LibraryList)
        //        for (int imageIndex = 0; imageIndex < WBLibrary.LibraryList.Count; imageIndex++)
        //        {
        //            WBImage wbImage = WBLibrary.LibraryList[imageIndex];

        //            // Create image to be shown in library grid; set source to thumbnail for this WBImage
        //            Image gridImage = new Image();
        //            BitmapImage sourceBitmap = WBHelpers.ImagetoBitmapSource(wbImage.Thumbnail);
        //            gridImage.Source = sourceBitmap;

        //            // TODO: implement dependency (attached?) property instead
        //            // Set index value for this image to the current index (using tag)
        //            gridImage.Tag = imageIndex;

        //            // Add click handler for clicking this image
        //            gridImage.MouseUp += ImageClickHandler;

        //            // Calculate background color for this image based on its AverageBrightness
        //            int backgroundBrightness = (int)Math.Round(wbImage.AverageBrightness * 255);

        //            // Create border for image
        //            Border imageBorder = new Border
        //            {
        //                BorderBrush = new SolidColorBrush(Colors.Black),
        //                BorderThickness = new Thickness(1),
        //                Child = gridImage,
        //                Margin = new Thickness(20),
        //                Background = new SolidColorBrush(Color.FromArgb(255,
        //                                                                (byte)backgroundBrightness,
        //                                                                (byte)backgroundBrightness,
        //                                                                (byte)backgroundBrightness))
        //            };

        //            // Add image to grid
        //            imageGrid.Children.Add(imageBorder);

        //            // If placing this image in next column will fit in window, then place it there
        //            // Also place it here if this is first image and it won't fit (otherwise logic will place the first image on second row)
        //            // This allows for the cut off rendering of the library to stil appear consistent (if user resizes window super small so
        //            // not even first image can fit, it will still render part of the image on the first row/column)
        //            if (totalWidth + 240 <= availableGridWidth || firstImage == true)
        //            {
        //                // If this is first row, column still needs to be created, so create it
        //                if (row == 0)
        //                {
        //                    ColumnDefinition newColumn = new ColumnDefinition
        //                    {
        //                        Width = new GridLength(240)
        //                    };
        //                    imageGrid.ColumnDefinitions.Add(newColumn);
        //                }

        //                // Place image at this row and column
        //                Grid.SetRow(imageBorder, row);
        //                Grid.SetColumn(imageBorder, column);

        //                // Increment counters for next image (possibly) in this row
        //                column++;
        //                totalWidth += 240;

        //                // If this was first image, set flag to false
        //                if (firstImage == true)
        //                {
        //                    firstImage = false;
        //                }
        //            }
        //            // Otherwise place this image on a new row starting at column 0
        //            else
        //            {
        //                // Create new row
        //                RowDefinition newRow = new RowDefinition
        //                {
        //                    Height = new GridLength(240)
        //                };
        //                imageGrid.RowDefinitions.Add(newRow);

        //                // Increment row counter and reset column and width counters
        //                row++;
        //                column = 0;

        //                // Place image at this row and column
        //                Grid.SetRow(imageBorder, row);
        //                Grid.SetColumn(imageBorder, column);

        //                // Increment column and total width counters so next image is (possibly) placed in next
        //                // column
        //                column++;
        //                totalWidth = 240;
        //            }
        //        }
        //    }
        //}

        //private void ImageClickHandler(object sender, MouseButtonEventArgs e)
        //{
        //    string cool = "cool";
        //}

        private void Cool1Click(object sender, RoutedEventArgs e)
        {
            Manager.BrightestTime = new DateTime(1, 1, 1, 12, 0, 0);
            Manager.DarkestTime = new DateTime(1, 1, 1, 0, 0, 0);
            Manager.ManageWalls();
        }
    }
}