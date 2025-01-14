using ImGuiNET;

namespace Towermap;

public class MenuSlot : ImGuiElement
{
    public string Name;

    public MenuSlot(string name) 
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