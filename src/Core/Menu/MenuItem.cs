using System;
using ImGuiNET;

namespace Towermap;

public class MenuItem : ImGuiElement
{
    public string Name;
    public Action OnCallback;
    public Action<bool> OnCallbackToggle;
    private bool selected;
    private string shortcut;
    public MenuItem(string name, Action onCallback = null) 
    {
        Name = name;
        OnCallback = onCallback;
    }

    public MenuItem(string name, bool defaultValue, Action<bool> onCallback = null) 
    {
        Name = name;
        OnCallbackToggle = onCallback;
        selected = defaultValue;
    }

    public override void DrawGui()
    {
        if (OnCallbackToggle != null)
        {
            if (ImGui.MenuItem(Name, null, ref selected))
            {
                OnCallbackToggle(selected);
            }
            return;
        }
        if (ImGui.MenuItem(Name))
        {
            OnCallback?.Invoke();
        }
    }
}
