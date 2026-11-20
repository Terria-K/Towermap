using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Cosmiria;

namespace Towermap;

public struct TowerData 
{
    public int ThemeID;
    public float ArrowRates;
}

public sealed class QuestData
{
    public readonly List<Wave>[] Waves = [[], []];

    public void Load(XmlElement data) 
    {
        Waves[0].Clear();
        Waves[1].Clear();

        LoadWaves(Waves[0], data["normal"]);
        LoadWaves(Waves[1], data["hardcore"]);
    }

    private static void LoadWaves(List<Wave> waves, XmlElement data) 
    {
        foreach (XmlElement wave in data.GetElementsByTagName("wave"))
        {
            bool dark = wave.AttrBool("dark");
            bool scroll = wave.AttrBool("scroll");
            bool slow = wave.AttrBool("slow");
            Wave w = new Wave() 
            {
                IsDark = dark,
                IsScroll = scroll,
                IsSlow = slow,
            };

            foreach (XmlElement group in wave.ChildNodes)
            {
                if (group.Name == "floors")
                {
                    w.Groups.Add(new Group(int.Parse(group.InnerText)));
                }
                else if (group.Name == "group")
                {
                    var delay = group.AttrInt("delay");
                    var solo = group.AttrBool("solo");
                    var coop = group.AttrBool("coop");

                    var treasureList = new List<BindableValue<int>>();
                    if (group.TryGetElement("treasure", out XmlElement treasureXml))
                    {
                        ReadOnlySpan<char> treasureText = treasureXml.InnerText.AsSpan().Trim();
                        var splitted = treasureText.Split(',');

                        foreach (var range in splitted)
                        {
                            var trimmed = treasureText[range].Trim();
                            treasureList.Add(new(Array.IndexOf(Pickups.PickupNames, new string(trimmed))));
                        }
                    }

                    var enemies = new List<TextBuffer>();
                    if (group.TryGetElement("enemies", out XmlElement enemiesXml))
                    {
                        ReadOnlySpan<char> enemiesText = enemiesXml.InnerText.AsSpan().Trim();
                        foreach (var range in enemiesText.Split(','))
                        {
                            enemies.Add(new TextBuffer(new string(enemiesText[range].Trim())));
                        }
                    }

                    var spawns = new List<Group.Spawn>();
                    if (group.TryGetElement("spawns", out XmlElement spawnsXml))
                    {
                        var spawnsText = spawnsXml.InnerText.AsSpan().Trim();
                        var splitted = spawnsText.Split(',');
                        foreach (var range in splitted)
                        {
                            spawns.Add(new Group.Spawn()
                            {
                                Name = new string(spawnsText[range].Trim()),
                                IsChecked = true
                            });
                        }
                    }

                    w.Groups.Add(new Group() 
                    {
                        TreasureIDs = treasureList,
                        Delay = new(delay, 1, 0, integer: true),
                        Solo = solo,
                        CoOp = coop,
                        Spawns = spawns,
                        Enemies = enemies
                    });
                }
            }

            waves.Add(w);
        }
    }

    public void SetAllGroupDirty()
    {
        foreach (var diffWave in Waves)
        {
            foreach (var wave in diffWave)
            {
                foreach (var group in wave.Groups)
                {
                    group.Dirty = true;
                }
            }
        }
    }

    public static void RefreshSpawn(Tower tower, List<Group.Spawn> spawns)
    {
        var spawn = spawns.Where(x => x.IsChecked).Select(x => x.Name).ToHashSet();
        var nameList = tower.Levels[0].Actors.Where(x => x.Data.Name == "Spawner")
            .Select(x => x.CustomData["name"].ToString())
            .ToList();
        
        spawns.Clear();
        
        foreach (var name in nameList)
        {
            if (spawn.Contains(name))
            {
                spawns.Add(new Group.Spawn() 
                {
                    Name = name,
                    IsChecked = true 
                });
                continue;
            }
            spawns.Add(new Group.Spawn() 
            {
                Name = name,
                IsChecked = false
            });
        }
    }

