using System;
using System.Numerics;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;

namespace Towermap;


public class PhantomActor
{
    private Actor actor;
    private ActorManager manager;
    public Vector2 Position;
    public float PosX => Position.X;
    public float PosY => Position.Y;
    public Scene Scene;

    public bool Active { get; set; }

    public PhantomActor(Scene scene, ActorManager manager) 
    {
        Scene = scene;
        this.manager = manager;
    }

    public void SetActor(Actor actor) 
    {
        this.actor = actor;
    }

    public void Update(double delta)
    {
        if (actor == null || (Scene as EditorScene).ToolSelected != Tool.Pen || !Active)
            return;
        int x = Input.Mouse.X;
        int y = Input.Mouse.Y;
        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        Position.X = gridX;
        Position.Y = gridY;
    }

    public void Draw(Batch spriteBatch)
    {
        if (actor == null || (Scene as EditorScene).ToolSelected != Tool.Pen || !Active)
            return;
        DrawUtils.Rect(spriteBatch, Position, Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
        spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY), 
            Color.White * 0.7f, Vector2.One, actor.Origin);

        if (PosX - actor.Origin.X < actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX + 320, PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX + 320, PosY), 
                Color.White * 0.7f, Vector2.One, actor.Origin);
        }

        if (PosX - actor.Origin.X > 320 - actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX - 320, PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX - 320, PosY), 
                Color.White * 0.7f, Vector2.One, actor.Origin);
        }

        if (PosY - actor.Origin.Y < actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY + 240), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY + 240), 
                Color.White * 0.7f, Vector2.One, actor.Origin);
        }

        if (PosY - actor.Origin.Y > 240 - actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY - 240), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY - 240), 
                Color.White * 0.7f, Vector2.One, actor.Origin);
        }
    }
}