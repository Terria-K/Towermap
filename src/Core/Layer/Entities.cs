using System;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class Entities : ImGuiElement
{
    private IntPtr imGuiTexture;
    private string searchText = string.Empty;
    public Action<Actor> OnSelectActor;
    public Entities(IntPtr imGuiTexture) 
    {
        this.imGuiTexture = imGuiTexture;
    }

    public override void DrawGui()
    {
        ImGui.BeginChild("Entities");
        ImGui.SeparatorText("Entities");

        ImGui.InputText("Search", ref searchText, 50);

        ImGui.PushItemWidth(-1);
        ImGui.BeginListBox("##Entities", new Vector2(180, 200));
        foreach (var (name, actor) in ActorManager.Actors) 
        {
            if (string.IsNullOrEmpty(searchText) || name.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) 
            {
                if (ImGui.ImageButton(name, imGuiTexture, new Vector2(20, 20) * 1.5f, 
                    actor.Texture.UV.TopLeft, actor.Texture.UV.BottomRight)) 
                {
                    OnSelectActor?.Invoke(actor);
                }
                ImGui.SameLine();
                ImGui.Text(name);
            }
        }
        ImGui.EndListBox();
        ImGui.PopItemWidth();

        ImGui.EndChild();    
    }
}