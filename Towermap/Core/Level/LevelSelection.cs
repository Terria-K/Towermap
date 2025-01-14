using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class LevelSelection : ImGuiElement
{
    private List<Level> levels = [];
    public IReadOnlyList<Level> Levels => levels;
    public Action<Level> OnSelect;

    public void SelectTower(Tower tower) 
    {
        levels.Clear();

        string path = Path.GetDirectoryName(tower.TowerPath);
        var files = Directory.GetFiles(path);
        Array.Sort(files);
        foreach (var file in files)
        {
            if (!file.EndsWith(".json") && !file.EndsWith(".oel"))
            {
                continue;
            }
            levels.Add(new Level(file));
        }
    }

    public override void DrawGui()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 2);
        ImGui.SetNextWindowSize(new Vector2(150, 640), ImGuiCond.Always);
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.SetNextWindowPos(mainViewport.Pos +  new Vector2(0, 20));
        ImGui.Begin("Levels", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        foreach (var level in levels) 
        {
            string filename;
            if (level.Unsaved) 
            {
                filename = level.FileName + "*";
            }
            else 
            {
                filename = level.FileName;
            }
            if (ImGui.Selectable(filename)) 
            {
                OnSelect?.Invoke(level);
            }
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}