    public void Save(string towerPath)
    {
        XmlDocument document = new XmlDocument();
        var data = document.CreateElement("data");
        document.AppendChild(data);

        int i = 0;
        foreach (var wave in Waves)
        {
            string diff = i == 0 ? "normal" : "hardcore";

            var diffXml = document.CreateElement(diff);
            data.AppendChild(diffXml);

            foreach (var w in wave)
            {
                var waveXml = document.CreateElement("wave");
                if (w.IsDark)
                {
                    waveXml.SetAttribute("dark", "true");
                }
                if (w.IsSlow)
                {
                    waveXml.SetAttribute("slow", "true");
                }
                if (w.IsScroll)
                {
                    waveXml.SetAttribute("scroll", "true");
                }

                foreach (var group in w.Groups)
                {
                    if (group.IsFloor)
                    {
                        var floorXml = document.CreateElement("floor");
                        floorXml.InnerText = group.FloorNumber.IntValue.ToString();
                        waveXml.AppendChild(floorXml);    
                        continue;
                    }

                    var groupXml = document.CreateElement("group");
                    if (group.CoOp)
                    {
                        groupXml.SetAttribute("coop", "true");
                    }
                    if (group.Solo)
                    {
                        groupXml.SetAttribute("solo", "true");
                    }
                    if (group.Delay.IntValue != 0)
                    {
                        groupXml.SetAttribute("delay", group.Delay.IntValue.ToString());
                    }
                    var listStr = new List<string>();
                    if (group.TreasureIDs.Count > 0)
                    {
                        var treasureXml = document.CreateElement("treasure");
                        foreach (var treasure in group.TreasureIDs)
                        {
                            var name = Pickups.PickupNamesWithNone[treasure.Value];
                            if (name == "None")
                            {
                                continue;
                            }
                            listStr.Add(name);
                        }

                        treasureXml.InnerText = string.Join(',', listStr);
                        groupXml.AppendChild(treasureXml);
                    }

                    listStr.Clear();
                    if (group.Spawns.Count > 0)
                    {
                        var spawnsXml = document.CreateElement("spawns");
                        foreach (var spawn in group.Spawns)
                        {
                            var name = spawn.Name;
                            if (!spawn.IsChecked)
                            {
                                continue;
                            }
                            listStr.Add(name);
                        }
                        spawnsXml.InnerText = string.Join(',', listStr);
                        groupXml.AppendChild(spawnsXml);
                    }

                    listStr.Clear();
                    if (group.Enemies.Count > 0)
                    {
                        var enemiesXml = document.CreateElement("enemies");
                        foreach (var enemy in group.Enemies)
                        {
                            listStr.Add(enemy.ToString());
                        }
                        enemiesXml.InnerText = string.Join(',', listStr);
                        groupXml.AppendChild(enemiesXml);
                    }

                    waveXml.AppendChild(groupXml);
                }

                diffXml.AppendChild(waveXml);
            }
            i += 1;
        }

        document.Save(Path.Combine(Path.GetDirectoryName(towerPath), "data.xml"));
    }
}

public struct TowerSettingsDialog() : IComponent
{
    public BindableValue<bool> Open;
    public Tower Tower;
    public QuestData Quest;
    public Callback<TowerData> OnSave;
    public Callback OnSaveQuest;

    private readonly BindableValue<int> tab = new(0);
    private readonly BindableValue<int> difficulty = new(0);
    private readonly BindableValue<int> themeId = new(0);
    private readonly BindableValue<int> treasureId = new(0);
    private readonly NumberBuffer arrowRates = new(0, 0.1, 0, 1);

    public Node Render(UIStack ui, EventStack ev)
    {
        ui.Effect(ev.Listener(static (ref TowerSettingsDialog d) => d.Reset()), Open.Value);
        var modal = ui.Modal(Open, "Tower Settings", style: default(Style).W(620));
        if (!modal.IsOpen)
        {
            return modal;
        }

        if (Tower == null)
        {
            modal.Body.Child(ui.Text("No tower were loaded yet."));
            return modal;
        }

        var tabs = ui.Tabs(tab, default(Style).Grow());
        tabs.Tab(0, "Tower");
        tabs.Tab(1, "Data");
        tabs.Panel.Child(tab.Value == 0 ? TowerTab(ui, ev) : DataTab(ui, ev));
        modal.Body.Child(tabs);
        return modal;
    }

