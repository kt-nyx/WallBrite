using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallBrite
{
    public class LibraryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WBImage> LibraryList { get; }

        private string _sortOrder;

        private readonly string[] AllowedSortOrders = new string[] { "descending", "ascending" };
        private readonly string[] AllowedSortTypes = new string[] { "brightness", "date", "enabled" };

        public string SortType { get; private set; }

        public string SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                // Check that this is valid sort order (ascending or descending)
                if (!AllowedSortOrders.Any(checkOrder => checkOrder == value))
                {
                    throw new ArgumentException("Not a valid sort order");
                }

                if (_sortOrder != value)
                {
                    LibraryList.Reverse();
                }

                _sortOrder = value;
            }
        }

        public LibraryViewModel()
        {
            // Create new empty library list
            LibraryList = new ObservableCollection<WBImage>();
        }

        /// <summary>
        /// Adds given WBImage at given filePath to the library; or throws exception if image is
        /// already in the library
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when image being added is already in library (its path matches the path of another image 
        /// already in the library)
        /// </exception>
        public void AddImage(WBImage image, string filePath)
        {
            // Return false and don't add this image if it's already in the library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(filePath)))
            {
                throw new InvalidOperationException("This image is already in the library");
            }

            // Add the image to the library and return true (if it's not already in the library)
            LibraryList.Add(image);
        }

        // TODO: add alphabetical
        /// <summary>
        /// Sorts the image list by given sort type: valid inputs are "brightness", "date" or "enabled"
        /// Brightness sorts by the average brightness value of each WBImage in the list
        /// Date sorts by creation date (date added to the library)
        /// Enabled sorts by the enabled bool (whether this image is enabled in the library)
        /// </summary>
        /// <param name="sortType"></param>
        /// <exception cref="ArgumentException">Thrown when sort type given is not a valid sort type</exception>
        public void Sort(string sortType)
        {
            // If invalid sort type, return false
            if (!AllowedSortTypes.Any(checkType => checkType == sortType))
            {
                throw new ArgumentException("Not a valid sort type");
            }

            // Sort library by brightness values
            if (sortType == "brightness")
            {
                LibraryList.OrderBy(image=> image.AverageBrightness);
            }
            // Sort library by creation date (date added to library)
            else if (sortType == "date")
            {
                LibraryList.OrderBy(image => image.AddedDate);
            }
            // Sort library by enabled status
            else if (sortType == "enabled")
            {
                LibraryList.OrderBy(image => image.IsEnabled);
            }

            // Set internal SortType property to given sortType
            SortType = sortType;
        }
    }
}
