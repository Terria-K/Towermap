using System.Numerics;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public class EditorWindow
{
    private RenderTarget levelTarget;
    private nint textureTarget;

    public bool IsItemHovered;

    public EditorWindow(ImGuiRenderer renderer, RenderTarget levelTarget)
    {
        this.levelTarget = levelTarget;
        textureTarget = renderer.BindTexture(levelTarget);

    }

    public void DrawGui()
    {
        var quad = new TextureQuad(
                    new Point(420, 240),
                    new Rectangle(0, 0, (int)WorldUtils.WorldWidth, (int)WorldUtils.WorldHeight));
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(WorldUtils.WorldWidth, WorldUtils.WorldHeight) * 2);
        ImGui.Begin("Editor Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        Vector2 windowPos = GetCenteredViewportCenter(new Vector2(WorldUtils.WorldWidth, WorldUtils.WorldHeight) * 2);
        Vector2 screenPos = GetCenteredScreenCenter(new Vector2(WorldUtils.WorldWidth, WorldUtils.WorldHeight) * 2);

        ImGui.SetCursorPos(windowPos);

        ImGui.Image(textureTarget, new Vector2(WorldUtils.WorldWidth, WorldUtils.WorldHeight) * 2, quad.UV.TopLeft, quad.UV.BottomRight);

        ImGui.SetCursorPos(windowPos);
        ImGui.InvisibleButton("EditorWindow", new Vector2(WorldUtils.WorldWidth, WorldUtils.WorldHeight) * 2);
        IsItemHovered = ImGui.IsItemHovered();

        WorldUtils.WorldX = (int)screenPos.X;
        WorldUtils.WorldY = (int)screenPos.Y;

        ImGui.End();
        ImGui.PopStyleVar();
    }

    private Vector2 GetCenteredScreenCenter(Vector2 aspectSize) 
    {
        var windowSize = GetWindowSize();

        var viewportX = (windowSize.X / 2.0f) - (aspectSize.X / 2.0f);
        var viewportY = (windowSize.Y / 2.0f) - (aspectSize.Y / 2.0f);

        return new Vector2(viewportX, viewportY) + ImGui.GetCursorScreenPos();
    }

    private Vector2 GetCenteredViewportCenter(Vector2 aspectSize) 
    {
        var windowSize = GetWindowSize();

        var viewportX = (windowSize.X / 2.0f) - (aspectSize.X / 2.0f);
        var viewportY = (windowSize.Y / 2.0f) - (aspectSize.Y / 2.0f);

        return new Vector2(
            viewportX + ImGui.GetCursorPosX(), 
            viewportY + ImGui.GetCursorPosY());
    }

    private static Vector2 GetWindowSize() 
    {
        var windowSize = ImGui.GetContentRegionAvail();
        windowSize.X -= ImGui.GetScrollX();
        windowSize.Y -= ImGui.GetScrollY();
        return windowSize;
    }
}