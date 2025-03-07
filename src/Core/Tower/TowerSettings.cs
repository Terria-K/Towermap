using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml;
using ImGuiNET;
using Riateu;

namespace Towermap;

public struct TowerData 
{
    public int ThemeID;
    public int TreasureID;
    public float ArrowRates;
}

public class TowerSettings : ImGuiElement
{
    public Theme Theme => theme;
    private Theme theme;
    private string themeName = "";
    private TowerData towerData;
    private Tower tower;
    private List<Wave>[] waves = new List<Wave>[2];
    public Action<TowerData> OnSave;

    public TowerSettings() 
    {
        Enabled = false;
        waves = [
            new List<Wave>(),
            new List<Wave>()
        ];
    }

    public void SetData(XmlElement data) 
    {
        waves[0].Clear();
        waves[1].Clear();

        SetWaveData(waves[0], data["normal"]);
        SetWaveData(waves[1], data["hardcore"]);
    }

    public void SetWaveData(List<Wave> waves, XmlElement data) 
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

                    var treasureList = new List<int>();
                    if (group.TryGetElement("treasure", out XmlElement treasureXml))
                    {
                        ReadOnlySpan<char> treasureText = treasureXml.InnerText.AsSpan().Trim();
                        var splitted = treasureText.Split(',');

                        foreach (var range in splitted)
                        {
                            var trimmed = treasureText[range].Trim();
                            treasureList.Add(Array.IndexOf(Pickups.PickupNames, new string(trimmed)));
                        }
                    }

