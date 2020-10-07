using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Point = System.Drawing.Point;

namespace aMAZEgen
{
    public static class MazeDrawer
    {
        private const string TileSet = "tileset";
        private const string Ext = ".png";
        private static Dictionary<int, Image> _tileDictionary;

        private static readonly Dictionary<int, int> TileAliases = new Dictionary<int, int>
        {
            [16] = 0 //Convert ignored tiles to grass.
        };

        public static Bitmap DrawMaze(MazePiece[,] maze, double decorChance, int seed, bool debug = false)
        {
            _tileDictionary = new Dictionary<int, Image>();

            Rectangle reference;
            try
            {
                var bitmap = new Bitmap(Path.Combine(TileSet, "0" + Ext));
                reference = new Rectangle(Point.Empty, bitmap.Size);
            }
            catch (Exception e)
            {
                reference = new Rectangle(Point.Empty, new Size(32, 32));
            }

            var image = new Bitmap(
                maze.GetLength(0) * reference.Width,
                maze.GetLength(1) * reference.Height);


            using var graphics = Graphics.FromImage(image);
            var random = new Random(seed);
            for (var x = 0; x < maze.GetLength(0); x++)
            for (var y = 0; y < maze.GetLength(1); y++)
            {
                var tileId = (int) maze[x, y];
                var success = false;
                if (TileAliases.ContainsKey(tileId))
                {
                    tileId = TileAliases[tileId];
                    success = true;
                }

                if (!_tileDictionary.ContainsKey(tileId))
                {
                    if (debug)
                    {
                        _tileDictionary[tileId] = DrawDebugTile(reference, tileId);
                    }
                    else
                    {
                        try
                        {
                            _tileDictionary[tileId] = new Bitmap(Path.Combine(TileSet, tileId + Ext));
                            success = true;
                        }
                        catch (Exception)
                        {
                            _tileDictionary[tileId] = DrawDebugTile(reference, tileId);
                        }
                    }
                }
                else
                {
                    success = true;
                }

                graphics.DrawImage(_tileDictionary[tileId], new Point(reference.Width * x, reference.Height * y));

                if (!success || debug) continue;

                var nextDouble = random.NextDouble();
                var b = nextDouble > decorChance;
                if (b) continue;
                if (((MazePiece) tileId).HasFlag(MazePiece.Sign)) continue;
                var images = Directory.GetFiles(tileId == 0
                    ? Path.Combine(TileSet, "decor", "empty")
                    : Path.Combine(TileSet, "decor", "path"));

                var decoration = new Bitmap(images[random.Next(images.Length)]);
                graphics.DrawImage(decoration, new Point(reference.Width * x, reference.Height * y));
            }

            return image;
        }

        private static Bitmap DrawDebugTile(Rectangle size, int tileId)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            var placeholder = Graphics.FromImage(bitmap);
            placeholder.FillRectangle(Brushes.Black, size);
            placeholder.DrawString(tileId.ToString(), new Font("Consolas", 8),
                Brushes.White, new PointF(0, 0));
            placeholder.Flush();
            return bitmap;
        }
    }
}