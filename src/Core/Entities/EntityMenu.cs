using System;
using System.Numerics;
using ImGuiNET;

namespace Towermap;

public class EntityMenu : ImGuiElement
{
    private ActorManager manager;
    private IntPtr imGuiTexture;
    public Action<Actor> OnSelectActor;
    public EntityMenu(ActorManager manager, IntPtr imGuiTexture) 
    {
        this.manager = manager;
        this.imGuiTexture = imGuiTexture;
    }

    public override void DrawGui()
    {
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);

        ImGui.SetNextWindowSize(new Vector2(1280 - 50, 620 - 50), ImGuiCond.Always);
        ImGui.SetNextWindowPos(mainViewport.Pos + new Vector2(25, 25));
        ImGui.Begin("Entity Menu", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.Modal);

        if (ImGui.BeginTabBar("TagTab"))
        {
            if (ImGui.BeginTabItem("All"))
            {
                ImGui.Columns(10, "col-All", false);
                foreach (var (name, actor) in manager.Actors)
                {
                    if (ImGui.ImageButton(actor.Name, imGuiTexture, new Vector2(40, 40) * 1.5f, 
                        actor.Texture.UV.TopLeft, actor.Texture.UV.BottomRight)) 
                    {
                        OnSelectActor?.Invoke(actor);
                        Enabled = false;
                    }

                    ImGui.Text(name);

                    ImGui.NextColumn();
                }
                ImGui.Columns(1);

                ImGui.EndTabItem();
            }
            foreach (var actorTagged in manager.ActorTagged) 
            {
                if (ImGui.BeginTabItem(actorTagged.Key))
                {
                    ImGui.Columns(10, "col-" + actorTagged.Key, false);
                    foreach (var actor in actorTagged.Value)
                    {
                        if (ImGui.ImageButton(actor.Name, imGuiTexture, new Vector2(40, 40) * 1.5f, 
                            actor.Texture.UV.TopLeft, actor.Texture.UV.BottomRight)) 
                        {
                            OnSelectActor?.Invoke(actor);
                            Enabled = false;
                        }

                        ImGui.Text(actor.Name);

                        ImGui.NextColumn();
                    }
                    ImGui.Columns(1);

                    ImGui.EndTabItem();
                }

            }
            ImGui.EndTabBar();
        }


        ImGui.End();
    }
}