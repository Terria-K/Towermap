using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;
using ImGuiNET;

namespace Towermap;

public struct ExportOverride 
{
    public string Name;
    public string Author;
    public string Tileset;
    public string BGTileset;
    public string Background;
    public string Music;
    public string Lanterns;
    public string World;
    public float DarkOpacity;
    public bool Cold;
}
public class ExportOption : ImGuiElement
{
    private string name = "";
    private string author = "";
    private string tileset;
    private string bgtileset;
    private string background;
    private string music;
    private string lanterns;
    private string world;
    private float darkOpacity;
    private bool cold;
    private Tower tower;
    public  Action<ExportOverride> OnExport;
    public ExportOption() {}

    public void SetTower(Tower tower)
    {
        this.tower = tower;
        tileset = tower.Theme.Tileset;
        bgtileset = tower.Theme.BGTileset;
        background = tower.Theme.Background;
        music = tower.Theme.Music;
        lanterns = tower.Theme.Lanterns;
        world = tower.Theme.World ?? "Normal";
        darkOpacity = tower.Theme.DarknessOpacity;
        cold = tower.Theme.Cold;
    }

    public override void DrawGui()
    {
        ImGui.SetNextWindowSize(new Vector2(520, 320));
        if (ImGui.BeginPopupModal("Export Option", ref enabled)) 
        {
            ImGui.InputText("Name", ref name, 100);
            ImGui.InputText("Author", ref author, 100);

            ImGui.InputText("Override Tileset", ref tileset, 100);
            ImGui.InputText("Override BG Tileset", ref bgtileset, 100);
            ImGui.InputText("Override Background", ref background, 100);
            ImGui.InputText("Override Music", ref music, 100);
            ImGui.InputText("Override Lanterns", ref lanterns, 100);
            ImGui.InputText("Override World", ref world, 100);
            ImGui.InputFloat("Override Dark Opacity", ref darkOpacity);
            ImGui.Checkbox("Override Cold", ref cold);

            bool condition = string.IsNullOrEmpty(name) || string.IsNullOrEmpty(author);

            if (condition)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Export"))
            {
                OnExport?.Invoke(new ExportOverride {
                    Name = name,
                    Author = author,
                    Tileset = tileset,
                    BGTileset = bgtileset,
                    Background = background,
                    Music = music,
                    Lanterns = lanterns,
                    World = world,
                    DarkOpacity = darkOpacity,
                    Cold = cold
                });
                Enabled = false;
            }

            if (condition)
            {
                ImGui.EndDisabled();
            }
            ImGui.EndPopup();
        }
    }
}

