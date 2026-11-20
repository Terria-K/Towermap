using Cosmiria;

namespace Towermap;

public struct LayerList : IComponent
{
    private static readonly (Layers Layer, string Icon, string Name)[] layers = [
        (Layers.Solids, FA6.BorderAll, "Solids"),
        (Layers.BG, FA6.BorderNone, "BG"),
        (Layers.Entities, FA6.Person, "Entities"),
        (Layers.SolidTiles, FA6.SquareFull, "SolidTiles"),
        (Layers.BGTiles, FA6.SquareMinus, "BGTiles")
    ];

    public Layers Current;
    public bool[] Visibility;
    public Callback<Layers> OnSelect;
    public Callback<Layers> OnToggleVisibility;

    public Node Render(UIStack ui, EventStack ev)
    {
        Node list = ui.View(Styles.Column.Gap(2));
        foreach (var (layer, icon, name) in layers)
        {
            list.Child(ui.View(Styles.Row).Child([
                ui.Selectable(layer == Current)
                    .OnClick(ev.Listener(static (ref LayerList l, Layers layer) => l.OnSelect.Invoke(layer), layer))
                    .Styled(default(Style).Grow())
                    .Child([ui.Icon(icon), name]),
                ui.Button(null, Styles.Small)
                    .Child(ui.Icon(Visibility[(int)layer] ? FA6.Eye : FA6.EyeSlash))
                    .OnClick(ev.Listener(static (ref LayerList l, Layers layer) => l.OnToggleVisibility.Invoke(layer), layer)),
            ]));
        }
        return list;
    }
}
