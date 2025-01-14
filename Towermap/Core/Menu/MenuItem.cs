using System;
using ImGuiNET;

namespace Towermap;

public class MenuItem : ImGuiElement
{
    public string Name;
    public Action OnCallback;
    public MenuItem(string name, Action onCallback = null) 
    {
        Name = name;
        OnCallback = onCallback;
    }
    public override void DrawGui()
    {
        if (ImGui.MenuItem(Name))
        {
            OnCallback?.Invoke();
        }
    }
}