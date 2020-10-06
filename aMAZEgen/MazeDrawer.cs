using System;
using System.Drawing;
using System.IO;
using Point = System.Drawing.Point;

namespace aMAZEgen
{
    public static class MazeDrawer
    {
        private const string TileSet = "tileset";
        private const string Ext = ".png";

        public static Bitmap DrawMaze(MazePiece[,] maze, double decorChance)
        {
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
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    Image tile;
                    var success = false;
                    try
                    {
                        tile = new Bitmap(Path.Combine(TileSet, (int) maze[i, j] + Ext));
                        success = true;
                    }
                    catch (Exception e)
                    {
                        var bitmap = new Bitmap(reference.Width, reference.Height);
                        var placeholder = Graphics.FromImage(bitmap);
                        placeholder.FillRectangle(Brushes.Black, reference);
                        placeholder.DrawString(((int) maze[i, j]).ToString(), new Font("Consolas", 8),
                            Brushes.White, new PointF(0, 0));
                        placeholder.Flush();
                        tile = bitmap;
                    }

                    graphics.DrawImage(tile, new Point(reference.Width * i, reference.Height * j));

                    if (!success) continue;

                    var random = new Random();
                    if (random.NextDouble() > decorChance) continue;
                    if (maze[i, j].HasFlag(MazePiece.Sign)) continue;
                    var images = Directory.GetFiles(maze[i, j] == 0
                        ? Path.Combine(TileSet, "decor", "empty")
                        : Path.Combine(TileSet, "decor", "path"));

                    var decoration = new Bitmap(images[random.Next(images.Length)]);
                    graphics.DrawImage(decoration, new Point(reference.Width * i, reference.Height * j));
                }
            }

            return image;
        }
    }
}