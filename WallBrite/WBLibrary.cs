using System.Collections.Generic;

namespace WallBrite
{
    internal static class WBLibrary
    {
        public static List<WBImage> LibraryList { get; }

        static WBLibrary()
        {
            // Create new empty library list
            LibraryList = new List<WBImage>();
        }

        public static void AddImage(WBImage image)
        {
            LibraryList.Add(image);
        }

        public static void RemoveImage(WBImage image)
        {
            LibraryList.Remove(image);
        }

        public static bool Sort(string sortType)
        {
            // Sort library by brightness values
            if (sortType == "brightness")
            {
                LibraryList.Sort((image1, image2) => image1.AverageBrightness.CompareTo(image2.AverageBrightness));
                return true;
            }
            else if (sortType == "date")
            {
                LibraryList.Sort((image1, image2) => image1.AddedDate.CompareTo(image2.AddedDate));
                return true;
            }
            else if (sortType == "enabled")
            {
                LibraryList.Sort((image1, image2) => image1.Enabled.CompareTo(image2.Enabled));
                return true;
            }
            return false;
        }
    }
}