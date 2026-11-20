using System;
using Cosmiria;

namespace Towermap;

public struct EntityList() : IComponent
{
    public Actor Selected;
    public Callback<Actor> OnSelect;

    private readonly TextBuffer search = new();

    public Node Render(UIStack ui, EventStack ev)
    {
        Node list = ui.View(Styles.Panel.H(220).OverflowScroll());
        string filter = search.ToString();
        foreach (var (name, actor) in ActorManager.Actors)
        {
            if (filter.Length > 0 && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            list.Child(ui.Selectable(actor == Selected)
                .OnClick(ev.Listener(static (ref EntityList l, Actor a) => l.OnSelect.Invoke(a), actor))
                .Child([
                    ui.AtlasImage(actor.Texture, ImageFit.Contain, default(Style).Size(24).Shrink(0)),
                    name
                ]));
        }

        return ui.View(Styles.Column).Child([
            ui.Separator("Entities"),
            ui.TextInput(search, "Search", Styles.Field.W(Length.Full)),
            list
        ]);
    }
}
