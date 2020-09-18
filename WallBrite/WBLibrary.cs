using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace WallBrite
{
    internal static class WBLibrary
    {
        public static List<WBImage> LibraryList { get; }

        private static string _sortOrder;

        private static string[] AllowedSortOrders = new string[] { "descending", "ascending" };
        private static string[] AllowedSortTypes = new string[] { "brightness", "date", "enabled" };

        public static string SortType { get; private set; }

        public static string SortOrder
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

        static WBLibrary()
        {
            // Create new empty library list
            LibraryList = new List<WBImage>();
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
        public static void AddImage(WBImage image, string filePath)
        {
            // Return false and don't add this image if it's already in the library
            if (LibraryList.Any(checkImage => checkImage.Path.Equals(filePath))) {
                throw new InvalidOperationException("This image is already in the library");
            }

            // Add the image to the library and return true (if it's not already in the library)
            LibraryList.Add(image);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if given image is not actually in the library
        /// </exception>
        public static void RemoveImage(WBImage image)
        {
            // Return false and don't remove this image if it's not in the library
            if (!LibraryList.Any(checkImage => checkImage == image))
            {
                throw new InvalidOperationException("This image not in the library");
            }

            // Otherwise remove image from library
            LibraryList.Remove(image);
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
        public static void Sort(string sortType)
        {
            // If invalid sort type, return false
            if (!AllowedSortTypes.Any(checkType => checkType == sortType))
            {
                throw new ArgumentException("Not a valid sort type");
            }

            // Sort library by brightness values
            if (sortType == "brightness")
            {
                LibraryList.Sort((image1, image2) => image1.AverageBrightness.CompareTo(image2.AverageBrightness));
            }
            // Sort library by creation date (date added to library)
            else if (sortType == "date")
            {
                LibraryList.Sort((image1, image2) => image1.AddedDate.CompareTo(image2.AddedDate));
            }
            // Sort library by enabled status
            else if (sortType == "enabled")
            {
                LibraryList.Sort((image1, image2) => image1.isEnabled.CompareTo(image2.isEnabled));
            }

            // Set internal SortType property to given sortType
            SortType = sortType;
        }
    }
}