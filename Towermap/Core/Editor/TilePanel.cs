using System;
using System.Numerics;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;

namespace Towermap;

public class TilePanel : ImGuiElement
{
    private string name;
    private IntPtr texturePtr;
    private TextureQuad texture;
    private bool holding;
    private Rectangle currentRect = new Rectangle(0, 0, 10, 10);
    private Vector2 framePos;

    public bool IsImageHovered;
    public bool IsWindowHovered;

    public TilePanel(IntPtr intPtr, string atlasName, string name) 
    {
        this.name = name;
        texturePtr = intPtr;

        texture = Resource.Atlas[atlasName];
    }

    public Array2D<int> GetData() 
    {
        int width = currentRect.Width / 10;
        int height = currentRect.Height / 10;
        var data = new Array2D<int>(height, width);

        for (int x = 0; x < data.Columns; x++) 
        {
            for (int y = 0; y < data.Rows; y++) 
            {
                var rx = currentRect.X + x * 10;
                var ry = currentRect.Y + y * 10;
                data[y, x] = ((ry / 10) * (texture.Width / 10) + (rx / 10));
            }
        }
        return data;
    }

    public void Update() 
    {
        var x = Input.Mouse.X;
        var y = Input.Mouse.Y;

        var fx = framePos.X;
        var fy = framePos.Y;
        var rx = (int)Math.Floor(((x - fx) / 2) / 10) * 10;
        var ry = (int)Math.Floor(((y - fy) / 2) / 10) * 10;

        if (holding) 
        {
            var width = currentRect.X - rx;

            if (width < 0) 
            {
                currentRect.Width = -width;
            }
            var height = currentRect.Y - ry ;
            if (height < 0) 
            {
                currentRect.Height = -height;
            }

            if (rx >= texture.Width) 
            {
                currentRect.Width = -(currentRect.X - texture.Width);
            }
            if (ry >= texture.Height) 
            {
                currentRect.Height = -(currentRect.Y - texture.Height);
            }
        }
        if (IsImageHovered && Input.Mouse.LeftButton.Pressed) 
        {
            if (rx >= 0 && ry >= 0 && rx <= texture.Width - 10 && ry <= texture.Height - 10)
            {
                currentRect = new Rectangle(rx, ry, 10, 10);
                holding = true;
            }
        }

        if (Input.Mouse.LeftButton.IsUp) 
        {
            holding = false;
        }
    }

    public override void DrawGui()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 4);
        ImGui.SetNextWindowSize(new Vector2(texture.Width + 10, texture.Height + 20) * 2, ImGuiCond.Always);
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize;
        if (IsImageHovered)
            flags |= ImGuiWindowFlags.NoMove;
        ImGui.Begin(name, flags);

        if (!Input.Mouse.LeftButton.IsDown && !Input.Mouse.RightButton.IsDown)
            IsWindowHovered = ImGui.IsWindowHovered();

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        framePos = ImGui.GetCursorScreenPos();
        ImGui.Image(texturePtr, new Vector2(texture.Width, texture.Height) * 2, texture.UV.TopLeft, texture.UV.BottomRight);
        IsImageHovered = ImGui.IsItemHovered();

        drawList.AddRect(new Vector2(framePos.X + currentRect.X * 2, framePos.Y + currentRect.Y * 2), 
            new Vector2(framePos.X + (currentRect.X * 2 + currentRect.Width * 2), framePos.Y + (currentRect.Y * 2 + currentRect.Height * 2)), Color.Yellow.RGBA);
        ImGui.End();
        ImGui.PopStyleVar();
    }
}