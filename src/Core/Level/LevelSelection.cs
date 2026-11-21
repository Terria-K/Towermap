using System.Linq;
using Cosmiria;

namespace Towermap;

public struct LevelList : IComponent
{
    public Tower Tower;
    public Level Current;
    public Callback<Level> OnSelect;
    public Callback OnAdd;
    public Callback<Level> OnDelete;

    public Node Render(UIStack ui, EventStack ev)
    {
        if (Tower == null)
        {
            return ui.Text("No tower loaded.").Styled(Styles.Muted);
        }

        Node list = ui.View(Styles.Column.Gap(2));
        foreach (var level in Tower.Levels)
        {
            var nodeLevel = ui.Selectable(level == Current, level.Unsaved ? level.FileName + "*" : level.FileName)
                .OnClick(ev.Listener(static (ref LevelList l, Level level) => l.OnSelect.Invoke(level), level));
            list.Child(nodeLevel);

            var menu = ui.ContextMenu(nodeLevel);
            if (menu.IsOpen)
            {
                menu.Item("Delete", ev.Listener(static (ref LevelList list, Level level) =>
                {
                    list.OnDelete.Invoke(level);
                }, level));
            }

            nodeLevel.Child(menu.Popup);
        }
        return list.Child(ui.Selectable(false).OnClick(ev.Listener(static (ref LevelList l) => l.OnAdd.Invoke()))
            .Child([ui.Icon(FA6.Plus), "Add Level"]));
    }
}

public struct DeleteLevelDialog() : IComponent
{
    public BindableValue<bool> Open;
    public BindableValue<Level> Level;
    public Callback OnLevelDelete;

    public Node Render(UIStack ui, EventStack ev)
    {
        var modal = ui.Modal(Open, "Delete Level", style: ui.Style().W(320));
        if (modal.IsOpen)
        {
            modal.Body.Child([
                ui.View(ui.Style().Gap(4).FlexCol().JustifyBetween().ItemsCenter()).Child([
                    ui.Text($"Are you sure you want to delete this level? {Level.Value.FileName}").Styled(ui.Style().TextBase()),
                    ui.Separator(),
                    ui.Separator(),
                    ui.View(ui.Style().Gap(4))
                        .Child([
                            ui.Button("OK")
                                .OnClick(ev.Listener(static (ref DeleteLevelDialog self) => 
                                { 
                                    self.OnLevelDelete.Invoke(); 
                                    self.Open.Value = false;
                                })),
                            ui.Button("Cancel")
                                .OnClick(ev.Listener(static (ref DeleteLevelDialog self) => self.Open.Value = false)),
                        ])
                ])
            ]);
        }

        return modal;
    }
}

public readonly record struct NewLevel(string Name, int Width);

public struct NewLevelDialog() : IComponent
{
    private static readonly string[] Widths = ["320", "420"];

    public BindableValue<bool> Open;
    public Tower Tower;
    public Callback<NewLevel> OnCreate;

    private readonly BindableValue<int> width = new(0);
    private readonly TextBuffer name = new();

    public Node Render(UIStack ui, EventStack ev)
    {
        ui.Effect(ev.Listener(static (ref NewLevelDialog d) => d.name.Clear()), Open.Value);
        var modal = ui.Modal(Open, "New Level", style: default(Style).W(420));
        if (modal.IsOpen)
        {
            modal.Body.Child([
                ui.Labeled("Level Name", ui.TextInput(name, null, Styles.Field)),
                ui.Labeled("Width", ui.ComboBox(width, Widths)),
                ui.Button("Create").OnClick(ev.Listener(static (ref NewLevelDialog d) => d.Create())).Disabled(!CanCreate()),
            ]);
        }
        return modal;
    }

    private readonly bool CanCreate()
    {
        if (name.Length == 0 || Tower == null)
        {
            return false;
        }
        string file = name.ToString() + ".oel";
        return !Tower.Levels.Any(x => x.FileName == file);
    }

    private void Create()
    {
        OnCreate.Invoke(new NewLevel(name.ToString(), int.Parse(Widths[width.Value])));
        Open.Value = false;
    }
}
