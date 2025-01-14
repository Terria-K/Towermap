using System.Xml;

namespace Towermap;

public class Tower 
{
    public enum TowerType { Versus, Quest, Trials, DarkWorld }
    public TowerType Type;
    public Theme Theme;
    public string TowerPath;

    public void SetTheme(Theme theme) 
    {
        Theme = theme;
    }


    public bool Load(string path)
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);
        TowerPath = path;

        var tower = document["tower"];
        var theme = tower["theme"];

        var themeName = theme.InnerText;
        if (Themes.TryGetTheme(themeName, out Theme)) 
        {
            return true;
        }
        return false;
    }

    public void Save()
    {
    }

}