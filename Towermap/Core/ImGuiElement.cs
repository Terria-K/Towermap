using System.Collections.Generic;

namespace Towermap;

public abstract class ImGuiElement 
{
    private List<ImGuiElement> childrens = [];
    public bool Enabled { get => enabled; set => enabled = value; }
    protected bool enabled = true;
    
    public ImGuiElement Add(ImGuiElement element) 
    {
        childrens.Add(element);
        return this;
    }

    public abstract void DrawGui();

    protected void UpdateChildrens() 
    {
        foreach (var child in childrens) 
        {
            child.UpdateGui();
        }
    }

    public void UpdateGui() 
    {
        if (enabled) 
        {
            DrawGui();
        }
    }
}