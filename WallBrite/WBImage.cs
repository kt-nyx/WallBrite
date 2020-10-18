using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    public class WBImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Average brightness calculated (or manually set) for this image
        /// </summary>
        public float AverageBrightness { get; private set; }

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
        [JsonIgnore]
        public BitmapImage Thumbnail { get; private set; }

        /// <summary>
        /// Background color for use in library UI
        /// </summary>
        public SolidColorBrush BackgroundColor { get; private set; }

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
            // Free up this bitmap memory after using it to create the WBImage
            using (Bitmap image = new Bitmap(stream)) {
                // Create thumbnail, maintaining aspect ratio but staying within a 200 * 200 box:
                // If image width larger than height; set width of thumbnail to 200 and reduce height
                // proportionally
                if (image.Width >= image.Height)
                    Thumbnail = Helpers.ImagetoBitmapSource(image.GetThumbnailImage(200,
                                                         200 * image.Height / image.Width,
                                                         null, IntPtr.Zero));

                // If image height larger than width; set height of thumbnail to 200 and reduce width
                // proportionally
                else Thumbnail = Helpers.ImagetoBitmapSource(image.GetThumbnailImage(200 * image.Width / image.Height,
                                                          200,
                                                          null, IntPtr.Zero));

                // Calculate and set average brightness of this WBImage
                CalculateAverageBrightness(image);
            }

            // Set background color for UI
            int backgroundBrightness = (int)Math.Round(AverageBrightness * 255);
            BackgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness));

            // Set creation date
            AddedDate = DateTime.Now;

            // Set enabled true by default
            IsEnabled = true;

            // Set the path
            Path = path;
        }

        [JsonConstructor]
        public WBImage(float averageBrightness, bool isEnabled, DateTime addedDate, SolidColorBrush backgroundColor, string path)
        {
            // TODO: handle exceptions
            using (Image image = Image.FromFile(path))
            {

                if (image.Width >= image.Height)
                    Thumbnail = Helpers.ImagetoBitmapSource(image.GetThumbnailImage(200,
                                                         200 * image.Height / image.Width,
                                                         null, IntPtr.Zero));

                // If image height larger than width; set height of thumbnail to 200 and reduce width
                // proportionally
                else Thumbnail = Helpers.ImagetoBitmapSource(image.GetThumbnailImage(200 * image.Width / image.Height,
                                                          200,
                                                          null, IntPtr.Zero));
            }
            AverageBrightness = averageBrightness;
            IsEnabled = isEnabled;
            AddedDate = addedDate;
            BackgroundColor = backgroundColor;
            Path = path;
        }

        /// <summary>
        /// Sets and returns average brightness level (from fully black at 0.0 to fully white at 1.0) of this
        /// WBImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Average brightness level of given bitmap image</returns>
        public float CalculateAverageBrightness(Bitmap image)
        {
            // Brightness value to be summed and averaged
            AverageBrightness = 0;

            // Get log of width and height of image; to be used when looping over pixels to
            // increase efficiency
            // (Rather than looping over every pixel, using the log will allow taking a sample
            //  of only pixels on every log(width)th column and log(height)th row)
            int widthLog = Convert.ToInt32(Math.Log(image.Width));
            int heightLog = Convert.ToInt32(Math.Log(image.Height));

            // Loop over image's pixels (taking only the logged sample as described above)
            for (int x = 0; x < image.Width; x += widthLog)
            {
                for (int y = 0; y < image.Height; y += heightLog)
                {
                    // For every sampled pixel, get the color value of the pixel and add its brightness to
                    // the running sum
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    AverageBrightness += pixelColor.GetBrightness();
                }
            }

            // Divide summed brightness by the number of pixels sampled to get the average
            AverageBrightness /= (image.Width / widthLog) * (image.Height / heightLog);

            return AverageBrightness;
        }
    }
}