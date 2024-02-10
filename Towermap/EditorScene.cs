using System;
using ImGuiNET;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public class EditorScene : Scene
{
    private ImGuiRenderer imGui;
    public EditorScene(GameApp game) : base(game)
    {
        imGui = new ImGuiRenderer(game.GraphicsDevice, game.MainWindow, 960, 640);
    }

    public override void Begin()
    {
    }

    public override void Update(double delta)
    {
        imGui.Update(GameInstance.Inputs, ImGuiCallback);
    }

    private void ImGuiCallback()
    {
        ImGui.ShowDemoWindow();
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer, IBatch batch)
    {
        buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, Color.CornflowerBlue));
        imGui.Draw(buffer);
        buffer.EndRenderPass();
    }

    public override void End()
    {
    }
}