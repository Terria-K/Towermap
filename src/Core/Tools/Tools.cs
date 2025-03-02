using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Riateu.Graphics;

namespace Towermap;

public class Tools : ImGuiElement
{
    private class Tool(string name, Action onCallback, bool selectable, int section)
    {
        public string Name = name;
        public Action OnCallback = onCallback;
        public int Section = section;
        public bool Selectable = selectable;
    }

    private List<Tool> toolName = [];
    private Tool currentSelected;

    public void AddTool(string tool, Action act, bool selectable, int section = 0) 
    {
        toolName.Add(new Tool(tool, act, selectable, section));
    }

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(155, 20));
        ImGui.Begin("Tools", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
        int section = 0;
        foreach (var tool in toolName) 
        {
            if (tool.Section != section)
            {
                float x = ImGui.GetCursorPosX();
                ImGui.SetCursorPosX(x + 2);
                section = tool.Section;
            }
            bool cond = tool == currentSelected;
            if (cond)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Color(255, 51, 204, 255).ABGR);
            }
            if (ImGui.Button(tool.Name, new Vector2(40, 40))) 
            {
                if (tool.Selectable)
                {
                    currentSelected = tool;
                }
                tool.OnCallback();
            }
            if (cond)
            {
                ImGui.PopStyleColor();
            }
            ImGui.SameLine();
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}