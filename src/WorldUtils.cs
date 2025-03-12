using System;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public static class WorldUtils 
{
    public static int WorldX = 285;
    public static int WorldY = 90;

    public static float WorldWidth = 320;
    public static float WorldHeight = 240;
    public const double WorldSize = 2;

    public const int TileSize = 10;

    public static Vector2 Opposite(Vector2 vec)
    {
        float halfWidth = WorldWidth * 0.5f;
        return new Vector2(halfWidth + (halfWidth - vec.X), vec.Y);
    }

    public static Point Opposite(int gridX, int gridY)
    {
        int halfWidth = ((int)WorldWidth / 2) / 10;
        return new Point(halfWidth + (halfWidth - gridX), gridY);
    }

    public static int ToGrid(float num) 
    {
        return (int)Math.Floor(num / (5 * WorldSize));
    }

    public static bool InBounds(Point point)
    {
        return InBounds(point.X, point.Y);
    }

    public static bool InBounds(int gridX, int gridY)
    {
        return gridX > -1 && gridY > -1 && gridX < WorldWidth / 10 && gridY < WorldHeight / 10;
    }

    public static bool TurnStandard()
    {
        bool isAlready = WorldWidth == 320;
        WorldWidth = 320;
        WorldX = 285;
        return isAlready;
    }

    public static bool TurnWide()
    {
        bool isAlready = WorldHeight == 420;
        WorldWidth = 420;
        WorldX = 200;
        return isAlready;
    }
}