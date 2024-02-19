using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class LevelSelection : ImGuiElement
{
    private List<string> levels = [];
    private string path;
    public Action<string> OnSelect;

    public void SelectTower(string towerPath) 
    {
        levels.Clear();
        path = towerPath;
        var files = Directory.GetFiles(towerPath);
        Array.Sort(files);
        foreach (var file in files)
        {
            if (!file.EndsWith(".json") && !file.EndsWith(".oel"))
            {
                continue;
            }
            levels.Add(Path.GetFileName(file));
        }
    }

    public override void DrawGui()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowSize(new Vector2(150, 640), ImGuiCond.Always);
        ImGui.SetNextWindowPos(new Vector2(0, 20));
        ImGui.Begin("Levels", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        foreach (var level in levels) 
        {
            if (ImGui.Selectable(level)) 
            {
                OnSelect?.Invoke(Path.Combine(path, level));
            }
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}