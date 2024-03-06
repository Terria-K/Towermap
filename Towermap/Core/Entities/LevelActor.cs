using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class LevelActor : Entity
{
    private Texture texture;
    private Transform rectTransform;
    private float transparency;
    private Rectangle rect;
    public Actor Data;
    public ulong ID;

    public Quad TextureQuad;
    public Dictionary<string, object> CustomData;
    public int Width;
    public int Height;
    public bool RenderFlipped;

    public bool Selected;

    public LevelActor(Texture texture, Actor actor, Quad quad, ulong id) 
    {
        Depth = -3;
        this.Data = actor;
        this.TextureQuad = quad;
        this.texture = texture;
        rectTransform = new Transform();
        AddTransform(rectTransform);
        rectTransform.Scale = new Vector2(20, 20);
        this.ID = id;
        Width = actor.Width;
        Height = actor.Height;
        if (actor.CustomValues != null) 
        {
            CustomData = new Dictionary<string, object>(actor.CustomValues);
        }
        else 
        {
            CustomData = new Dictionary<string, object>();
        }
    }

    public override void Ready()
    {
        base.Ready();
        rect = new Rectangle((int)Transform.PosX, (int)Transform.PosY, Width, Height);
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;
        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

        var scene = Scene as EditorScene;
        if (scene.CurrentLayer != Layers.Entities) 
        {
            transparency = 0.1f;
            return;
        }
        if (rect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y)) 
        {
            if (scene.ToolSelected == Tool.Rect) 
            {
                transparency = 0.7f;
                if (Input.InputSystem.Mouse.RightButton.IsPressed && !scene.HasRemovedEntity) 
                {
                    scene.RemoveActor(this);
                }
                if (Input.InputSystem.Mouse.LeftButton.IsPressed) 
                {
                    scene.SelectLevelActor(this);
                }
            }
        }
        else
        {
            transparency = 0.3f;
        }
    }

    public override void Draw(CommandBuffer buffer, IBatch spriteBatch)
    {
        Color color = Color.Yellow * transparency;
        if (Selected) 
        {
            color = Color.Yellow * (transparency + 0.3f);
        }
        Data.OnRender?.Invoke(this, Position, spriteBatch);
        DrawUtils.Rect(spriteBatch, Position, color, new Vector2(Width, Height), Data.Origin);
        spriteBatch.Add(TextureQuad, this.texture, GameContext.GlobalSampler, new Vector2(0, 0), Color.White, Vector2.One, Data.Origin, Transform.WorldMatrix);
        if (Transform.PosX - Data.Origin.X < Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX + 320, Transform.PosY), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Add(Data.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(320, 0), 
                Color.White, Vector2.One, Data.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosX - Data.Origin.X > 320 - Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX - 320, Transform.PosY), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Add(Data.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(-320, 0), 
                Color.White, Vector2.One, Data.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosY - Data.Origin.Y < Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX, Transform.PosY + 240), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Add(Data.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(0, 240), 
                Color.White, Vector2.One, Data.Origin, Transform.WorldMatrix);
        }

        if (Transform.PosY - Data.Origin.Y > 240 - Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(Transform.PosX, Transform.PosY - 240), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Add(Data.Texture, Resource.TowerFallTexture, GameContext.GlobalSampler, new Vector2(0, -240), 
                Color.White, Vector2.One, Data.Origin, Transform.WorldMatrix);
        }
        base.Draw(buffer, spriteBatch);
    }

    public LevelActorInfo Save() 
    {
        Dictionary<string, object> values = new Dictionary<string, object>(CustomData);
        if (Data.ResizeableX) 
        {
            values["width"] = Width;
        }
        if (Data.ResizeableY) 
        {
            values["height"] = Height;
        }
        return new LevelActorInfo(ID, Data.Name, (int)PosX, (int)PosY, values);
    }
}

public struct LevelActorInfo(ulong id, string name, int x, int y, Dictionary<string, object> values)
{
    public ulong ID = id;
    public string Name = name;
    public int X = x;
    public int Y = y;
    public Dictionary<string, object> Values = values;
}