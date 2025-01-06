using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public static class DrawUtils 
{
    public static void Rect(Batch batch, Vector2 position, Color color, Vector2 scale, Vector2 origin) 
    {
        batch.Draw(Resource.Pixel, position - origin, color, scale);
    }
}