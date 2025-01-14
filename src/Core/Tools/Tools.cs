using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class Tools : ImGuiElement
{
    private struct Tool(string name, Action onCallback)
    {
        public string Name = name;
        public Action OnCallback = onCallback;
    }

    private List<Tool> toolName = [];

    public void AddTool(string tool, Action act) 
    {
        toolName.Add(new Tool(tool, act));
    }

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(155, 20));
        ImGui.Begin("Tools", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
        foreach (var tool in toolName) 
        {
            if (ImGui.Button(tool.Name, new Vector2(40, 40))) 
            {
                tool.OnCallback();
            }
            ImGui.SameLine();
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}