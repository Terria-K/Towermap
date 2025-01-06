using System;
using System.Numerics;
using System.Collections.Generic;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;

namespace Towermap;

public class LevelActor : Entity
{
    public Actor Data;
    public ulong ID;

    public TextureQuad TextureQuad;
    public Dictionary<string, object> CustomData;
    public int Width;
    public int Height;
    public List<Vector2> Nodes;
    public bool RenderFlipped;
    public bool Selected;

    private Texture texture;
    private Transform rectTransform;
    private float transparency;
    private Rectangle rect;
    private bool isHeld;
    private Vector2 lastHold;
    

    public LevelActor(Texture texture, Actor actor, TextureQuad quad, ulong id) 
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
        if (actor.HasNodes) 
        {
            Nodes = new List<Vector2>();
        }
    }

    public void AddNode(Vector2 position) 
    {
        Nodes.Add(position);
    }

    public void RemoveNode(Vector2 position) 
    {
        Nodes.Remove(position);
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
        return new LevelActorInfo(ID, Data.Name, (int)PosX, (int)PosY, values, Nodes?.ToArray());
    }

    public override void Ready()
    {
        base.Ready();
        rect = new Rectangle((int)Transform.PosX, (int)Transform.PosY, Width, Height);
    }

    public override void Update(double delta)
    {
        base.Update(delta);
        rect.X = (int)Transform.PosX;
        rect.Y = (int)Transform.PosY;
        rect.Width = Width;
        rect.Height = Height;
        var scene = Scene as EditorScene;
        if (scene.CurrentLayer != Layers.Entities) 
        {
            transparency = 0.1f;
            return;
        }

        int x = Input.Mouse.X;
        int y = Input.Mouse.Y;
        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

        if (scene.ToolSelected == Tool.Node) 
        {
            if (Nodes != null && Nodes.Count > 0) 
            {
                var node = Nodes[^1];
                var nodeRect = new Rectangle((int)node.X, (int)node.Y, Width, Height);

                if (nodeRect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y)) 
                {
                    if (Input.Mouse.RightButton.Pressed && !scene.HasRemovedEntity) 
                    {
                        RemoveNode(node);
                    }
                }
            }

            return;
        }

        if (isHeld) 
        {
            if (Input.Mouse.LeftButton.Released) 
            {
                isHeld = false;
                // Possible negative numbers should be resolved
                if (Transform.PosX < 0)
                    Transform.PosX = Transform.PosX + 320;
                if (Transform.PosY < 0)
                    Transform.PosY = Transform.PosY + 240;
                return;
            }
            int snapX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize) + lastHold.X) / 5.0f) * 5.0f);
            int snapY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize) + lastHold.Y) / 5.0f) * 5.0f);
            Transform.PosX = snapX % 320;
            Transform.PosY = snapY % 240;
            return;
        }

        if (Selected) 
        {
            if (Input.Keyboard.IsPressed(KeyCode.Left)) 
            {
                PosX = MathUtils.Wrap(PosX - 5, 0, 320);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Right)) 
            {
                PosX = MathUtils.Wrap(PosX + 5, 0, 320);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Up)) 
            {
                PosY = MathUtils.Wrap(PosY - 5, 0, 240);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Down)) 
            {
                PosY = MathUtils.Wrap(PosY + 5, 0, 240);
            }
        }

        if (rect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y)) 
        {
            if (scene.ToolSelected == Tool.Rect) 
            {
                transparency = 0.7f;
                if (Input.Mouse.RightButton.Pressed && !scene.HasRemovedEntity) 
                {
                    scene.RemoveActor(this);
                }
                if (Input.Mouse.LeftButton.Pressed) 
                {
                    scene.SelectLevelActor(this);
                    isHeld = true;
                    lastHold = new Vector2(
                        (Transform.PosX - Data.Origin.X) - ((x - WorldUtils.WorldX) * 0.5f) + Data.Origin.X,
                        (Transform.PosY - Data.Origin.Y) - ((y - WorldUtils.WorldY) * 0.5f) + Data.Origin.Y
                    );
                }
            }
            return;
        }

        transparency = 0.3f;
    }

    public override void Draw(Batch spriteBatch)
    {
        // Render the Entity
        Color color = Color.Yellow * transparency;
        if (Selected) 
        {
            color = Color.Yellow * (transparency + 0.3f);
        }
        Data.OnRender?.Invoke(this, Position, spriteBatch);
        DrawUtils.Rect(spriteBatch, Position, color, new Vector2(Width, Height), Data.Origin);
        spriteBatch.Draw(TextureQuad, new Vector2(PosX, PosY), Color.White, Vector2.One, Data.Origin);
        if (PosX - Data.Origin.X < Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX + 320, PosY), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Draw(Data.Texture, new Vector2(PosX + 320, PosY), 
                Color.White, Vector2.One, Data.Origin);
        }

        if (PosX - Data.Origin.X > 320 - Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX - 320, PosY), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Draw(Data.Texture, new Vector2(PosX -320, PosY), 
                Color.White, Vector2.One, Data.Origin);
        }

        if (PosY - Data.Origin.Y < Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY + 240), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Draw(Data.Texture, new Vector2(PosX, PosY + 240), 
                Color.White, Vector2.One, Data.Origin);
        }

        if (PosY - Data.Origin.Y > 240 - Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY - 240), color, new Vector2(Width, Height), Data.Origin);
            spriteBatch.Draw(Data.Texture, new Vector2(PosX, PosY - 240), 
                Color.White, Vector2.One, Data.Origin);
        }

        // Render the Nodes
        var scene = Scene as EditorScene;
        if (Data.HasNodes) 
        {
            // Render the potential node to be place
            var opacity = scene.ToolSelected != Tool.Node ? 0.3f : 1f;
            if (scene.ToolSelected == Tool.Node && Selected)
            {
                int x = Input.Mouse.X;
                int y = Input.Mouse.Y;
                int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
                int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

                
                var start = Nodes.Count > 0 ? Nodes[^1] : Position;
                var end = new Vector2(gridX, gridY);

                DrawUtils.Line(spriteBatch, start, end, Color.White * 0.6f);
                spriteBatch.Draw(Data.Texture, end, 
                    Color.White * 0.4f, Vector2.One, Data.Origin);
            }


            // Render all the nodes
            for (int i = 0; i < Nodes.Count; i++) 
            {
                if (i == 0) 
                {
                    var start = Position;
                    var end = Nodes[i];
                    DrawUtils.Line(spriteBatch, start, end, Color.White * opacity);
                    if (Nodes.Count == 1) 
                    {
                        spriteBatch.Draw(Data.Texture, end, 
                            Color.White * 0.4f, Vector2.One, Data.Origin);
                    }
                }
                else if (i == Nodes.Count - 1) 
                {
                    var start = Nodes[i - 1];
                    var end = Nodes[i];

                    DrawUtils.Line(spriteBatch, start, end, Color.White * opacity);
                    spriteBatch.Draw(Data.Texture, end, 
                        Color.White * 0.4f, Vector2.One, Data.Origin);
                }
                else 
                {
                    var start = Nodes[i - 1];
                    var end = Nodes[i];

                    DrawUtils.Line(spriteBatch, start, end, Color.White * opacity);
                }
            }
        }
        base.Draw(spriteBatch);
    }
}
