using System;
using System.IO;
using System.Numerics;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public class PromptScene : Scene
{
    private SaveState saveState;
    private ImGuiRenderer imGui;
    private string TFPath = "";
    private bool darkWorldFound;
    private bool fortRiseFound;
    private bool validPath;

    public PromptScene(GameApp game, ImGuiRenderer renderer, SaveState saveState) : base(game)
    {
        imGui = renderer;
        this.saveState = saveState;
    }

    public override void Begin()
    {
    }

    public override void End() {}

    public override void Process(double delta)
    {
        if (!string.IsNullOrEmpty(TFPath))
        {
            if (!File.Exists(Path.Combine(TFPath, "TowerFall.exe")))
            {
                validPath = false;
                return;
            }

            validPath = true;

            darkWorldFound = Directory.Exists(Path.Combine(TFPath, "DarkWorldContent"));
            fortRiseFound = File.Exists(Path.Combine(TFPath, "PatchVersion.txt"));
        }
    }

    private void Proceed()
    {
        saveState.TFPath = TFPath;
        SaveIO.SaveJson<SaveState>("towersave.json", saveState, SaveStateContext.Default.SaveState);
        (GameInstance as TowermapGame).InitEditor(imGui, saveState);
    }

    public override void Render(CommandBuffer commandBuffer, RenderTarget swapchainTarget)
    {
        imGui.Update(GameInstance.InputDevice, ImGuiRender);

        var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(swapchainTarget, Color.Black, true));
        imGui.Render(commandBuffer, renderPass);
        commandBuffer.EndRenderPass(renderPass);
    }

    private void ImGuiRender()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(0, 0));
        ImGui.SetNextWindowSize(new Vector2(1280, 640));
        ImGui.Begin("Prompt Menu", ImGuiWindowFlags.NoResize 
        | ImGuiWindowFlags.NoTitleBar 
        | ImGuiWindowFlags.NoSavedSettings 
        | ImGuiWindowFlags.NoFocusOnAppearing
        | ImGuiWindowFlags.NoMouseInputs);

        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(20, 280));
        ImGui.SetNextWindowSize(new Vector2(1280 - 40, 320));
        ImGui.Begin("Prompt", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);

        float itemWidth = ImGui.CalcItemWidth();
        float textWidth = ImGui.CalcTextSize("Towermap needs to get a content from TowerFall. Input the TowerFall path below").X;
        ImGui.SetCursorPosX(((ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX()) - itemWidth - (textWidth / 2.0f)) / 2.0f);
        ImGui.Text("Towermap needs to get a content from TowerFall. Input the TowerFall path below");
        ImGui.SetCursorPosX(((ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX()) - itemWidth - (400 /2)) / 2.0f);
        ImGui.SetNextItemWidth(400);
        ImGui.InputText("##TowerFallPathInput", ref TFPath, 100);
        ImGui.SetCursorPosX(((ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX()) - itemWidth) / 1.8f);
        if (ImGui.Button("Browse")) 
        {
            Browse();
        }

        if (!validPath)
        {
            ImGui.BeginDisabled();
        }
        ImGui.SetCursorPosX(((ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX()) - itemWidth) / 1.8f);
        if (ImGui.Button("Proceed")) 
        {
            Proceed();
        }
        if (!validPath)
        {
            ImGui.EndDisabled();
        }

        ImGui.SeparatorText("Features Available");
        ImGui.BeginDisabled();
        ImGui.Checkbox("Dark World", ref darkWorldFound);
        ImGui.Checkbox("FortRise", ref fortRiseFound);
        ImGui.EndDisabled();

        ImGui.End();

        ImGui.End();


        ImGui.BeginMainMenuBar();
        if (ImGui.MenuItem("Close"))
        {
            GameInstance.Quit();
        }

        ImGui.EndMainMenuBar();
    }

    private void Browse()
    {
        FileDialog.OpenFolder((path) => {
            TFPath = path;
        });
    }
}