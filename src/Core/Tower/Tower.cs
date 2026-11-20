using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Towermap;

public class Tower 
{
    public enum TowerType { Versus, Quest, Trials, DarkWorld }
    public TowerType Type;
    public Theme Theme;
    public string TowerPath;
    public float ArrowRates;
    public List<string> Treasures = [];

    private List<Level> levels = [];
    public IReadOnlyList<Level> Levels => levels;


    public void RefreshLevels()
    {
        levels.Clear();

        string path = Path.GetDirectoryName(TowerPath);
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

    public void CreateLevel(string name, int width)
    {
        int w = width / 10;
        int h = (int)WorldUtils.WorldHeight / 10;
        string emptyDefault;
        using (ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[w * h + h]))
        {
            for (int y = 0; y < h; y++) 
            {
                for (int x = 0; x < w; x += 2) 
                {
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

        level.SetAttribute("width", width.ToString());
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

        document.Save(Path.Combine(Path.GetDirectoryName(TowerPath), $"{name}.oel"));
        RefreshLevels();
    }

    public void SetTheme(Theme theme) 
    {
        Theme = theme;
    }

    private TowerType GuessType(XmlElement elm)
    {
        if (elm["treasure"] != null)
        {
            return TowerType.Versus;
        }
        if (elm["time"] != null)
        {
            return TowerType.DarkWorld;
        }
        return TowerType.Quest;
    }

    public bool Load(string path)
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);
        Treasures.Clear();
        TowerPath = path;

        var tower = document["tower"];
        var theme = tower["theme"];

        TowerType towerType = TowerType.Versus;
        if (tower.HasAttribute("mode"))
        {
            towerType = Enum.Parse<TowerType>(tower.Attr("mode"));
        }
        else 
        {
            towerType = GuessType(tower);
        }
        Type = towerType;

        var treasure = tower["treasure"];
        if (treasure != null)
        {
            ArrowRates = treasure.AttrFloat("arrows", 0.2f);
            var tr = treasure.InnerText.Trim().Split(",");
            foreach (string t in tr)
            {
                Treasures.Add(t);
            }
        }

        var themeName = theme.InnerText.Trim();
        if (Themes.TryGetTheme(themeName, out Theme)) 
        {
            return true;
        }
        return false;
    }

    public void Save()
    {
        XmlDocument document = new XmlDocument();
        var tower = document.CreateElement("tower");
        tower.SetAttribute("mode", Type.ToString());
        document.AppendChild(tower);

        var theme = document.CreateElement("theme");
        theme.InnerText = Theme.ID;
        tower.AppendChild(theme);

        var treasure = document.CreateElement("treasure");
        if (ArrowRates != 0)
        {
            treasure.SetAttribute("arrows", ArrowRates.ToString());
        }
        treasure.InnerText = string.Join(",", Treasures);
        tower.AppendChild(treasure);

        document.Save(TowerPath);
    }
}