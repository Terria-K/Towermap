using Cosmiria;

namespace Towermap;

public struct HistoryPanel : IComponent
{
    private static readonly Style Undone = default(Style).Opacity(0.45f);

    public History History;
    public string LevelName;
    public Callback<int> OnJump;

    public Node Render(UIStack ui, EventStack ev)
    {
        if (History == null)
        {
            return ui.Text("No level loaded.").Styled(Styles.Muted);
        }

        Node list = ui.View(Styles.Column.Gap(2));
        for (int position = History.Commands.Count; position > 0; position--)
        {
            Node item = Entry(ui, ev, position).Child(ui.Text(History.Commands[position - 1].Name));
            list.Child(position > History.Position ? item.Styled(Undone) : item);
        }
        return list.Child(Entry(ui, ev, 0).Child([ui.Icon(FA6.FolderOpen), LevelName]));
    }

    private readonly Node Entry(UIStack ui, EventStack ev, int position) =>
        ui.Selectable(History.Position == position)
            .OnClick(ev.Listener(static (ref HistoryPanel h, int p) => h.OnJump.Invoke(p), position));
}
