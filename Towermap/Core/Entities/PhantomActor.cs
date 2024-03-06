using System;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

namespace Towermap;


public class PhantomActor : Entity 
{
    private Actor actor;
    private ActorManager manager;

    public PhantomActor(ActorManager manager) 
    {
        Depth = -3;
        this.manager = manager;
    }

    public void SetActor(Actor actor) 
    {
        this.actor = actor;
    }

    public override void Update(double delta)
    {
        if (actor == null)
            return;
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;
        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        Transform.PosX = gridX;
        Transform.PosY = gridY;
        base.Update(delta);
    }

    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        if (actor == null)
            return;
        base.Draw(buffer, spriteBatch);
        DrawUtils.Rect(spriteBatch, Transform.Position, Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
        spriteBatch.Add(actor.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Color.White * 0.7f, Vector2.One, actor.Origin, Transform.WorldMatrix);

        if (Transform.PosX - actor.Origin.X < actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX + 320, Transform.PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Add(actor.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(320, 0), 
                Color.White * 0.7f, Vector2.One, actor.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosX - actor.Origin.X > 320 - actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX - 320, Transform.PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Add(actor.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(-320, 0), 
                Color.White * 0.7f, Vector2.One, actor.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosY - actor.Origin.Y < actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX, Transform.PosY + 240), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Add(actor.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(0, 240), 
                Color.White * 0.7f, Vector2.One, actor.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosY - actor.Origin.Y > 240 - actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX, Transform.PosY - 240), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Add(actor.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(0, -240), 
                Color.White * 0.7f, Vector2.One, actor.Origin, Transform.WorldMatrix);
        }
    }

    public LevelActor PlaceActor(Scene scene) 
    {
        if (this.actor == null)
            return null;
        ulong id = manager.GetID();
        var actor = new LevelActor(Resource.TowerFallTexture, this.actor, this.actor.Texture, id);
        actor.PosX = PosX;
        actor.PosY = PosY;
        scene.Add(actor);
        return actor;
    }
}