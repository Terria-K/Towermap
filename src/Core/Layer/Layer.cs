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
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
        ImGui.Begin("Layers", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        var size = Vector2.UnitX * (ImGui.GetWindowSize().X - 42f);

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

        ImGui.PopStyleVar();
        UpdateChildrens();
        ImGui.End();
        ImGui.PopStyleVar();
    }
}