using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Riateu.Graphics;

namespace Towermap;

public class Tools : ImGuiElement
{
    public enum ToolType
    {
        Selectable,
        FireAndForget,
        Toggleable
    }
    private class Tool
    {
        public string Name;
        public Action OnCallback;
        public Action<bool> OnCallbackToggled;
        public int Section;
        public ToolType ToolType;
        public bool Toggled;

        public Tool(string name, Action onCallback, ToolType toolType, int section)
        {
            Name =name;
            OnCallback = onCallback;
            ToolType = toolType;
            Section = section;
        }

        public Tool(string name, Action<bool> onCallback, ToolType toolType, int section)
        {
            Name = name;
            OnCallbackToggled = onCallback;
            ToolType = toolType;
            Section = section;
        }
    }

    private List<Tool> tools = [];
    private Tool currentSelected;

    public void AddTool(string tool, Action act, ToolType toolType, int section = 0) 
    {
        tools.Add(new Tool(tool, act, toolType, section));
    }

    public void AddTool(string tool, Action<bool> act, ToolType toolType, int section = 0) 
    {
        tools.Add(new Tool(tool, act, toolType, section));
    }

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(155, 20));
        ImGui.Begin("Tools", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
        int section = 0;
        foreach (var tool in tools) 
        {
            if (tool.Section != section)
            {
                float x = ImGui.GetCursorPosX();
                ImGui.SetCursorPosX(x + 2);
                section = tool.Section;
            }
            bool cond = tool == currentSelected || tool.Toggled;
            if (cond)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Color(255, 51, 204, 255).ABGR);
            }
            if (ImGui.Button(tool.Name, new Vector2(40, 40))) 
            {
                switch (tool.ToolType)
                {
                case ToolType.Selectable:
                    currentSelected = tool;
                    goto default;
                case ToolType.Toggleable:
                    tool.Toggled = !tool.Toggled;
                    tool.OnCallbackToggled(tool.Toggled);
                    break;
                default:
                    tool.OnCallback();
                    break;
                }
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