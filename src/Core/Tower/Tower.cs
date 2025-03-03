using System;
using System.Collections.Generic;
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


    public void AddLevel(Level level)
    {
        levels.Add(level);
    }

    public void ClearAllLevels()
    {
        levels.Clear();
    }

    public void SetTheme(Theme theme) 
    {
        Theme = theme;
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

        var treasure = tower["treasure"];
        if (treasure != null)
        {
            ArrowRates = treasure.AttrFloat("arrows", 0.2f);
            var tr = treasure.InnerText.Split(",");
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