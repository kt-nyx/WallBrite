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
    }
}
