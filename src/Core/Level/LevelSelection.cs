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
    public Action<Level> OnSelect;
    public Action OnCreated;

    public void SelectTower(Tower tower) 
    {
        this.tower = tower;
        Refresh();
    }

    private void Refresh()
    {
        tower.ClearAllLevels();

        string path = Path.GetDirectoryName(tower.TowerPath);
        var files = Directory.GetFiles(path);
        Array.Sort(files);
        foreach (var file in files)
        {
            if (!file.EndsWith(".json") && !file.EndsWith(".oel"))
            {
                continue;
            }
            tower.AddLevel(new Level(file));
        }
    }

    private void CreateLevel(string name, string width)
    {
        int w = (int)WorldUtils.WorldWidth / 10;
        int h = (int)WorldUtils.WorldHeight / 10;
        int len = w * h; // buffer won't go up beyond 4096 anyway as the game is quite small


        var arr2D = new StackArray2D<bool>(w, h, stackalloc bool[len]);
        string emptyDefault;
        using (ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[len]))
        {
            for (int y = 0; y < h; y++) 
            {
                for (int x = 0; x < w; x += 2) 
                {
                    // we can unroll the loop like this
                    builder.Append('0');
                    builder.Append('0');
                }

                if (y != h - 1)
                {
                    builder.AppendLine();
                }
            }
            emptyDefault = builder.ToString();
        }


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
            bool condition = string.IsNullOrEmpty(levelName) || tower.Levels.Select(x => x.FileName == levelName + ".oel").FirstOrDefault();
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
        ImGui.Begin("Levels", ImGuiWindowFlags.NoCollapse);

        if (tower == null)
        {
            ImGui.End();
            ImGui.PopStyleVar();
            return;
        }

        foreach (var level in tower.Levels) 
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