using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    public static class Helpers
    {

        /// <summary>
        /// Converts System.Drawing Image to a System.Windows.Media.Imaging BitmapImage, for use as an image
        /// source in the WPF
        /// Mostly taken from:
        /// https://stackoverflow.com/questions/41998142/converting-system-drawing-image-to-system-windows-media-imagesource-with-no-resu
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapImage ImagetoBitmapSource (Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
