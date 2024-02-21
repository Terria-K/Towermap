using System;
using System.Numerics;
using ImGuiNET;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class TilePanel : ImGuiElement
{
    private string name;
    private IntPtr texturePtr;
    private SpriteTexture texture;
    private bool holding;
    private Rect currentRect = new Rect(0, 0, 20, 20);
    private Vector2 framePos;

    public bool IsItemHovered;

    public TilePanel(IntPtr intPtr, string atlasName, string name) 
    {
        this.name = name;
        texturePtr = intPtr;

        texture = Resource.Atlas[atlasName];
    }

    public Array2D<int> GetData() 
    {
        int width = currentRect.X / 10;
        int height = currentRect.Y / 10;
        var data = new Array2D<int>(width, height);

        for (int x = 0; x < data.Columns; x++) 
        {
            for (int y = 0; y < data.Rows; y++) 
            {
                var rx = currentRect.X + x * 10;
                var ry = currentRect.Y + y * 10;
                data[y, x] = ((y / 10) * (texture.Width / 10) + (x / 10));
            }
        }
        return data;
    }

    public void Update() 
    {
        var x = Input.InputSystem.Mouse.X;
        var y = Input.InputSystem.Mouse.Y;

        var fx = framePos.X;
        var fy = framePos.Y;
        var rx = (int)Math.Floor(((x - fx) / 2) / 10) * 10;
        var ry = (int)Math.Floor(((y - fy) / 2) / 10) * 10;

        if (holding) 
        {
            var width = currentRect.X - rx;

            if (width < 0) 
            {
                currentRect.W = -width;
            }
            var height = currentRect.Y - ry ;
            if (height < 0) 
            {
                currentRect.H = -height;
            }

            if (rx >= texture.Width) 
            {
                currentRect.W = -(currentRect.X - texture.Width);
            }
            if (ry >= texture.Height) 
            {
                currentRect.H = -(currentRect.Y - texture.Height);
            }
        }
        if (IsItemHovered && Input.InputSystem.Mouse.LeftButton.IsPressed) 
        {
            if (rx >= 0 && ry >= 0 && rx <= texture.Width - 10 && ry <= texture.Height - 10)
            {
                currentRect = new Rect(rx, ry, 10, 10);
                holding = true;
            }
        }

        if (Input.InputSystem.Mouse.LeftButton.IsUp) 
        {
            holding = false;
        }
    }

    public override void DrawGui()
    {
        ImGui.SetNextWindowSize(new Vector2(texture.Width + 10, texture.Height + 20) * 2, ImGuiCond.Always);
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize;
        if (IsItemHovered)
            flags |= ImGuiWindowFlags.NoMove;
        ImGui.Begin(name, flags);
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        framePos = ImGui.GetCursorScreenPos();
        ImGui.Image(texturePtr, new Vector2(texture.Width, texture.Height) * 2, texture.UV.TopLeft.ToNumericsVec2(), texture.UV.BottomRight.ToNumericsVec2());
        IsItemHovered = ImGui.IsItemHovered();

        drawList.AddRect(new Vector2(framePos.X + currentRect.X * 2, framePos.Y + currentRect.Y * 2), 
            new Vector2(framePos.X + (currentRect.X * 2 + currentRect.W * 2), framePos.Y + (currentRect.Y * 2 + currentRect.H * 2)), Color.Yellow.PackedValue);
        ImGui.End();
    }
}