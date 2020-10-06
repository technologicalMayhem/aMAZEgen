using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace aMAZEgen
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

        private void MakeMaze_OnClick(object sender, RoutedEventArgs e)
        {
            var lastChance = float.Parse(LastChance.Text) / 100;
            var decorChance = float.Parse(DecorChance.Text) / 100;
            var maze = MazeGenerator.GenerateMaze(new Size(20, 20), 0, 0, true, lastChance);
            var image = MazeDrawer.DrawMaze(maze, decorChance);
            Image.Source = Convert(image);
        }

        private static BitmapImage Convert(Image bitmap)
        {
            var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            memory.Dispose();

            return bitmapImage;
        }
    }
}