                    var enemies = new List<string>();
                    if (group.TryGetElement("enemies", out XmlElement enemiesXml))
                    {
                        ReadOnlySpan<char> enemiesText = enemiesXml.InnerText.AsSpan().Trim();
                        char splitters = ',';

                        var count = enemiesText.Count(splitters) + 1;

                        var list = new List<string>(count);

                        var splitted = enemiesText.Split(splitters);
                        foreach (var range in splitted)
                        {
                            var trimmed = enemiesText[range];
                            list.Add(new string(trimmed.Trim()));                           
                        }
                        enemies = list;
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
                        Delay = delay,
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
        foreach (var diffWave in waves)
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

    private void RefreshSpawn(List<Group.Spawn> spawns)
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

    public void SetTower(Tower tower) 
    {
        this.tower = tower;
        towerData.ArrowRates = tower.ArrowRates;
    }
    
    public void SetTheme(Theme theme) 
    {
        this.theme = theme;
        themeName = theme.ID;
        towerData.ThemeID = Array.IndexOf(Themes.ThemeNames, themeName);
    }

    public override void DrawGui()
    {
        ImGui.SetNextWindowSize(new Vector2(520, 520), ImGuiCond.FirstUseEver);
        if (ImGui.BeginPopupModal("Theme Settings", ref enabled)) 
        {
            if (ImGui.BeginTabBar("Tabbar")) 
            {
                if (ImGui.BeginTabItem("Tower")) 
                {
                    if (tower == null)
                    {
                        ImGui.Text("No tower were loaded yet.");
                        ImGui.EndTabItem();
                        ImGui.EndTabBar();
                        ImGui.EndPopup();
                        return;
                    }
                    ImGui.Combo("Theme", ref towerData.ThemeID, Themes.ThemeNames, Themes.ThemeNames.Length);

                    switch (tower.Type)
                    {
                    case Tower.TowerType.Quest:
                        // Nothing on Quest
                        break;
                    case Tower.TowerType.Versus:
                        if (ImGui.InputFloat("Arrow Rates", ref towerData.ArrowRates, 0.1f)) 
                        {
                            towerData.ArrowRates = Math.Clamp(towerData.ArrowRates, 0, 1);
                        }

                        ImGui.BeginTable("treasure_table", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders);
                        List<string> toRemove = [];
                        int i = 0;
                        foreach (var treasure in tower.Treasures)
                        {
                            ImGui.Text(treasure);
                            ImGui.SameLine();
                            ImGui.PushID(treasure + "_rmbtn" + i);
                            if (ImGui.Button("Remove"))
                            {
                                toRemove.Add(treasure);
                            }
                            ImGui.PopID();
                            ImGui.TableNextColumn();
                            i += 1;
                        }

                        foreach (var removing in toRemove)
                        {
                            tower.Treasures.Remove(removing);
                        }

                        ImGui.EndTable();
                        ImGui.Combo("Treasure", ref towerData.TreasureID, Pickups.PickupNames, Pickups.PickupNames.Length);
                        if (ImGui.Button("Add Treasure")) 
                        {
                            tower.Treasures.Add(Pickups.PickupNames[towerData.TreasureID]);
                        }
                        break;
                    }


                    if (ImGui.Button("Save")) 
                    {
                        tower.ArrowRates = towerData.ArrowRates;
                        tower.Save();
                        OnSave?.Invoke(towerData);
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Data")) 
                {
                    if (!tower.Levels[0].LoadedIn)
                    {
                        ImGui.Text("You must load the first level to proceed.");
                        ImGui.EndTabItem();
                        ImGui.EndTabBar();
                        ImGui.EndPopup();
                        return;
                    }
                    if (ImGui.BeginTabBar("DifficultyTab"))
                    {
                        for (int i = 0; i < waves.Length; i++)
                        {
                            if (ImGui.BeginTabItem(i == 0 ? "Normal" : "Hardcore"))
                            {
                                int waveNum = 1;
                                foreach (var wave in waves[i]) 
                                {
                                    RenderWave(wave, waveNum);

                                    waveNum += 1;
                                }

                                ImGui.Separator();
                                if (waves[i].Count == 0)
                                {
                                    if (ImGui.Button("Copy From " + (i != 0 ? "Normal" : "Hardcore"))) 
                                    {
                                    }
                                }
                                if (ImGui.Button("Add Wave")) 
                                {
                                    waves[i].Add(new Wave());
                                }

                                if (ImGui.Button("Save")) 
                                {
                                    SaveData();
                                }

                                ImGui.EndTabItem();
                            }
                        }

                        ImGui.EndTabBar();
                    }

                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }

            ImGui.EndPopup();
        }
    }

    private void RenderWave(Wave wave, int waveNum) 
    {
        if (ImGui.CollapsingHeader($"Wave {waveNum}"))
        {
            ImGui.PushID(waveNum);
            ImGui.Checkbox("Dark", ref wave.IsDark);
            ImGui.SameLine();
            ImGui.Checkbox("Slow", ref wave.IsSlow);
            ImGui.SameLine();
            ImGui.Checkbox("Scroll", ref wave.IsScroll);
            ImGui.PopID();

            int groupNum = 1;
            foreach (var group in wave.Groups) 
            {
                ImGui.PushID($"Wave{waveNum}-Group{groupNum}");
                if (ImGui.TreeNode($"Group {groupNum}")) 
                {
                    ImGui.Checkbox("Floor", ref group.IsFloor);
                    if (group.IsFloor)
                    {
                        ImGui.InputInt("Floor Number", ref group.FloorNumber);
                    }
                    else 
                    {
                        if (ImGui.InputInt("Delay", ref group.Delay, 1)) 
                        {
                            group.Delay = Math.Max(0, group.Delay);
                        }
                        ImGui.Checkbox("Solo", ref group.Solo);
                        ImGui.SameLine();
                        ImGui.Checkbox("CoOp", ref group.CoOp);
                        
                        ImGui.SeparatorText("Treasure");
                        for (int i = 0; i < group.TreasureIDs.Count; i++)
                        {
                            var treasureID = group.TreasureIDs[i];
                            ImGui.PushID(i);
                            if (ImGui.Combo("Select Treasure", ref treasureID, Pickups.PickupNamesWithNone, Pickups.PickupNamesWithNone.Length)) 
                            {
                                group.TreasureIDs[i] = treasureID;
                            }
                            ImGui.PopID();
                        }
                        if (ImGui.Button("Add Treasure"))
                        {
                            group.TreasureIDs.Add(20);
                        }
                        ImGui.SeparatorText("Spawns");
                        if (tower.Levels.Count == 0)
                        {
                            ImGui.Text("Level must have at least 1 level for spawns");
                        }
                        else 
                        {
                            string currentSpawner = "";
                            if (group.Dirty)
                            {
                                RefreshSpawn(group.Spawns);
                                group.Dirty = false;
                            }
                            ImGui.Columns(4, "spawn_column", false);
                            for (int i = 0; i < group.Spawns.Count; i++)
                            {
                                var spawn = group.Spawns[i];
                                var name = spawn.Name;
                                if (currentSpawner == name)
                                {
                                    ImGui.Text("Found duplicated portals.");
                                }
                                else 
                                {
                                    bool isChecked = spawn.IsChecked;
                                    if (ImGui.Checkbox(spawn.Name, ref isChecked)) 
                                    {
                                        group.Spawns[i] = new Group.Spawn()
                                        {
                                            Name = spawn.Name,
                                            IsChecked = isChecked
                                        };
                                    }
                                    ImGui.NextColumn();
                                    currentSpawner = spawn.Name;
                                }
                            }

                            ImGui.Columns(1);
                        }

                        ImGui.SeparatorText("Enemies");
                        for (int i = 0; i < group.Enemies.Count; i++)
                        {
                            var enemy = group.Enemies[i];
                            ImGui.PushID(i);
                            if (ImGui.InputText("Enemy Name", ref enemy, 100))
                            {
                                group.Enemies[i] = enemy;
                            }
                            ImGui.PopID();
                        }
                        
                        if (ImGui.Button("Add Enemies")) 
                        {
                            group.Enemies.Add("Slime");
                        }
                    }
                    ImGui.TreePop();
                }


                ImGui.PopID();
                groupNum += 1;
            }
            ImGui.PushID(waveNum);

            ImGui.Spacing();
            ImGui.Spacing();

            if (ImGui.Button("Add Group")) 
            {
                wave.Groups.Add(new Group());
            }
            ImGui.PopID();
        }
    }

    public void SaveData()
    {
        XmlDocument document = new XmlDocument();
        var data = document.CreateElement("data");
        document.AppendChild(data);

        int i = 0;
        foreach (var wave in waves)
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
                        floorXml.InnerText = group.FloorNumber.ToString();
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
                    if (group.Delay != 0)
                    {
                        groupXml.SetAttribute("delay", group.Delay.ToString());
                    }
                    var listStr = new List<string>();
                    if (group.TreasureIDs.Count > 0)
                    {
                        var treasureXml = document.CreateElement("treasure");
                        foreach (var treasure in group.TreasureIDs)
                        {
                            var name = Pickups.PickupNamesWithNone[treasure];
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
                        foreach (var spawn in group.Enemies)
                        {
                            var name = spawn;
                            listStr.Add(name);
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

        document.Save(Path.Combine(Path.GetDirectoryName(tower.TowerPath), "data.xml"));
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
    // if the group is floor, this will be used
    public int FloorNumber;
    public bool IsFloor;

    // if not, then this one
    public List<Spawn> Spawns;
    public List<string> Enemies;
    public List<int> TreasureIDs = [];
    public int Delay;
    public bool Solo;
    public bool CoOp;

    // Some state outside of its data
    public bool Dirty = true;

    public Group(bool isFloor = false) 
    {
        if (!isFloor) 
        {
            Spawns = new List<Spawn>();
            Enemies = new List<string>();
        }
    }

    public Group(int floorNum) 
    {
        IsFloor = true;
        FloorNumber = floorNum;
    }

    public struct Spawn 
    {
        public string Name;
        public bool IsChecked;
    }
}