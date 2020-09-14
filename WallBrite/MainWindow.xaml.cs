using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += OnWindowSizeChange;
        }
        

        private void OnWindowSizeChange(object sender, SizeChangedEventArgs e)
        {
            RefreshImageGrid();
        }

        private void AddFiles(object sender, RoutedEventArgs e)
        {
            WBManager.AddFiles();
            RefreshImageGrid();
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            WBManager.AddFolder();
            RefreshImageGrid();
        }

        // TODO: move refresh code to a place that makes more sense
        private void RefreshImageGrid ()
        {
            // Clear grid of images before repopulating
            imageGrid.Children.Clear();
            imageGrid.RowDefinitions.Clear();
            imageGrid.ColumnDefinitions.Clear();

            // Create first row before populating
            RowDefinition firstRow = new RowDefinition
            {
                Height = new GridLength(200)
            };
            imageGrid.RowDefinitions.Add(firstRow);

            // Maintain count of current row and column in grid for image placement
            int row = 0;
            int column = 0;

            double availableGridWidth = MainPanel.ActualWidth - SidePanel.ActualWidth;
            // Total width currently taken up by all grid columns; for use in resizing grid to fit window
            int totalWidth = 0;

            // Loop over each image in the library
            foreach (WBImage wbImage in WBLibrary.LibraryList)
            {
                // Create image to be shown in library grid; set source to thumbnail for this WBImage
                Image gridImage = new Image();
                BitmapImage sourceBitmap = WBHelpers.ImagetoBitmapSource(wbImage.Thumbnail);
                gridImage.Source = sourceBitmap;

                // Add image to grid
                imageGrid.Children.Add(gridImage);
                

                // If placing this image in next column will fit in window, then place it there
                if (totalWidth + 200 <= availableGridWidth)
                {
                    // If this is first row, column still needs to be created, so create it
                    if (row == 0)
                    {
                        ColumnDefinition newColumn = new ColumnDefinition
                        {
                            Width = new GridLength(200)
                        };
                        imageGrid.ColumnDefinitions.Add(newColumn);
                    }

                    // PLace image at this row and column
                    Grid.SetRow(gridImage, row);
                    Grid.SetColumn(gridImage, column);

                    // Increment counters for next image (possibly) in this row
                    column++;
                    totalWidth += 200;
                } 
                // Otherwise place this image on a new row starting at column 0
                else {
                    // Create new row
                    RowDefinition newRow = new RowDefinition
                    {
                        Height = new GridLength(200)
                    };
                    imageGrid.RowDefinitions.Add(newRow);
 
                    // Increment row counter and reset column and width counters
                    row++;
                    column = 0;

                    // Place image at this row and column
                    Grid.SetRow(gridImage, row);
                    Grid.SetColumn(gridImage, column);

                    // Increment column and total width counters so next image is (possibly) placed in next
                    // column
                    column++;
                    totalWidth = 200;
                }
            }
        }
        
    }
}