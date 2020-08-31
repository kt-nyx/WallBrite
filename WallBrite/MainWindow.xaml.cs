using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WallBrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            Bitmap darkImage = new Bitmap(@"D:\bright.jpg");

            Color pixelColor;
            float averageBrightness = 0;
            int widthLog = Convert.ToInt32(Math.Log(darkImage.Width));
            int heightLog = Convert.ToInt32(Math.Log(darkImage.Height));

            // Dark image average brightness test
            for (int x = 0; x < darkImage.Width; x += widthLog)
            {
                for (int y = 0; y < darkImage.Height; y+= heightLog)
                {
                    pixelColor = darkImage.GetPixel(x, y);
                    averageBrightness += pixelColor.GetBrightness();
                }
            }

            // averageBrightness /= darkImage.Width * darkImage.Height;
            averageBrightness /= (darkImage.Width / widthLog) * (darkImage.Height / heightLog);

        }
    }
}
