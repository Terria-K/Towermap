using ImGuiNET;

namespace Towermap;

public class MenuBar : ImGuiElement
{
    public MenuBar() 
    {

    }

    public override void DrawGui()
    {
        ImGui.BeginMainMenuBar();
        UpdateChildrens();
        ImGui.EndMainMenuBar();
    }
}