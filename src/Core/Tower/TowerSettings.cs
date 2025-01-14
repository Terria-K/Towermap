using System;
using ImGuiNET;
using Riateu;

namespace Towermap;

public class TowerSettings : ImGuiElement
{
    public Theme Theme => theme;
    private Theme theme;
    private string themeName = "";
    private int currentTheme;
    private string path;
    public Action<int> OnSave;

    public TowerSettings() 
    {
        Enabled = false;
    }
    
    public void SetTheme(Theme theme) 
    {
        this.theme = theme;
        themeName = theme.Name;
        currentTheme = Array.IndexOf(Themes.ThemeNames, themeName);
    }

    public override void DrawGui()
    {
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
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }

            ImGui.EndPopup();
        }
    }
}