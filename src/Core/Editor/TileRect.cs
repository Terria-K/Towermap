using System;
using System.Numerics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;


public class TileRect 
{
    public enum Type { Place, Remove }
    public int StartX;
    public int StartY;
    public int Width;
    public int Height;
    public bool Started;
    public Type ButtonType;

    public void Start(int x, int y, Type buttonType) 
    {
        StartX = x;
        StartY = y;
        ButtonType = buttonType;
        Started = true;
    }

    public void Update(int x, int y) 
    {
        int rx = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
        int ry = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);

        int width = StartX - rx;
        int height = StartY - ry;

        if (width == 0)
        {
            Width = 10;
        }
        else 
        {
            Width = -width;
        }

        if (height == 0) 
        {
            Height = 10;
        }
        else 
        {
            Height = -height;
        }
    }

    public void AdjustIfNeeded() 
    {
        if (Width < 0) 
        {
            StartX = StartX + Width;
            Width = Math.Abs(Width);
        }
        if (Height < 0) 
        {
            StartY = StartY + Height;
            Height = Math.Abs(Height);
        }
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
            _ => throw new InvalidOperationException()
        };

        DrawUtils.Rect(spriteBatch, new Vector2(StartX, StartY), color, new Vector2(Width, Height), Vector2.Zero);
    }
}