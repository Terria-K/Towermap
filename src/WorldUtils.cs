using System;

namespace Towermap;

public static class WorldUtils 
{
    public static int WorldX = 200;
    public static int WorldY = 90;

    public static float WorldWidth = 320;
    public static float WorldHeight = 240;
    public const double WorldSize = 2;

    public const int TileSize = 10;

    public static int ToGrid(float num) 
    {
        return (int)Math.Floor(num / (5 * WorldSize));
    }

    public static bool InBounds(int gridX, int gridY)
    {
        return gridX > -1 && gridY > -1 && gridX < WorldWidth / 10 && gridY < WorldHeight / 10;
    }

    public static bool TurnStandard()
    {
        bool isAlready = WorldWidth == 320;
        WorldWidth = 320;
        return isAlready;
    }

    public static bool TurnWide()
    {
        bool isAlready = WorldHeight == 420;
        WorldWidth = 420;
        return isAlready;
    }
}