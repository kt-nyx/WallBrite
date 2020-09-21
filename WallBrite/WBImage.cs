using System;
using System.Drawing;
using System.IO;

namespace WallBrite
{
    public class WBImage
    {
        /// <summary>
        /// Image represented by this WBImage
        /// </summary>
        private readonly Bitmap _image;

        /// <summary>
        /// Average brightness calculated (or manually set) for this image
        /// </summary>
        public float AverageBrightness { get; set; }

        /// <summary>
        /// Whether this WBImage will show up in the library
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creation date of this WBImage
        /// </summary>
        public DateTime AddedDate { get; private set; }

        /// <summary>
        /// Thumbnail image for use in library UI
        /// </summary>
        public Image Thumbnail { get; private set; }

        /// <summary>
        /// Path to the original image
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Creates WBImage using image from given stream and file path
        /// Calculates its average brightness, creates a proportionate thumbnail, sets creation date, and
        /// stores the given path to the original file
        /// </summary>
        /// <param name="stream"></param>
        public WBImage(Stream stream, string path)
        {
            // Create bitmap using image from given stream
            _image = new Bitmap(stream);

            // Create thumbnail, maintaining aspect ratio but staying within a 200 * 200 box:
            // If image width larger than height; set width of thumbnail to 200 and reduce height
            // proportionally
            if (_image.Width >= _image.Height)
                Thumbnail = _image.GetThumbnailImage(200,
                                                     200 * _image.Height / _image.Width,
                                                     null, IntPtr.Zero);

            // If image height larger than width; set height of thumbnail to 200 and reduce width
            // proportionally
            else Thumbnail = _image.GetThumbnailImage(200 * _image.Width / _image.Height,
                                                      200,
                                                      null, IntPtr.Zero);

            // Calculate and set average brightness of this WBImage
            CalculateAverageBrightness();

            // Set creation date
            AddedDate = DateTime.Now;

            // Set enabled true by default
            IsEnabled = true;

            // Set the path
            Path = path;
        }

        /// <summary>
        /// Sets and returns average brightness level (from fully black at 0.0 to fully white at 1.0) of this
        /// WBImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Average brightness level of given bitmap image</returns>
        public float CalculateAverageBrightness()
        {
            // Brightness value to be summed and averaged
            AverageBrightness = 0;

            // Get log of width and height of image; to be used when looping over pixels to
            // increase efficiency
            // (Rather than looping over every pixel, using the log will allow taking a sample
            //  of only pixels on every log(width)th column and log(height)th row)
            int widthLog = Convert.ToInt32(Math.Log(_image.Width));
            int heightLog = Convert.ToInt32(Math.Log(_image.Height));

            // Loop over image's pixels (taking only the logged sample as described above)
            for (int x = 0; x < _image.Width; x += widthLog)
            {
                for (int y = 0; y < _image.Height; y += heightLog)
                {
                    // For every sampled pixel, get the color value of the pixel and add its brightness to
                    // the running sum
                    Color pixelColor = _image.GetPixel(x, y);
                    AverageBrightness += pixelColor.GetBrightness();
                }
            }

            // Divide summed brightness by the number of pixels sampled to get the average
            AverageBrightness /= (_image.Width / widthLog) * (_image.Height / heightLog);

            return AverageBrightness;
        }
    }
}