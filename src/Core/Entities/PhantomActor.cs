using System;
using System.Numerics;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;

namespace Towermap;


public class PhantomActor
{
    private Actor actor;
    public Vector2 Position;
    public float PosX => Position.X;
    public float PosY => Position.Y;
    public int Width => actor.Width;
    public int Height => actor.Height;
    public float OriginX => actor.Origin.X;
    public float OriginY => actor.Origin.Y;
    public bool ResizeableX => actor.ResizeableX;
    public bool ResizeableY => actor.ResizeableY;
    public Scene Scene;

    public bool Active { get; set; }

    public PhantomActor(Scene scene) 
    {
        Scene = scene;
    }

    public void SetActor(Actor actor) 
    {
        this.actor = actor;
    }

    public void Update(double delta)
    {
        if (actor == null || (Scene as EditorScene).ToolSelected != Tool.Pen || !Active)
            return;
        var io = ImGui.GetIO();
        int x = (int)io.MousePos.X;
        int y = (int)io.MousePos.Y;
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
        actor.OnRender?.Invoke(null, actor, Position, new Vector2(actor.Width, actor.Height), spriteBatch, Color.Yellow * 0.4f);
        spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY), 
            Color.White * 0.4f, Vector2.One, actor.Origin);

        if (PosX - actor.Origin.X < actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX + WorldUtils.WorldWidth, PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            actor.OnRender?.Invoke(null, actor, new Vector2(PosX + WorldUtils.WorldWidth, PosY), new Vector2(actor.Width, actor.Height), spriteBatch, Color.Yellow * 0.4f);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX + WorldUtils.WorldWidth, PosY), 
                Color.Yellow * 0.4f, Vector2.One, actor.Origin);
        }

        if (PosX - actor.Origin.X > WorldUtils.WorldWidth - actor.Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX - WorldUtils.WorldWidth, PosY), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            actor.OnRender?.Invoke(null, actor, new Vector2(PosX - WorldUtils.WorldWidth, PosY), new Vector2(actor.Width, actor.Height), spriteBatch, Color.Yellow * 0.4f);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX - WorldUtils.WorldWidth, PosY), 
                Color.Yellow * 0.4f, Vector2.One, actor.Origin);
        }

        if (PosY - actor.Origin.Y < actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY + WorldUtils.WorldHeight), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            actor.OnRender?.Invoke(null, actor, new Vector2(PosX, PosY + WorldUtils.WorldHeight), new Vector2(actor.Width, actor.Height), spriteBatch, Color.Yellow * 0.4f);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY + WorldUtils.WorldHeight), 
                Color.Yellow * 0.4f, Vector2.One, actor.Origin);
        }

        if (PosY - actor.Origin.Y > WorldUtils.WorldHeight - actor.Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY - WorldUtils.WorldHeight), Color.Yellow * 0.2f, new Vector2(actor.Width, actor.Height), actor.Origin);
            actor.OnRender?.Invoke(null, actor, new Vector2(PosX, PosY - WorldUtils.WorldHeight), new Vector2(actor.Width, actor.Height), spriteBatch, Color.Yellow * 0.4f);
            spriteBatch.Draw(actor.Texture, new Vector2(PosX, PosY - WorldUtils.WorldHeight), 
                Color.Yellow * 0.4f, Vector2.One, actor.Origin);
        }
    }
}