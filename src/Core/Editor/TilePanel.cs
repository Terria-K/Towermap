using System;
using System.Numerics;
using System.Xml;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;
using SDL3;
using Towermap.TowerFall;

namespace Towermap;

public class TilePanel : ImGuiElement
{
    private string name;
    private IntPtr texturePtr;
    private TextureQuad texture;
    private bool holding;
    private Rectangle currentRect = new Rectangle(0, 0, 10, 10);
    private Point startPos;
    private Vector2 framePos;

    public bool IsImageHovered;
    public bool IsWindowHovered;

    public TilePanel(IntPtr intPtr, string atlasName, string name) 
    {
        this.name = name;
        texturePtr = intPtr;

        texture = Resource.Atlas[atlasName];
    }

    public void SetTheme(TilesetData.Tileset tileset) 
    {
        texture = Resource.Atlas[tileset.Image];
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
        var buttons = (uint)SDL.SDL_GetGlobalMouseState(out float x, out float y);

        var fx = framePos.X;
        var fy = framePos.Y;
        var rx = (int)Math.Floor(((x - fx) / 2) / 10) * 10;
        var ry = (int)Math.Floor(((y - fy) / 2) / 10) * 10;

        if (holding) 
        {
            rx = Math.Max(0, rx);
            ry = Math.Max(0, ry);
            int lx = Math.Min(startPos.X, rx);
            int ly = Math.Min(startPos.Y, ry);
            int lw = Math.Max(startPos.X, rx);
            int lh = Math.Max(startPos.Y, ry);

            currentRect = new Rectangle(lx, ly, lw - lx + 10, lh - ly + 10);

            if (rx >= texture.Width)
            {
                currentRect.Width = texture.Width - currentRect.X;
            }

            if (ry >= texture.Height)
            {
                currentRect.Height = texture.Height - currentRect.Y;
            }
        }
        if (IsImageHovered && (buttons & 0b0001) != 0 && !holding) 
        {
            if (rx >= 0 && ry >= 0 && rx <= texture.Width - 10 && ry <= texture.Height - 10)
            {
                currentRect = new Rectangle(rx, ry, 10, 10);
                startPos = new Point(rx, ry);
                holding = true;
            }
        }

        if ((buttons & 0b0001) == 0) 
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

        drawList.AddRect(
            new Vector2(framePos.X + currentRect.X * 2, framePos.Y + currentRect.Y * 2), 
            new Vector2(framePos.X + (currentRect.X * 2 + currentRect.Width * 2), 
            framePos.Y + (currentRect.Y * 2 + currentRect.Height * 2)), 
            Color.Yellow.RGBA);
        ImGui.End();
        ImGui.PopStyleVar();
    }
}