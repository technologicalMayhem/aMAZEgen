using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace aMAZEgen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageSource _normal;
        private ImageSource _debug;

        public MainWindow()
        {
            InitializeComponent();
            Noise.ValueChanged += Test_OnClick;
        }

        private void MakeMaze_OnClick(object sender, RoutedEventArgs e)
        {
            var lastChance = float.Parse(LastChance.Text) / 100;
            var decorChance = float.Parse(DecorChance.Text) / 100;
            var seed = new Random().Next();
            if (!string.IsNullOrWhiteSpace(Seed.Text))
            {
                int.TryParse(Seed.Text, out seed);
            }

            FinalSeed.Content = seed;
            
            var maze = MazeGenerator.GenerateMaze(new Size(20, 20), 0.10f, (int) Noise.Value, true, lastChance, seed);
            _normal = Convert(MazeDrawer.DrawMaze(maze, decorChance, seed));
            _debug = Convert(MazeDrawer.DrawMaze(maze, decorChance, seed, true));
            Debug_OnClick(this, new RoutedEventArgs());
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            SimplexNoise.Noise.Seed = new Random().Next();
            var calc2D = SimplexNoise.Noise.Calc2D(20, 20, 0.10f);
            var bitmap = new Bitmap(20, 20);
            var graphics = Graphics.FromImage(bitmap);
            for (var x = 0; x < calc2D.GetLength(0); x++)
            for (var y = 0; y < calc2D.GetLength(1); y++)
            {
                var f = calc2D[x, y];
                var pen = new Pen(Color.Black);
                if (f > Noise.Value) pen = new Pen(Color.White);
                graphics.DrawRectangle(pen, new Rectangle(x, y, 1, 1));
            }

            graphics.Flush();

            Image.Source = Convert(bitmap);
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

        private void Debug_OnClick(object sender, RoutedEventArgs e)
        {
            if (Debug.IsChecked != null && Debug.IsChecked.Value)
            {
                Image.Source = _debug;
            }
            else
            {
                Image.Source = _normal;
            }
        }
    }
}