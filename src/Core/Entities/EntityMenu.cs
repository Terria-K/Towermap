using System;
using System.Collections.Generic;
using Cosmiria;

namespace Towermap;

public struct EntityMenu() : IComponent
{
    private static readonly Style Grid = default(Style).GridColsFill(120).Gap(6);
    private static readonly Style Cell = default(Style).FlexCol().ItemsCenter().Gap(4).MinW(0).Padding(6).Rounded(Styles.Radius).Bg(Styles.Raised).Border(1, Styles.Line);
    private static readonly Style CellHover = default(Style).Bg(Styles.Hover);
    private static readonly Style CellName = default(Style).TextSizeOf(12).TextCenter().MaxW(Length.Full);

    public BindableValue<bool> Open;
    public Callback<Actor> OnSelect;

    private readonly BindableValue<int> tab = new(0);

    public Node Render(UIStack ui, EventStack ev)
    {
        var modal = ui.Modal(Open, "Entities", style: default(Style).W(Length.Percent(90)).H(Length.Percent(90)));
        if (!modal.IsOpen)
        {
            return modal;
        }

        var tabs = ui.Tabs(tab, default(Style).Grow().MinH(0));
        tabs.Tab(0, "All");
        int index = 1;
        foreach (var tag in ActorManager.ActorTagged.Keys)
        {
            tabs.Tab(index++, tag);
        }

        Node grid = ui.View(Grid);
        if (tab.Value == 0)
        {
            foreach (var actor in ActorManager.Actors.Values)
            {
                grid.Child(Item(ui, ev, actor));
            }
        }
        else
        {
            foreach (var actor in TaggedActors(tab.Value - 1))
            {
                grid.Child(Item(ui, ev, actor));
            }
        }

        tabs.Panel.Styled(default(Style).Grow().MinH(0).OverflowScroll()).Child(grid);
        modal.Body.Styled(default(Style).Grow()).Child(tabs);
        return modal;
    }

    private static List<Actor> TaggedActors(int index)
    {
        foreach (var list in ActorManager.ActorTagged.Values)
        {
            if (index-- == 0)
            {
                return list;
            }
        }
        return [];
    }

    private static Node Item(UIStack ui, EventStack ev, Actor actor) =>
        ui.View(Cell).Hover(CellHover).Tooltip(actor.Name)
            .OnClick(ev.Listener(static (ref EntityMenu m, Actor a) => m.Select(a), actor))
            .Child([
                ui.AtlasImage(actor.Texture, ImageFit.Contain, default(Style).Size(56)),
                ui.Text(actor.Name).Styled(CellName)
            ]);

    private void Select(Actor actor)
    {
        OnSelect.Invoke(actor);
        Open.Value = false;
    }
}
