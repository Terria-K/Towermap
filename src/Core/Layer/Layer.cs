using System;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class LayersPanel : ImGuiElement
{
    public Action<string> OnLayerSelect;
    public Action<int, bool> ShowOrHide;
    private string[] layers = [
        FA6.BorderAll + " Solids",
        FA6.BorderNone + " BG",
        FA6.Person + " Entities",
        FA6.SquareFull + " SolidTiles",
        FA6.SquareMinus + " BGTiles"
    ];

    private bool[] visibility = [
        true, true, true, true, true
    ];

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowSize(new Vector2(200, 620), ImGuiCond.Always);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(1024 - 200, 20));

        var size = new Vector2(158, 0.0f);
        ImGui.Begin("Layers", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        for (int i = 0; i < layers.Length; i++) 
        {
            var span = layers[i];

            if (ImGui.Button(span, size)) 
            {
                OnLayerSelect?.Invoke(span);
            }
            ImGui.SameLine();
            ImGui.PushID(i);
            if (ImGui.Button(visibility[i] ? FA6.Eye : FA6.EyeSlash)) 
            {
                ShowOrHide?.Invoke(i, visibility[i] = !visibility[i]);
            }
            ImGui.PopID();
        }
        UpdateChildrens();
        ImGui.End();
        ImGui.PopStyleVar();
    }
}