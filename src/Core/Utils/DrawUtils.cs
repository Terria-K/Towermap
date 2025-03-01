using System.Numerics;
using System.Runtime.CompilerServices;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class DrawUtils 
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Line(Batch batch, Vector2 position, float angle, float length, Color color) 
    {
        batch.Draw(Resource.Pixel, position, color, new Vector2(length, 1), Vector2.Zero, angle, layerDepth: 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Line(Batch batch, Vector2 start, Vector2 end, Color color) 
    {
        Line(batch, start, MathUtils.Angle(start, end), Vector2.Distance(start, end), color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rect(Batch batch, Vector2 position, Color color, Vector2 scale, Vector2 origin) 
    {
        batch.Draw(Resource.Pixel, position - origin, color, scale);
    }
}