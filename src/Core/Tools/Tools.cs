using Cosmiria;

namespace Towermap;

public enum ToolAction
{
    Pen,
    Rect,
    Node,
    MirrorHorizontal,
    MirrorVertical,
    Symmetry
}

public struct Toolbar : IComponent
{
    private static readonly (ToolAction Action, string Icon, string Name, int Section)[] tools = [
        (ToolAction.Pen, FA6.Pen, "Pen (1)", 0),
        (ToolAction.Rect, FA6.UpDownLeftRight, "Select (2)", 0),
        (ToolAction.Node, FA6.CircleNodes, "Nodes (5)", 0),
        (ToolAction.MirrorHorizontal, FA6.LeftRight, "Mirror Horizontally (Shift+H)", 1),
        (ToolAction.MirrorVertical, FA6.UpDown, "Mirror Vertically (Shift+V)", 1),
        (ToolAction.Symmetry, FA6.PenRuler, "Symmetry", 2),
    ];

    private static readonly Style Button = default(Style).Size(32).Padding(0);
    private static readonly Style Active = Styles.Accent.Size(32).Padding(0);
    private static readonly Style Divider = default(Style).W(1).H(20).Mx(4).Bg(Styles.Line);

    public Tool Selected;
    public bool Symmetry;
    public Callback<ToolAction> OnAction;

    public Node Render(UIStack ui, EventStack ev)
    {
        Node row = ui.View(Styles.Row.Gap(4));
        int section = 0;
        foreach (var (action, icon, name, toolSection) in tools)
        {
            if (toolSection != section)
            {
                row.Child(ui.View(Divider));
                section = toolSection;
            }
            row.Child(ui.Button(null, IsActive(action) ? Active : Button)
                .Tooltip(name)
                .Child(ui.Icon(icon))
                .OnClick(ev.Listener(static (ref Toolbar t, ToolAction a) => t.OnAction.Invoke(a), action)));
        }
        return row;
    }

    private readonly bool IsActive(ToolAction action) => action switch
    {
        ToolAction.Pen => Selected == Tool.Pen,
        ToolAction.Rect => Selected == Tool.Rect,
        ToolAction.Node => Selected == Tool.Node,
        ToolAction.Symmetry => Symmetry,
        _ => false
    };
}
