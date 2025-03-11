using System;
using System.Numerics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;


public sealed class TileRect 
{
    public enum Type { Place, Remove, Move }
    public Point StartPos;
    public Rectangle ResultRect;
    public bool Started;
    public Type ButtonType;

    public void Start(int x, int y, Type buttonType) 
    {
        StartPos = new Point(x, y);
        ResultRect = Rectangle.Zero;
        ButtonType = buttonType;
        Started = true;
    }

    public void Update(int x, int y) 
    {
        int rx = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
        int ry = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);

        int lx = Math.Min(StartPos.X, rx);
        int ly = Math.Min(StartPos.Y, ry);
        int lw = Math.Max(StartPos.X, rx);
        int lh = Math.Max(StartPos.Y, ry);

        ResultRect = new Rectangle(lx, ly, lw - lx + 10, lh - ly + 10);
    }

    public void Draw(Batch spriteBatch) 
    {
        if (!Started) 
        {
            return;
        }

        Color color = ButtonType switch {
            Type.Place => Color.Green * 0.5f,
            Type.Remove => new Color(1f, 0f, 0f, 0.5f),
            Type.Move => Color.Yellow * 0.5f,
            _ => throw new InvalidOperationException()
        };

        DrawUtils.Rect(spriteBatch, new Vector2(ResultRect.X, ResultRect.Y), color, new Vector2(ResultRect.Width, ResultRect.Height), Vector2.Zero);
    }
}