using System;
using System.Collections.Generic;
using System.Drawing;

namespace aMAZEgen
{
    public static class MazeGenerator
    {
        private static List<Point> _visited;
        private static MazePiece[,] _cells;

        public static MazePiece[,] GenerateMaze(Size size, float chunkiness, float chunkSize, bool placeSign,
            float lastChance)
        {
            Setup(size);
            PlaceChunks(chunkiness, chunkSize);
            if (placeSign) PlaceExitSign();
            GeneratePaths(lastChance);
            //Todo: If the generation of an entrance and exit fails redo the generation process.
            PlaceEntranceAndExit(placeSign);

            return _cells;
        }

        private static void Setup(Size size)
        {
            _visited = new List<Point>();
            _cells = new MazePiece[size.Width, size.Height];

            for (var i = 0; i < _cells.GetLength(0); i++)
            {
                for (var j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j] = 0;
                }
            }
        }

        private static void PlaceChunks(float chunkiness, float chunkSize)
        {
            //TODO: Implement chuck placement.
            //Create natural looking chunks
            //Maybe perlin noise?
        }

        private static void PlaceExitSign()
        {
            var random = new Random();
            var signLowerRight = new Point(_cells.GetLength(0) - 1, random.Next(1, _cells.GetLength(1) - 1));

            _cells[signLowerRight.X, signLowerRight.Y] = MazePiece.Sign | MazePiece.South | MazePiece.East;
            _cells[signLowerRight.X - 1, signLowerRight.Y] = MazePiece.Sign | MazePiece.South | MazePiece.West;
            _cells[signLowerRight.X, signLowerRight.Y - 1] = MazePiece.Sign | MazePiece.North | MazePiece.East;
            _cells[signLowerRight.X - 1, signLowerRight.Y - 1] = MazePiece.Sign | MazePiece.North | MazePiece.West;
        }

        private static void GeneratePaths(float lastChance)
        {
            var random = new Random();
            var start = new Point(random.Next(_cells.GetLength(0)), random.Next(_cells.GetLength(1)));
            _visited.Add(start);

            var visited = _visited[random.Next(_visited.Count)];

            // var count = 0;

            while (_visited.Count > 0)
            {
                if (random.NextDouble() > lastChance)
                {
                    visited = _visited[random.Next(_visited.Count)];
                }

                var freeCells = AdjacentEmptyCells(visited);

                if (freeCells.Length <= 0)
                {
                    _visited.Remove(visited);
                    if (_visited.Count > 0) visited = _visited[random.Next(_visited.Count)];
                    continue;
                }

                var toVisit = freeCells[random.Next(freeCells.Length)];
                var directionToGo = GetDirection(visited, toVisit);

                _cells[visited.X, visited.Y] |= directionToGo;
                _cells[toVisit.X, toVisit.Y] |= directionToGo.GetOpposite();

                _visited.Add(toVisit);
                visited = toVisit;

                // MazeDrawer.DrawMaze(_cells).Save(Path.Combine("proc", count++ + ".png"));
            }
        }

        //TODO: Return boolean whether or not placement succeeded
        private static void PlaceEntranceAndExit(bool placeSign)
        {
            var random = new Random();
            _cells[0, random.Next(_cells.GetLength(1))] |= MazePiece.West;
            
            if (placeSign)
            {
                for (int i = 0; i < _cells.GetLength(1); i++)
                {
                    var mazePiece = _cells[_cells.GetLength(0) - 1, i];
                    if (mazePiece.HasFlag(MazePiece.Sign) && mazePiece.HasFlag(MazePiece.South) &&
                        mazePiece.HasFlag(MazePiece.East))
                    {
                        _cells[_cells.GetLength(0) - 1, i + 1] |= MazePiece.East;
                        break;
                    }
                }
            }
            else
            {
                _cells[_cells.GetLength(0) - 1, random.Next(_cells.GetLength(1))] |= MazePiece.East;
            }
        }


        private static MazePiece GetDirection(Point from, Point to)
        {
            var direction = new Point(from.X - to.X, from.Y - to.Y);

            if (direction.X > 0) return MazePiece.West;
            if (direction.X < 0) return MazePiece.East;
            if (direction.Y > 0) return MazePiece.North;
            if (direction.Y < 0) return MazePiece.South;

            throw new ArgumentOutOfRangeException($"{direction.X},{direction.Y} is not a valid direction.");
        }

        private static Point[] AdjacentEmptyCells(Point point)
        {
            var validCells = new List<Point>();
            var adjacentCells = new[]
            {
                new Point(point.X, point.Y + 1), //North
                new Point(point.X + 1, point.Y), //East
                new Point(point.X, point.Y - 1), //South
                new Point(point.X - 1, point.Y) //West
            };

            foreach (var cell in adjacentCells)
            {
                if (cell.X >= 0 && cell.X < _cells.GetLength(0) && cell.Y >= 0 && cell.Y < _cells.GetLength(1))
                {
                    if (_cells[cell.X, cell.Y] == 0)
                    {
                        validCells.Add(cell);
                    }
                }
            }

            return validCells.ToArray();
        }

        private static MazePiece GetOpposite(this MazePiece mazePiece)
        {
            return mazePiece switch
            {
                MazePiece.North => MazePiece.South,
                MazePiece.East => MazePiece.West,
                MazePiece.South => MazePiece.North,
                MazePiece.West => MazePiece.East,
                _ => throw new ArgumentOutOfRangeException(nameof(mazePiece), mazePiece, null)
            };
        }
    }
}