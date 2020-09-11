using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallBrite
{
    class WBLibrary
    {
        public List<WBImage> libraryList { get; }

        public WBLibrary ()
        {
            // Create new empty library list
            libraryList = new List<WBImage>();
        }

        public void AddImage(WBImage image)
        {
            libraryList.Add(image);
        }

        public void RemoveImage(WBImage image)
        {
            libraryList.Remove(image);
        }

        public bool Sort(string sortType)
        {
            // Sort library by brightness values
            if (sortType == "brightness")
            {
                libraryList.Sort((image1, image2) => image1.AverageBrightness.CompareTo(image2.AverageBrightness));
                return true;
            } else if (sortType == "date")
            {
                libraryList.Sort((image1, image2) => image1.CreationDate.CompareTo(image2.CreationDate));
                return true;
            } else if (sortType == "enabled")
            {
                libraryList.Sort((image1, image2) => image1.Enabled.CompareTo(image2.Enabled));
                return true;
            }
            return false;
        }
    }
}
