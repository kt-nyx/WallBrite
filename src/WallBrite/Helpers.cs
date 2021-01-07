using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    public static class Helpers
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapImage GetThumbnailFromBitmap(Bitmap bitmap)
        {
            BitmapImage thumbnail;
            // Create thumbnail, maintaining aspect ratio but staying within a 200 * 200 box:
            // If image width larger than height; set width of thumbnail to 200 and reduce height
            // proportionally
            if (bitmap.Width >= bitmap.Height)
                thumbnail = BitmapToBitmapImage(new Bitmap(bitmap, new System.Drawing.Size(200,
                                                                                           200 * bitmap.Height / bitmap.Width)));

            // If image height larger than width; set height of thumbnail to 200 and reduce width
            // proportionally
            else
                thumbnail = BitmapToBitmapImage(new Bitmap(bitmap, new System.Drawing.Size(200 * bitmap.Width / bitmap.Height,
                                                                                           200)));

            return thumbnail;

        }

        private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }


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
        public static byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
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
