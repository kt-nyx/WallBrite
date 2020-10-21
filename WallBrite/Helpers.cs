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

        /// <summary>
        /// Converts BitmapImage (e.g. WBImage thumbnails) to a byte array for serialization
        /// Mostly taken from https://stackoverflow.com/questions/6597676/bitmapimage-to-byte
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public static byte[] BitmapSourcetoByteArray(BitmapImage bitmapImage)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }
    }
}