    private void Reset()
    {
        if (!Open.Value || Tower?.Theme == null)
        {
            return;
        }
        themeId.Value = Array.IndexOf(Themes.ThemeNames, Tower.Theme.ID);
        arrowRates.Value = Tower.ArrowRates;
    }

    private Node TowerTab(UIStack ui, EventStack ev)
    {
        Node panel = ui.View(Styles.Column).Child(ui.Labeled("Theme", ui.ComboBox(themeId, Themes.ThemeNames)));

        if (Tower.Type == Tower.TowerType.Versus)
        {
            Node treasures = ui.View(Styles.Panel.FlexRow().FlexWrap());
            for (int i = 0; i < Tower.Treasures.Count; i++)
            {
                treasures.Child(ui.View(Styles.Row.Px(6).Py(2).Rounded(Styles.Radius).Bg(Styles.Raised)).Child([
                    Tower.Treasures[i],
                    ui.Button(null, Styles.Small).Child(ui.Icon(FA6.Xmark))
                        .OnClick(ev.Listener(static (ref TowerSettingsDialog d, int i) => d.Tower.Treasures.RemoveAt(i), i))
                ]));
            }

            panel.Child([
                ui.Labeled("Arrow Rates", ui.NumberInput(arrowRates, Styles.Field)),
                ui.Separator("Treasures"),
                treasures,
                ui.View(Styles.Row).Child([
                    ui.ComboBox(treasureId, Pickups.PickupNames),
                    ui.Button("Add Treasure").OnClick(ev.Listener(static (ref TowerSettingsDialog d) =>
                        d.Tower.Treasures.Add(Pickups.PickupNames[d.treasureId.Value])))
                ]),
            ]);
        }

        return panel.Child(ui.Button("Save").Styled(default(Style).SelfStart())
            .OnClick(ev.Listener(static (ref TowerSettingsDialog d) =>
                d.OnSave.Invoke(new TowerData { ThemeID = d.themeId.Value, ArrowRates = d.arrowRates.FloatValue }))));
    }

    private Node DataTab(UIStack ui, EventStack ev)
    {
        if (Tower.Levels.Count == 0 || !Tower.Levels[0].LoadedIn)
        {
            return ui.Text("You must load the first level to proceed.");
        }

        var tabs = ui.Tabs(difficulty);
        tabs.Tab(0, "Normal");
        tabs.Tab(1, "Hardcore");

        var list = Quest.Waves[difficulty.Value];
        for (int i = 0; i < list.Count; i++)
        {
            tabs.Panel.Child(RenderWave(ui, ev, Tower, list[i], i + 1));
        }

        tabs.Panel.Child(ui.View(Styles.Row).Child([
            ui.Button("Add Wave").OnClick(ev.Listener(static (ref TowerSettingsDialog d) => d.Quest.Waves[d.difficulty.Value].Add(new Wave()))),
            ui.Button("Save").OnClick(ev.Listener(static (ref TowerSettingsDialog d) => d.OnSaveQuest.Invoke())),
        ]));
        return tabs;
    }

    private static Node RenderWave(UIStack ui, EventStack ev, Tower tower, Wave wave, int waveNum) 
    {
        var section = ui.Collapsible($"Wave {waveNum}", ui.OwnKeyed(wave, false), Styles.Panel);
        if (!section.IsOpen)
        {
            return section;
        }

        section.Body.Child(ui.View(Styles.Row.Gap(12)).Child([
            ui.Checkbox(wave.IsDark, "Dark").OnClick(ev.Listener(static (ref TowerSettingsDialog _, Wave w) => w.IsDark = !w.IsDark, wave)),
            ui.Checkbox(wave.IsSlow, "Slow").OnClick(ev.Listener(static (ref TowerSettingsDialog _, Wave w) => w.IsSlow = !w.IsSlow, wave)),
            ui.Checkbox(wave.IsScroll, "Scroll").OnClick(ev.Listener(static (ref TowerSettingsDialog _, Wave w) => w.IsScroll = !w.IsScroll, wave)),
        ]));

        for (int i = 0; i < wave.Groups.Count; i++)
        {
            section.Body.Child(RenderGroup(ui, ev, tower, wave.Groups[i], i + 1));
        }

        section.Body.Child(ui.Button("Add Group").Styled(default(Style).SelfStart())
            .OnClick(ev.Listener(static (ref TowerSettingsDialog _, Wave w) => w.Groups.Add(new Group()), wave)));
        return section;
    }

