using System;
using ImGuiNET;

namespace Towermap;

public class EntityData : ImGuiElement
{
    private string entityName = string.Empty;
    private LevelActor levelActor;
    private int actorNewWidth;
    private int actorNewHeight;
    private int[] actorNewPos = new int[2];
    private string id = string.Empty;

    public EntityData() 
    {
    }

    public void SelectActor(LevelActor levelActor)
    {
        this.levelActor = levelActor;
        entityName = this.levelActor.Data.Name;
        actorNewPos[0] = (int)this.levelActor.PosX;
        actorNewPos[1] = (int)this.levelActor.PosY;
        if (levelActor.Data.ResizeableX) 
        {
            actorNewWidth = this.levelActor.Width;
        }

        if (levelActor.Data.ResizeableY) 
        {
            actorNewHeight = this.levelActor.Height;
        }
        id = levelActor.ID.ToString();
    }

    public override void DrawGui()
    {
        ImGui.BeginChild("Entities");
        ImGui.SeparatorText(entityName);
        if (levelActor != null) 
        {
            ImGui.LabelText("ID", id);
            ImGui.InputInt2("Position", ref actorNewPos[0]);
            if (levelActor.Data.ResizeableX) 
            {
                ImGui.SliderInt("Width", ref actorNewWidth, 10, 320);
            }
            if (levelActor.Data.ResizeableY) 
            {
                ImGui.SliderInt("Height", ref actorNewHeight, 10, 320);
            }

            foreach (var (key, value) in levelActor.CustomData) 
            {
                if (value is bool val) 
                {
                    if (ImGui.Checkbox(key, ref val)) 
                    {
                        levelActor.CustomData[key] = val;
                    }
                }
            }
        }

        ImGui.EndChild();
    }
}