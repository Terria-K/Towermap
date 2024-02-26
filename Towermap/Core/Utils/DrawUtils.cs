using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class DrawUtils 
{
    public static void Rect(IBatch batch, Vector2 position, Color color, Vector2 scale, Vector2 origin) 
    {
        batch.Add(Resource.Pixel, Resource.TowerFallTexture, GameContext.GlobalSampler, position, color, scale, origin);
    }
}