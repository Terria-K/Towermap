using System.Numerics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class DrawUtils 
{
    public static void Line(Batch batch, Vector2 position, float angle, float length, Color color) 
    {
        batch.Draw(Resource.Pixel, position, color, new Vector2(length, 1), Vector2.Zero, angle, layerDepth: 1);
    }
    public static void Line(Batch batch, Vector2 start, Vector2 end, Color color) 
    {
        Line(batch, start, MathUtils.Angle(start, end), Vector2.Distance(start, end), color);
    }
    public static void Rect(Batch batch, Vector2 position, Color color, Vector2 scale, Vector2 origin) 
    {
        batch.Draw(Resource.Pixel, position - origin, color, scale);
    }
}