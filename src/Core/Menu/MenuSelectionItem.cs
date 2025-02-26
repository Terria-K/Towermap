using ImGuiNET;

namespace Towermap;

public class MenuSelectionItem : ImGuiElement
{
    public string Name;
    public MenuSelectionItem(string name) 
    {
        Name = name;
    }

    public override void DrawGui()
    {
        if (ImGui.BeginMenu(Name))
        {
            UpdateChildrens();
            ImGui.EndMenu();
        }
    }
}