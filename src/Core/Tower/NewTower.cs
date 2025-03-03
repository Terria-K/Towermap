using System;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public sealed class NewTower : ImGuiElement
{
    private string towerName = "";
    private string towerPath = "";
    private int towerMode;
    private int currentTheme;
    private string[] modes;

    public Action<string, string, int, int> OnCreateTower;

    public NewTower(SaveState saveState) 
    {
        if (saveState.DarkWorld)
        {
            modes = ["Versus", "Quest", "DarkWorld"];
        }
        else 
        {
            modes = ["Versus", "Quest"];
        }
    }

    public override void DrawGui()
    {
        ImGui.SetNextWindowSize(new Vector2(520, 320));
        if (ImGui.BeginPopupModal("New Tower", ref enabled)) 
        {
            ImGui.InputText("Tower Name", ref towerName, 100);
            ImGui.InputText("Tower Path", ref towerPath, 200);
            if (ImGui.Button("Browse")) 
            {
                FileDialog.OpenFolder((filepath) => towerPath = filepath, null);
            }
            ImGui.Combo("Tower Mode", ref towerMode, modes, modes.Length);
            ImGui.Combo("Theme", ref currentTheme, Themes.ThemeNames, Themes.ThemeNames.Length);

            bool condition = string.IsNullOrEmpty(towerName) || string.IsNullOrEmpty(towerPath);

            if (condition)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Create"))
            {
                OnCreateTower?.Invoke(towerName, towerPath, towerMode, currentTheme);
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