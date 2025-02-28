using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using ImGuiNET;
using Riateu;

namespace Towermap;

public class LevelSelection : ImGuiElement
{
    private bool openNewLevel;
    private string levelName = "";
    private int currentWidth;
    private string[] widths = ["320", "420"];
    private Tower tower;
    private List<Level> levels = [];
    public IReadOnlyList<Level> Levels => levels;
    public Action<Level> OnSelect;
    public Action OnCreated;

    public void SelectTower(Tower tower) 
    {
        this.tower = tower;
        Refresh();
    }

    private void Refresh()
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

    private void CreateLevel(string name, string width)
    {
        var arr2D = new Array2D<bool>((int)WorldUtils.WorldWidth / 10, (int)WorldUtils.WorldHeight / 10);
        StringBuilder builder = new StringBuilder();
        for (int x = 0; x < arr2D.Columns; x ++) 
        {
            for (int y = 0; y < arr2D.Rows; y++) 
            {
                char c = arr2D[y, x] ? '1' : '0';
                builder.Append(c);
            }
            builder.AppendLine();
        }
        var emptyDefault = builder.ToString();

        XmlDocument document = new XmlDocument();
        var level = document.CreateElement("level");
        document.AppendChild(level);

        level.SetAttribute("width", width);
        level.SetAttribute("height", "240");

        var Solids = document.CreateElement("Solids");
        Solids.SetAttribute("exportMode", "Bitstring");
        Solids.InnerText = emptyDefault;
        level.AppendChild(Solids);

        var BG = document.CreateElement("BG");
        BG.SetAttribute("exportMode", "Bitstring");
        BG.InnerText = emptyDefault;
        level.AppendChild(BG);

        var SolidTiles = document.CreateElement("SolidTiles");
        SolidTiles.SetAttribute("exportMode", "TrimmedCSV");
        level.AppendChild(SolidTiles);

        var BGTiles = document.CreateElement("BGTiles");
        BGTiles.SetAttribute("exportMode", "TrimmedCSV");
        level.AppendChild(BGTiles);

        var Entities = document.CreateElement("Entities");
        level.AppendChild(Entities);

        document.Save(Path.Combine(Path.GetDirectoryName(tower.TowerPath), $"{name}.oel"));
    }

    public override void DrawGui()
    {
        if (openNewLevel) 
        {
            ImGui.OpenPopup("New Level");
        }

        ImGui.SetNextWindowSize(new Vector2(520, 320));
        if (ImGui.BeginPopupModal("New Level", ref openNewLevel)) 
        {
            ImGui.InputText("Level Name", ref levelName, 100);
            ImGui.Combo("Width", ref currentWidth, widths, 2);
            bool condition = string.IsNullOrEmpty(levelName) || levels.Select(x => x.FileName == levelName + ".oel").FirstOrDefault();
            if (condition)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Create"))
            {
                CreateLevel(levelName, widths[currentWidth]);
                Refresh();
                OnCreated?.Invoke();
                openNewLevel = false;
            }

            if (condition)
            {
                ImGui.EndDisabled();
            }
            ImGui.EndPopup();
        }

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
        if (ImGui.Selectable("+ Add Level")) 
        {
            openNewLevel = true;
        }
        ImGui.End();
        ImGui.PopStyleVar();
    }
}