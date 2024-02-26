using System;
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
    private Transform rectTransform;
    private Actor actor;
    private float transparency;
    private Rectangle rect;

    public SpriteRenderer Sprite => renderer;

    public LevelActor(Texture texture, Actor actor, Quad quad) 
    {
        Depth = -3;
        this.actor = actor;
        renderer = new SpriteRenderer(texture, quad); 
        renderer.Origin = actor.Origin;
        this.texture = texture;
        AddComponent(renderer);
        rectTransform = new Transform();
        AddTransform(rectTransform);
        rectTransform.Scale = new Vector2(20, 20);
    }

    public override void Ready()
    {
        base.Ready();
        rect = new Rectangle((int)Transform.PosX, (int)Transform.PosY, actor.Width, actor.Height);
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;
        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

        if (IsColliding(gridX + (int)actor.Origin.X, gridY + (int)actor.Origin.Y)) 
        {
            transparency = 0.7f;
            var scene = Scene as EditorScene;
            if (Input.InputSystem.Mouse.RightButton.IsPressed && !scene.HasRemovedEntity) 
            {
                DestroySelf();
                scene.HasRemovedEntity = true;
            }
        }
        else
        {
            transparency = 0.3f;
        }
    }

    public bool IsColliding(int x, int y) 
    {
        return rect.Contains(x, y);
    }

    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        actor.OnRender?.Invoke(this, Position, spriteBatch);
        DrawUtils.Rect(spriteBatch, Position, Color.Yellow * transparency, new Vector2(actor.Width, actor.Height), actor.Origin);
        base.Draw(buffer, spriteBatch);
    }
}