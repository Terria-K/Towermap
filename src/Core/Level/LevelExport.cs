using System;
using System.Linq;
using System.Text;
using System.Xml;
using Cosmiria;

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
public struct ExportDialog() : IComponent
{
    public BindableValue<bool> Open;
    public Theme Theme;
    public Callback<ExportOverride> OnExport;

    private readonly TextBuffer name = new();
    private readonly TextBuffer author = new();
    private readonly TextBuffer tileset = new();
    private readonly TextBuffer bgtileset = new();
    private readonly TextBuffer background = new();
    private readonly TextBuffer music = new();
    private readonly TextBuffer lanterns = new();
    private readonly TextBuffer world = new();
    private readonly NumberBuffer darkOpacity = new(0, 0.1, 0, 1);
    private bool cold;

    public Node Render(UIStack ui, EventStack ev)
    {
        ui.Effect(ev.Listener(static (ref ExportDialog d) => d.Reset()), Open.Value);
        var modal = ui.Modal(Open, "Export Option", style: default(Style).W(560));
        if (!modal.IsOpen)
        {
            return modal;
        }

        modal.Body.Child([
            ui.Labeled("Name", ui.TextInput(name, null, Styles.Field)),
            ui.Labeled("Author", ui.TextInput(author, null, Styles.Field)),
            ui.Labeled("Tileset", ui.TextInput(tileset, null, Styles.Field)),
            ui.Labeled("BG Tileset", ui.TextInput(bgtileset, null, Styles.Field)),
            ui.Labeled("Background", ui.TextInput(background, null, Styles.Field)),
            ui.Labeled("Music", ui.TextInput(music, null, Styles.Field)),
            ui.Labeled("Lanterns", ui.TextInput(lanterns, null, Styles.Field)),
            ui.Labeled("World", ui.TextInput(world, null, Styles.Field)),
            ui.Labeled("Dark Opacity", ui.NumberInput(darkOpacity, Styles.Field)),
            ui.Checkbox(cold, "Cold").OnClick(ev.Listener(static (ref ExportDialog d) => d.cold = !d.cold)),
            ui.Text("Be warned that certain entities might not be published on the workshop page. Make sure that all entities exist in the official TF editor.")
                .Styled(Styles.Muted),
            ui.Button("Export").OnClick(ev.Listener(static (ref ExportDialog d) => d.Export()))
                .Disabled(name.Length == 0 || author.Length == 0),
        ]);
        return modal;
    }

    private void Reset()
    {
        if (!Open.Value || Theme == null)
        {
            return;
        }
        tileset.Set(Theme.Tileset);
        bgtileset.Set(Theme.BGTileset);
        background.Set(Theme.Background);
        music.Set(Theme.Music);
        lanterns.Set(Theme.Lanterns);
        world.Set(Theme.World ?? "Normal");
        darkOpacity.Value = Theme.DarknessOpacity;
        cold = Theme.Cold;
    }

    private void Export()
    {
        OnExport.Invoke(new ExportOverride {
            Name = name.ToString(),
            Author = author.ToString(),
            Tileset = tileset.ToString(),
            BGTileset = bgtileset.ToString(),
            Background = background.ToString(),
            Music = music.ToString(),
            Lanterns = lanterns.ToString(),
            World = world.ToString(),
            DarkOpacity = darkOpacity.FloatValue,
            Cold = cold
        });
        Open.Value = false;
    }
}

public static class LevelExport
{
    public static void TowerExport(Tower tower, string path, in ExportOverride exportOverride)
    {
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