    private static Node RenderGroup(UIStack ui, EventStack ev, Tower tower, Group group, int groupNum)
    {
        var section = ui.Collapsible($"Group {groupNum}", ui.OwnKeyed(group, false));
        if (!section.IsOpen)
        {
            return section;
        }

        Node body = section.Body;
        body.Child(ui.Checkbox(group.IsFloor, "Floor")
            .OnClick(ev.Listener(static (ref TowerSettingsDialog _, Group g) => g.IsFloor = !g.IsFloor, group)));
        if (group.IsFloor)
        {
            body.Child(ui.Labeled("Floor Number", ui.NumberInput(group.FloorNumber, Styles.Field)));
            return section;
        }

        body.Child([
            ui.Labeled("Delay", ui.NumberInput(group.Delay, Styles.Field)),
            ui.View(Styles.Row.Gap(12)).Child([
                ui.Checkbox(group.Solo, "Solo").OnClick(ev.Listener(static (ref TowerSettingsDialog _, Group g) => g.Solo = !g.Solo, group)),
                ui.Checkbox(group.CoOp, "CoOp").OnClick(ev.Listener(static (ref TowerSettingsDialog _, Group g) => g.CoOp = !g.CoOp, group)),
            ]),
            ui.Separator("Treasure"),
        ]);

        foreach (var treasure in group.TreasureIDs)
        {
            body.Child(ui.ComboBox(treasure, Pickups.PickupNamesWithNone));
        }
        body.Child(ui.Button("Add Treasure").Styled(default(Style).SelfStart())
            .OnClick(ev.Listener(static (ref TowerSettingsDialog _, Group g) =>
                g.TreasureIDs.Add(new(Array.IndexOf(Pickups.PickupNamesWithNone, "None"))), group)));

        body.Child(ui.Separator("Spawns"));
        if (group.Dirty)
        {
            QuestData.RefreshSpawn(tower, group.Spawns);
            group.Dirty = false;
        }
        Node spawns = ui.View(Styles.Row.FlexWrap());
        string previous = "";
        for (int i = 0; i < group.Spawns.Count; i++)
        {
            var spawn = group.Spawns[i];
            if (previous == spawn.Name)
            {
                spawns.Child(ui.Text("Found duplicated portals.").Styled(Styles.Muted));
                continue;
            }
            previous = spawn.Name;
            spawns.Child(ui.Checkbox(spawn.IsChecked, spawn.Name).Styled(default(Style).W(120))
                .OnClick(ev.Listener(static (ref TowerSettingsDialog _, (Group g, int i) a) => a.g.ToggleSpawn(a.i), (group, i))));
        }
        body.Child(spawns);

        body.Child(ui.Separator("Enemies"));
        foreach (var enemy in group.Enemies)
        {
            body.Child(ui.TextInput(enemy, "Enemy Name", Styles.Field.W(Length.Full)));
        }
        body.Child(ui.Button("Add Enemies").Styled(default(Style).SelfStart())
            .OnClick(ev.Listener(static (ref TowerSettingsDialog _, Group g) => g.Enemies.Add(new TextBuffer("Slime")), group)));
        return section;
    }
}

public class Wave 
{
    public bool IsDark;
    public bool IsSlow;
    public bool IsScroll;
    public List<Group> Groups = new List<Group>();
}

public class Group 
{
    public NumberBuffer FloorNumber = new(0, 1, integer: true);
    public bool IsFloor;

    public List<Spawn> Spawns = [];
    public List<TextBuffer> Enemies = [];
    public List<BindableValue<int>> TreasureIDs = [];
    public NumberBuffer Delay = new(0, 1, 0, integer: true);
    public bool Solo;
    public bool CoOp;

    public bool Dirty = true;

    public Group() {}

    public Group(int floorNum) 
    {
        IsFloor = true;
        FloorNumber.Value = floorNum;
    }

    public void ToggleSpawn(int index)
    {
        var spawn = Spawns[index];
        Spawns[index] = new Spawn { Name = spawn.Name, IsChecked = !spawn.IsChecked };
    }

    public struct Spawn 
    {
        public string Name;
        public bool IsChecked;
    }
}
