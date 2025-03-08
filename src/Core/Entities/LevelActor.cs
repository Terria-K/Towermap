using System;
using System.Numerics;
using System.Collections.Generic;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;
using ImGuiNET;

namespace Towermap;

public class LevelActor : Entity
{
    public Actor Data;
    public ulong ID;

    public Dictionary<string, object> CustomData;
    public int Width;
    public int Height;
    public Vector2 Size => new Vector2(Width, Height);
    public List<Vector2> Nodes;
    public bool RenderFlipped;
    public bool Selected;

    public TextureQuad TextureQuad;
    private Texture texture;
    private Transform rectTransform;
    private float transparency;
    private Rectangle rect;
    private bool isHeld;
    private Vector2 lastHold;
    private bool isHovering;
    private Vector2 lastPos;
    

    public LevelActor(Texture texture, Actor actor, ulong id) 
    {
        Depth = -3;
        this.Data = actor;
        this.texture = texture;
        rectTransform = new Transform();
        AddTransform(rectTransform);
        rectTransform.Scale = new Vector2(20, 20);
        TextureQuad = actor.Texture;
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
        var scene = Scene as EditorScene;
        scene.CommitHistory();
        Nodes.Add(position);
    }

    public void RemoveNode(Vector2 position) 
    {
        var scene = Scene as EditorScene;
        scene.CommitHistory();
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

        var io = ImGui.GetIO();

        int x = (int)io.MousePos.X;
        int y = (int)io.MousePos.Y;

        int gridX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int gridY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

        if (scene.ToolSelected == Tool.Node && Selected) 
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
                if (Position == lastPos)
                {
                    return;
                }
                var tempPos = Position;
                Position = lastPos;
                scene.CommitHistory();
                Position = tempPos;
                // Possible negative numbers should be resolved
                if (Transform.PosX < 0)
                {
                    Transform.PosX = Transform.PosX + WorldUtils.WorldWidth;
                }
                if (Transform.PosY < 0)
                {
                    Transform.PosY = Transform.PosY + WorldUtils.WorldHeight;
                }
                return;
            }
            int snapX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize) + lastHold.X) / 5.0f) * 5.0f);
            int snapY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize) + lastHold.Y) / 5.0f) * 5.0f);
            Transform.PosX = snapX % WorldUtils.WorldWidth;
            Transform.PosY = snapY % WorldUtils.WorldHeight;
            return;
        }

        if (Selected && !ImGui.GetIO().WantTextInput) 
        {
            if (Input.Keyboard.IsPressed(KeyCode.Left)) 
            {
                scene.CommitHistory();
                PosX = MathUtils.Wrap(PosX - 5, 0, WorldUtils.WorldWidth);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Right)) 
            {
                scene.CommitHistory();
                PosX = MathUtils.Wrap(PosX + 5, 0, WorldUtils.WorldWidth);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Up)) 
            {
                scene.CommitHistory();
                PosY = MathUtils.Wrap(PosY - 5, 0, WorldUtils.WorldHeight);
            }
            else if (Input.Keyboard.IsPressed(KeyCode.Down)) 
            {
                scene.CommitHistory();
                PosY = MathUtils.Wrap(PosY + 5, 0, WorldUtils.WorldHeight);
            }
        }

        if (InBounds(gridX, gridY) && 
            (rect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y) ||
            rect.Contains(gridX + (int)Data.Origin.X + (int)WorldUtils.WorldWidth, gridY + (int)Data.Origin.Y) ||
            rect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y + (int)WorldUtils.WorldHeight) ||
            rect.Contains(gridX + (int)Data.Origin.X - (int)WorldUtils.WorldWidth, gridY + (int)Data.Origin.Y) ||
            rect.Contains(gridX + (int)Data.Origin.X, gridY + (int)Data.Origin.Y - (int)WorldUtils.WorldHeight)))
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
                    lastPos = Position;
                    lastHold = new Vector2(
                        (Transform.PosX - Data.Origin.X) - ((x - WorldUtils.WorldX) * 0.5f) + Data.Origin.X,
                        (Transform.PosY - Data.Origin.Y) - ((y - WorldUtils.WorldY) * 0.5f) + Data.Origin.Y
                    );
                }

                isHovering = true;
            }
            return;
        }

        isHovering = false;
        transparency = 0.3f;
    }

    private bool InBounds(int x, int y) 
    {
        return x >= 0 && x <= WorldUtils.WorldWidth && y >= 0 && y <= WorldUtils.WorldHeight;
    }

    public override void Draw(Batch spriteBatch)
    {
        // Render the Entity
        Color color = Color.Yellow * transparency;
        if (Selected) 
        {
            color = Color.Green * (transparency + 0.3f);
        }
        DrawUtils.Rect(spriteBatch, Position, color, new Vector2(Width, Height), Data.Origin);
        Data.OnRender?.Invoke(this, Data, Position, Size, spriteBatch, Color.White);
        spriteBatch.Draw(TextureQuad, new Vector2(PosX, PosY), Color.White, Vector2.One, Data.Origin);

        if (isHovering) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX - 3, PosY - 3), color * 0.4f, new Vector2(Width + 6, Height + 6), Data.Origin);
        }

        if (PosX - Data.Origin.X < Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX + WorldUtils.WorldWidth, PosY), color, new Vector2(Width, Height), Data.Origin);
            Data.OnRender?.Invoke(this, Data, new Vector2(PosX + WorldUtils.WorldWidth, PosY), Size, spriteBatch, Color.White);
            spriteBatch.Draw(TextureQuad, new Vector2(PosX + WorldUtils.WorldWidth, PosY), 
                Color.White, Vector2.One, Data.Origin);

            if (isHovering) 
            {
                DrawUtils.Rect(spriteBatch, new Vector2(PosX + WorldUtils.WorldWidth - 3, PosY - 3), color * 0.4f, new Vector2(Width + 6, Height + 6), Data.Origin);
            }
        }

        if (PosX - Data.Origin.X > WorldUtils.WorldWidth - Width) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX - WorldUtils.WorldWidth, PosY), color, new Vector2(Width, Height), Data.Origin);
            Data.OnRender?.Invoke(this, Data, new Vector2(PosX - WorldUtils.WorldWidth, PosY), Size, spriteBatch, Color.White);
            spriteBatch.Draw(TextureQuad, new Vector2(PosX - WorldUtils.WorldWidth, PosY), 
                Color.White, Vector2.One, Data.Origin);

            if (isHovering) 
            {
                DrawUtils.Rect(spriteBatch, new Vector2(PosX - WorldUtils.WorldWidth - 3, PosY - 3), color * 0.4f, new Vector2(Width + 6, Height + 6), Data.Origin);
            }
        }

        if (PosY - Data.Origin.Y < Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY + WorldUtils.WorldHeight), color, new Vector2(Width, Height), Data.Origin);
            Data.OnRender?.Invoke(this, Data, new Vector2(PosX, PosY + WorldUtils.WorldHeight), Size, spriteBatch, Color.White);
            spriteBatch.Draw(TextureQuad, new Vector2(PosX, PosY + WorldUtils.WorldHeight), 
                Color.White, Vector2.One, Data.Origin);

            if (isHovering) 
            {
                DrawUtils.Rect(spriteBatch, new Vector2(PosX - 3, PosY - 237), color * 0.4f, new Vector2(Width + 6, Height + 6), Data.Origin);
            }
        }

        if (PosY - Data.Origin.Y > WorldUtils.WorldHeight - Height) 
        {
            DrawUtils.Rect(spriteBatch, new Vector2(PosX, PosY - WorldUtils.WorldHeight), color, new Vector2(Width, Height), Data.Origin);
            Data.OnRender?.Invoke(this, Data, new Vector2(PosX, PosY - WorldUtils.WorldHeight), Size, spriteBatch, Color.White);
            spriteBatch.Draw(TextureQuad, new Vector2(PosX, PosY - WorldUtils.WorldHeight), 
                Color.White, Vector2.One, Data.Origin);

            if (isHovering) 
            {
                DrawUtils.Rect(spriteBatch, new Vector2(PosX - 3, PosY - 237), color * 0.4f, new Vector2(Width + 6, Height + 6), Data.Origin);
            }
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
                Data.OnRender?.Invoke(this, Data, end, Size, spriteBatch, Color.White * 0.4f);
                spriteBatch.Draw(TextureQuad, end, 
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
                        Data.OnRender?.Invoke(this, Data, end, Size, spriteBatch, Color.White * 0.4f);
                        spriteBatch.Draw(TextureQuad, end, 
                            Color.White * 0.4f, Vector2.One, Data.Origin);
                    }
                }
                else if (i == Nodes.Count - 1) 
                {
                    var start = Nodes[i - 1];
                    var end = Nodes[i];

                    DrawUtils.Line(spriteBatch, start, end, Color.White * opacity);
                    Data.OnRender?.Invoke(this, Data, end, Size, spriteBatch, Color.White * 0.4f);
                    spriteBatch.Draw(TextureQuad, end, 
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

    public LevelActor Clone()
    {
        return new LevelActor(texture, Data, ID) 
        {
            CustomData = CustomData,
            Nodes = Nodes == null ? null : new List<Vector2>(Nodes),
            Width = Width,
            Height = Height,
            Selected = Selected,
            Scene = Scene,
            Position = Position
        };
    }
}
