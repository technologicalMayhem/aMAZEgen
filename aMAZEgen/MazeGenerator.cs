using System;
using System.Collections.Generic;
using System.Drawing;

namespace aMAZEgen
{
    public static class MazeGenerator
    {
        private static List<Point> _visited;
        private static MazePiece[,] _cells;
        private static int _seed;
        private static Random _random;

        public static MazePiece[,] GenerateMaze(Size size, float chunkiness, float chunkSize, bool placeSign,
            float lastChance, int seed)
        {
            _seed = seed;
            _random = new Random(_seed);
            bool success;
            var attempts = 0;
            do
            {
                Setup(size);
                PlaceChunks(chunkiness, chunkSize);
                if (placeSign) PlaceExitSign();
                GeneratePaths(lastChance);
                success = PlaceEntranceAndExit(placeSign);

                if (++attempts > 1000) _seed++;
            } while (!success);

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
            SimplexNoise.Noise.Seed = _seed;
            var calc2D = SimplexNoise.Noise.Calc2D(_cells.GetLength(0), _cells.GetLength(1), chunkiness);
            for (var x = 0; x < calc2D.GetLength(0); x++)
            for (var y = 0; y < calc2D.GetLength(1); y++)
            {
                var f = calc2D[x, y];
                if (f < chunkSize) _cells[x, y] |= MazePiece.Ignore;
            }
        }

        private static void PlaceExitSign()
        {
            var signLowerRight = new Point(_cells.GetLength(0) - 1, _random.Next(1, _cells.GetLength(1) - 1));

            _cells[signLowerRight.X, signLowerRight.Y] = MazePiece.Sign | MazePiece.South | MazePiece.East;
            _cells[signLowerRight.X - 1, signLowerRight.Y] = MazePiece.Sign | MazePiece.South | MazePiece.West;
            _cells[signLowerRight.X, signLowerRight.Y - 1] = MazePiece.Sign | MazePiece.North | MazePiece.East;
            _cells[signLowerRight.X - 1, signLowerRight.Y - 1] = MazePiece.Sign | MazePiece.North | MazePiece.West;
        }

        private static void GeneratePaths(float lastChance)
        {
            Point start;
            var attempts = 0;
            do
            {
                start = new Point(_random.Next(_cells.GetLength(0)), _random.Next(_cells.GetLength(1)));
                attempts++;
                if (++attempts > 1000)
                {
                    return;
                }
            } while (_cells[start.X, start.Y] != 0);

            _visited.Add(start);

            var visited = _visited[_random.Next(_visited.Count)];

            // var count = 0;

            while (_visited.Count > 0)
            {
                if (_random.NextDouble() > lastChance)
                {
                    visited = _visited[_random.Next(_visited.Count)];
                }

                var freeCells = AdjacentEmptyCells(visited);

                if (freeCells.Length <= 0)
                {
                    _visited.Remove(visited);
                    if (_visited.Count > 0) visited = _visited[_random.Next(_visited.Count)];
                    continue;
                }

                var toVisit = freeCells[_random.Next(freeCells.Length)];
                var directionToGo = GetDirection(visited, toVisit);

                _cells[visited.X, visited.Y] |= directionToGo;
                _cells[toVisit.X, toVisit.Y] |= directionToGo.GetOpposite();

                _visited.Add(toVisit);
                visited = toVisit;

                // MazeDrawer.DrawMaze(_cells).Save(Path.Combine("proc", count++ + ".png"));
            }
        }

        private static bool PlaceEntranceAndExit(bool placeSign)
        {
            //Place Entrance
            var potentialEntrances = new List<Point>();
            for (var y = 0; y < _cells.GetLength(1); y++)
            {
                if (!_cells[0, y].HasFlag(MazePiece.Ignore) && _cells[0, y] > 0)
                {
                    potentialEntrances.Add(new Point(0, y));
                }
            }

            if (potentialEntrances.Count <= 0) return false;
            var entrance = potentialEntrances[_random.Next(potentialEntrances.Count)];
            _cells[entrance.X, entrance.Y] |= MazePiece.West;

            //Place Exit
            if (placeSign)
            {
                for (var i = 0; i < _cells.GetLength(1); i++)
                {
                    var mazePiece = _cells[_cells.GetLength(0) - 1, i];
                    if (mazePiece.HasFlag(MazePiece.Sign) && mazePiece.HasFlag(MazePiece.South) &&
                        mazePiece.HasFlag(MazePiece.East))
                    {
                        if (_cells[_cells.GetLength(0) - 1, i + 1] > 0 && 
                            !_cells[_cells.GetLength(0) - 1, i + 1].HasFlag(MazePiece.Ignore))
                        {
                            _cells[_cells.GetLength(0) - 1, i + 1] |= MazePiece.East;
                            break;
                        }

                        return false;
                    }
                }
            }
            else
            {
                var potentialExits = new List<Point>();
                for (var y = 0; y < _cells.GetLength(1); y++)
                {
                    if (!_cells[0, y].HasFlag(MazePiece.Ignore) && _cells[_cells.GetLength(0) - 1, y] > 0)
                    {
                        potentialExits.Add(new Point(_cells.GetLength(0) - 1, y));
                    }
                }

                if (potentialExits.Count <= 0) return false;
                var exit = potentialExits[_random.Next(potentialExits.Count)];
                _cells[exit.X, exit.Y] |= MazePiece.East;
            }

            return true;
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