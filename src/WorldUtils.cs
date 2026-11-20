using System;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public static class WorldUtils
{
    public static float WorldX;
    public static float WorldY;
    public static double WorldSize = 2;

    public static float WorldWidth = 320;
    public static float WorldHeight = 240;

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
        return (int)Math.Floor(num / TileSize);
    }

    public static bool InBounds(Point point)
    {
        return InBounds(point.X, point.Y);
    }

    public static bool InBounds(int gridX, int gridY)
    {
        return gridX > -1 && gridY > -1 && gridX < WorldWidth / 10 && gridY < WorldHeight / 10;
    }

    public static void TurnStandard()
    {
        WorldWidth = 320;
    }

    public static void TurnWide()
    {
        WorldWidth = 420;
    }

    public static float ToWorldX(float screenX) => (float)((screenX - WorldX) / WorldSize);
    public static float ToWorldY(float screenY) => (float)((screenY - WorldY) / WorldSize);

    public static int Snap(float worldPos, float step) => (int)(Math.Floor(worldPos / step) * step);
}
