using System;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class LayersPanel : ImGuiElement
{
    public Action<string> OnLayerSelect;
    private string[] layers = [
        "Solids",
        "BG",
        "Entities",
        "SolidTiles",
        "BGTiles"
    ];

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowSize(new Vector2(200, 620), ImGuiCond.Always);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(1024 - 200, 20));

        var size = new Vector2(185, 0.0f);
        ImGui.Begin("Layers", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        for (int i = 0; i < layers.Length; i++) 
        {
            var span = layers[i];
            if (ImGui.Button(span, size)) 
            {
                OnLayerSelect?.Invoke(span);
            }
        }
        UpdateChildrens();
        ImGui.End();
        ImGui.PopStyleVar();
    }
}