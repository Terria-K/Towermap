using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Components;
using Riateu.Graphics;

namespace Towermap;

public class LevelActor : Entity
{
    private SpriteRenderer renderer;
    private Texture texture;
    private SpriteTexture rectPixel;
    private Transform rectTransform;

    public SpriteRenderer Sprite => renderer;

    public LevelActor(Texture texture, SpriteTexture quad) 
    {
        renderer = new SpriteRenderer(texture, quad); 
        this.texture = texture;
        AddComponent(renderer);
        rectTransform = new Transform();
        AddTransform(rectTransform);
        rectTransform.Scale = new Vector2(20, 20);
        rectPixel = Resource.Pixel with {
            Width = 20,
            Height = 20
        };
    }

    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        spriteBatch.Add(rectPixel, texture, GameContext.GlobalSampler, Vector2.Zero, Color.Yellow * 0.5f, rectTransform.WorldMatrix);
        base.Draw(buffer, spriteBatch);
    }
}