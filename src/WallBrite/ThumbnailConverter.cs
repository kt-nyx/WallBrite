using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    public class ThumbnailConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Convert value to base64 string
            var base64 = (string)reader.Value;
            // Convert base64 string to byte array
            byte[] byteArray = Convert.FromBase64String(base64);

            BitmapImage bitmap;
            // Create a memory stream holding the byte array
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                ms.Position = 0;
                // Create a BitmapImage source from the array in the stream
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
            }

            return bitmap;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Write bitmapimage as byte array to json
            writer.WriteValue(Helpers.BitmapImageToByteArray((BitmapImage) value));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BitmapImage);
        }
    }
}
