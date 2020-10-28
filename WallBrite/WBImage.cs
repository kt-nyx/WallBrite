using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    [Serializable]
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
        [JsonConverter(typeof(ThumbnailConverter))]
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

                // Create and set the thumbnail
                Thumbnail = Helpers.GetThumbnailFromBitmap(image);

                // Calculate and set average brightness of this image
                AverageBrightness = CalculateAverageBrightness(image);
            }

            BackgroundColor = GetBackgroundColor();
            AddedDate = DateTime.Now;
            Path = path;

            // Defaults to being enabled when created
            IsEnabled = true;
        }

        /// <summary>
        /// Creates WBImage using data from externally created bitmap and given file path
        /// Calculates its average brightness, creates a proportionate thumbnail, sets creation date, and
        /// stores the given path to the original file
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="path"></param>
        public WBImage(Bitmap bitmap, string path)
        {
            Thumbnail = Helpers.GetThumbnailFromBitmap(bitmap);
            AverageBrightness = CalculateAverageBrightness(bitmap);
            BackgroundColor = GetBackgroundColor();
            AddedDate = DateTime.Now;
            Path = path;
            IsEnabled = true;
        }

        [JsonConstructor]
        public WBImage(float averageBrightness, bool isEnabled, DateTime addedDate, BitmapImage thumbnail, SolidColorBrush backgroundColor, string path)
        {
            Thumbnail = thumbnail;
            AverageBrightness = averageBrightness;
            IsEnabled = isEnabled;
            AddedDate = addedDate;
            BackgroundColor = backgroundColor;
            Path = path;
        }

        public SolidColorBrush GetBackgroundColor()
        {
            int backgroundBrightness = (int)Math.Round(AverageBrightness * 255);
            SolidColorBrush backgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness));
            return backgroundColor;
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
            float averageBrightness = 0;

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
                    averageBrightness += pixelColor.GetBrightness();
                }
            }

            // Divide summed brightness by the number of pixels sampled to get the average
            averageBrightness /= (image.Width / widthLog) * (image.Height / heightLog);

            return averageBrightness;
        }
    }
}