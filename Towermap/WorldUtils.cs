using System;

namespace Towermap;

public static class WorldUtils 
{
    public const int WorldX = 170;
    public const int WorldY = 90;

    public const double WorldWidth = 320;
    public const double WorldHeight = 240;
    public const double WorldSize = 2;

    public const int TileSize = 10;

    public static int ToGrid(float num) 
    {
        return (int)Math.Floor(num / (5 * WorldSize));
    }
}