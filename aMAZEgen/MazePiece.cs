using System;

namespace aMAZEgen
{
    [Flags]
    public enum MazePiece
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        Ignore = 16,
        Sign = 32
    }
}