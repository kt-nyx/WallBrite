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
        public bool Enabled { get; set; }

        /// <summary>
        /// Creation date of this WBImage
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Thumbnail image for use in library UI
        /// </summary>
        public Image Thumbnail { get; private set; }

        /// <summary>
        /// Creates WBImage using image from given stream; 
        /// calculates its average brightness, creates a proportionate thumbnail, sets creation date
        /// </summary>
        /// <param name="stream"></param>
        public WBImage(Stream stream)
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
            CreationDate = DateTime.Now;
        }

        // FIXME: overloads on the chopping block
        ///// <summary>
        ///// Creates WBImage using given Bitmap image
        ///// </summary>
        ///// <param name="bitmap"></param>
        //public WBImage(Bitmap bitmap)
        //{
        //    // Create bitmap using given Bitmap image
        //    _image = bitmap;

        //    // Create thumbnail
        //    Thumbnail = _image.GetThumbnailImage(200,
        //    200 * _image.Height / _image.Width, null, IntPtr.Zero);

        //    // Calculate and set average brightness of this WBImage
        //    CalculateAverageBrightness();
        //}

        ///// <summary>
        ///// Creates WBImage using image at given path
        ///// </summary>
        ///// <param name="path"></param>
        //public WBImage(string path)
        //{
        //    // Create bitmap using image at given path
        //    _image = new Bitmap(@path);

        //    // Create thumbnail
        //    Thumbnail = _image.GetThumbnailImage(200,
        //    200 * _image.Height / _image.Width, null, IntPtr.Zero);

        //    // Calculate and set average brightness of this WBImage
        //    CalculateAverageBrightness();
        //}

        /// <summary>
        /// Returns the average brightness level (from fully black at 0.0 to fully white at 1.0) of given
        /// bitmap image (approximated for efficiency)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Average brightness level of given bitmap image</returns>
        public float CalculateAverageBrightness()
        {
            // Brightness value to be summed and averaged
            float averageBrightness = 0;

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
                    averageBrightness += pixelColor.GetBrightness();
                }
            }

            // Divide summed brightness by the number of pixels sampled to get the average
            averageBrightness /= (_image.Width / widthLog) * (_image.Height / heightLog);

            return averageBrightness;
        }

        // FIXME: overloads on the chopping block
        ///// <summary>
        ///// Returns the average brightness level (from fully black at 0.0 to fully white at 1.0) of image
        ///// at given FULL path (approximated for efficiency)
        ///// </summary>
        ///// <param name="path"></param>
        ///// <exception cref="FileNotFoundException">Thrown when no file found at given path</exception>
        ///// <returns>Average brightness level of image at given path</returns>
        //public static float CalculateAverageBrightness(string path)
        //{
        //    // Create bitmap from image at path
        //    Bitmap bitmap = new Bitmap(@path);

        //    return CalculateAverageBrightness(bitmap);
        //}

        ///// <summary>
        ///// Returns the average brightness level (from fully black at 0.0 to fully white at 1.0) of image
        ///// in given stream (approximated for efficiency)
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <exception cref="ArgumentException">
        ///// Thrown when stream does not contain an image or is null
        ///// </exception>
        ///// <returns>Average brightness level of image in given stream</returns>
        //public static float CalculateAverageBrightness(Stream stream)
        //{
        //    // Create bitmap from image at path
        //    Bitmap bitmap = new Bitmap(stream);

        //    return CalculateAverageBrightness(bitmap);
        //}

        ///// <summary>
        ///// Calculates, sets and returns the averageBrightness (from fully black at 0.0 to fully white at
        ///// 1.0) of image represented by this WBImage (approximated for efficiency)
        ///// </summary>
        ///// <returns>averageBrightness of image represented by this WBImage</returns>
        //private float CalculateAverageBrightness()
        //{
        //    // Set the field
        //    AverageBrightness = CalculateAverageBrightness(_image);
        //    return AverageBrightness;
        //}
    }
}