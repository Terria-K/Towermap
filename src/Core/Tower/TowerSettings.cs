using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using ImGuiNET;
using Riateu;

namespace Towermap;

public class TowerSettings : ImGuiElement
{
    public Theme Theme => theme;
    private Theme theme;
    private string themeName = "";
    private int currentTheme;
    private List<Wave>[] waves = new List<Wave>[2];
    public Action<int> OnSave;

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
            bool dark = AttrBool(wave, "dark");
            bool scroll = AttrBool(wave, "scroll");
            bool slow = AttrBool(wave, "slow");
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
                    w.Groups.Add(new Group() 
                    {

                    });
                }
            }

            waves.Add(w);
        }
    }

    private bool AttrBool(XmlElement element, string name, bool defaultValue = false) 
    {
        if (bool.TryParse(element.GetAttribute(name), out bool res))
        {
            return res;
        }

        return defaultValue;
    }
    
    public void SetTheme(Theme theme) 
    {
        this.theme = theme;
        themeName = theme.Name;
        currentTheme = Array.IndexOf(Themes.ThemeNames, themeName);
    }

    public override void DrawGui()
    {
        ImGui.SetNextWindowSize(new Vector2(520, 520));
        if (ImGui.BeginPopupModal("Theme Settings", ref enabled)) 
        {
            if (ImGui.BeginTabBar("Tabbar")) 
            {
                if (ImGui.BeginTabItem("Tower")) 
                {
                    ImGui.Combo("Theme", ref currentTheme, Themes.ThemeNames, Themes.ThemeNames.Length);
                    if (ImGui.Button("Save")) 
                    {
                        OnSave?.Invoke(currentTheme);
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Data")) 
                {
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
                                if (ImGui.Button("Add Wave")) 
                                {
                                    waves[i].Add(new Wave());
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
                        ImGui.SeparatorText("Treasure");
                        ImGui.Combo("Select Treasure", ref group.TreasureID, Pickups.PickupNames, Pickups.PickupNames.Length);
                        ImGui.SeparatorText("Spawns");
                        ImGui.Text("Tower must have a level name called 'level.oel'");
                        ImGui.SeparatorText("Enemies");
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
    public List<string> Spawns;
    public List<string> Enemies;
    public int TreasureID;


    public Group(bool isFloor = false) 
    {
        if (!isFloor) 
        {
            Spawns = new List<string>();
            Enemies = new List<string>();
        }
    }

    public Group(int floorNum) 
    {
        IsFloor = true;
        FloorNumber = floorNum;
    }
}