public static class LevelExport
{
    public static void TowerExport(Tower tower, string path, ExportOverride exportOverride)
    {
        // get the treasure mask
        Span<byte> treasureMask = stackalloc byte[21];
        treasureMask.Fill(48);

        foreach (var tres in tower.Treasures)
        {
            int ind = Array.IndexOf(Pickups.PickupNames, tres);
            byte inc = treasureMask[ind];
            inc += 1;
            treasureMask[ind] = Math.Min((byte)57, inc);
        }
        string treasureStr = Encoding.UTF8.GetString(treasureMask);

        var document = new XmlDocument();
        var towerXml = document.CreateElement("tower");
        document.AppendChild(towerXml);

        var title = document.CreateElement("title");
        title.InnerText = exportOverride.Name.ToUpperInvariant();
        towerXml.AppendChild(title);

        var author = document.CreateElement("author");
        author.InnerText = exportOverride.Author.ToUpperInvariant();
        towerXml.AppendChild(author);

        var icon = document.CreateElement("icon");
        towerXml.AppendChild(icon);
        {
            var data = document.CreateElement("data");
            data.InnerText = "0000000000000000011111000011111001111000000111100111100000011110011111000011111001001110011100100000011111100000000000111100000000000011110000000000011111100000000111100111100000111100001111000111110000111110000110000001100000010000000010000000000000000000";
            var tile = document.CreateElement("tile");
            tile.InnerText = "Normal";
            icon.AppendChild(data);
            icon.AppendChild(tile);
        }

        var levels = document.CreateElement("levels");
        var rand = new Random();

        int playerCount = 0;
        bool hasTeams = false;

        foreach (var level in tower.Levels)
        {
            level.Actors.Clear();
            level.LoadLevel(tower.Theme);
            bool teams = level.Actors.Where(x => x.Data.Name is "TeamSpawn" or "TeamSpawnA" or "TeamSpawnB").Any();
            if (teams && !hasTeams)
            {
                hasTeams = true;
            }
            int ffa = level.Actors.Where(x => x.Data.Name is "PlayerSpawn").Count();
            playerCount = Math.Max(ffa, playerCount);
            var levelXml = document.CreateElement("level");
            levelXml.SetAttribute("ffa", ffa.ToString());
            levelXml.SetAttribute("teams", teams ? "True" : "False");

            var LoadSeed = document.CreateElement("LoadSeed");
            LoadSeed.InnerText = (rand.Next() % 99999).ToString();
            levelXml.AppendChild(LoadSeed);

            string solid = level.Solids.Save();
            string bg = level.BGs.Save();
            var Solids = document.CreateElement("Solids");
            Solids.InnerText = solid;

            var BG = document.CreateElement("BG");
            BG.InnerText = bg;

            var SolidTiles = document.CreateElement("SolidTiles");
            SolidTiles.InnerText = level.SolidTiles.Save();

            var BGTiles = document.CreateElement("BGTiles");
            BGTiles.InnerText = level.BGTiles.Save();

            var Entities = document.CreateElement("Entities");

            foreach (var entity in level.Actors) 
            {
                if (entity is not LevelActor actor)
                    continue;
                var actorInfo = actor.Save();
                var element = document.CreateElement(actorInfo.Name);
                element.SetAttribute("x", actorInfo.X.ToString());
                element.SetAttribute("y", actorInfo.Y.ToString());
                foreach (var value in actorInfo.Values) 
                {
                    element.SetAttribute(value.Key, value.Value.ToString());
                }
                if (actorInfo.Nodes != null) 
                {
                    foreach (var node in actorInfo.Nodes) 
                    {
                        var xmlNode = document.CreateElement("node");
                        xmlNode.SetAttribute("x", node.X.ToString());
                        xmlNode.SetAttribute("y", node.Y.ToString());
                        element.AppendChild(xmlNode);
                    }
                }
                Entities.AppendChild(element);
            }

            levelXml.AppendChild(Solids);
            levelXml.AppendChild(BG);
            levelXml.AppendChild(SolidTiles);
            levelXml.AppendChild(BGTiles);
            levelXml.AppendChild(Entities);

            levels.AppendChild(levelXml);
            level.Actors.Clear();
        }


        var map = document.CreateElement("map");
        map.SetAttribute("x", tower.Theme.MapPosition.X.ToString());
        map.SetAttribute("y", tower.Theme.MapPosition.Y.ToString());
        towerXml.AppendChild(map);

        var players = document.CreateElement("players");
        players.SetAttribute("ffa", playerCount.ToString());
        players.SetAttribute("teams", hasTeams.ToString());
        towerXml.AppendChild(players);

        var mode = document.CreateElement("mode");
        mode.InnerText = "Any";
        towerXml.AppendChild(mode);

        var variants = document.CreateElement("variants");
        towerXml.AppendChild(variants);

        var treasure = document.CreateElement("treasure");
        treasure.SetAttribute("arrows", tower.ArrowRates.ToString());
        treasure.InnerText = treasureStr;
        towerXml.AppendChild(treasure);

        var tileset = document.CreateElement("tileset");
        tileset.InnerText = string.IsNullOrEmpty(exportOverride.Tileset) ? tower.Theme.Tileset : exportOverride.Tileset;
        towerXml.AppendChild(tileset);

        var bgtileset = document.CreateElement("bgtileset");
        bgtileset.InnerText = string.IsNullOrEmpty(exportOverride.BGTileset) ? tower.Theme.BGTileset : exportOverride.BGTileset;
        towerXml.AppendChild(bgtileset);

        var background = document.CreateElement("background");
        background.InnerText = string.IsNullOrEmpty(exportOverride.Background) ? tower.Theme.Background : exportOverride.Background;
        towerXml.AppendChild(background);

        var music = document.CreateElement("music");
        music.InnerText = string.IsNullOrEmpty(exportOverride.Music) ? tower.Theme.Music: exportOverride.Music;
        towerXml.AppendChild(music);

        var lanterns = document.CreateElement("lanterns");
        lanterns.InnerText = string.IsNullOrEmpty(exportOverride.Lanterns) ? tower.Theme.Lanterns : exportOverride.Lanterns;
        towerXml.AppendChild(lanterns);

        var darkOpacity = document.CreateElement("darkOpacity");
        darkOpacity.InnerText = exportOverride.DarkOpacity.ToString();
        towerXml.AppendChild(darkOpacity);

        var cold = document.CreateElement("cold");
        cold.InnerText = exportOverride.Cold.ToString();
        towerXml.AppendChild(cold);

        var world = document.CreateElement("world");
        world.InnerText = exportOverride.World;
        towerXml.AppendChild(world);

        towerXml.AppendChild(levels);

        document.Save(path);
    }
}