using Cosmiria;

namespace Towermap;

public struct EditorView : IComponent
{
    private static readonly Style Root = default(Style).FlexCol().SizeFull().Bg(Styles.Black);
    private static readonly Style Viewport = default(Style).W(Length.Full).H(0).Grow().Bg(Styles.Black);

    public EditorScene Editor;
    private DockLayout layout;

    public Node Render(UIStack ui, EventStack ev)
    {
        if (layout == null)
        {
            layout = new DockLayout(DockLayout.Row(0.14f,
                DockLayout.Tabs("levels"),
                DockLayout.Row(0.74f, DockLayout.Tabs("viewport"), DockLayout.Tabs("layers", "history"))));
            layout.Close("history");
        }

        var dock = ui.DockSpace(layout, style: default(Style).Grow());
        if (dock.Panel("levels", "Levels", out Node levels))
        {
            levels.Child(Levels(ui, ev));
        }
        if (dock.Panel("viewport", Editor.CurrentLevel?.FileName ?? "No Level", out Node viewport))
        {
            viewport.Child(ViewportPanel(ui, ev));
        }
        if (dock.Panel("layers", "Layers", out Node layers))
        {
            layers.Child(LayersPanel(ui, ev));
        }
        if (dock.Panel("history", "History", out Node history, closable: true))
        {
            history.Child(History(ui, ev));
        }

        return ui.View(Root).Child([
            Menus(ui, ev),
            dock,
            Dialogs(ui, ev),
        ]);
    }

    private readonly Node Menus(UIStack ui, EventStack ev)
    {
        var bar = ui.MenuBar(ui.Own(-1));

        var file = bar.Menu("File");
        file.Item("New Tower", ev.Listener(static (ref EditorView v) => v.Editor.NewTowerOpen.Value = true), "Ctrl+N");
        file.Item("Open", ev.Listener(static (ref EditorView v) => v.Editor.Open()), "Ctrl+O");
        if (file.IsOpen)
        {
            var recent = file.Submenu("Open Recent");
            if (recent.IsOpen)
            {
                foreach (var path in Editor.SaveState.RecentTowers)
                {
                    recent.Item(path, ev.Listener(static (ref EditorView v, string p) => v.Editor.OpenTowerFile(p), path));
                }
            }
        }
        file.Item("Save", ev.Listener(static (ref EditorView v) => v.Editor.Save()), "Ctrl+S");
        file.Item("Save As", ev.Listener(static (ref EditorView v) => v.Editor.Save(true)), "Ctrl+Shift+S");
        file.Item("Export As .tower", ev.Listener(static (ref EditorView v) => v.Editor.ShowExport()));
        file.Separator();
        file.Item("Quit", ev.Listener(static (ref EditorView v) => v.Editor.Game.Quit()));

        var edit = bar.Menu("Edit");
        edit.Item("Undo", ev.Listener(static (ref EditorView v) => v.Editor.Undo(false)), "Ctrl+Z");
        edit.Item("Redo", ev.Listener(static (ref EditorView v) => v.Editor.Undo(true)), "Ctrl+Shift+Z");

        var settings = bar.Menu("Settings");
        settings.Item("Tower Settings", ev.Listener(static (ref EditorView v) => v.Editor.SettingsOpen.Value = true));

        var view = bar.Menu("View");
        view.Item(Editor.ShowGrid ? "Hide Grid" : "Show Grid", ev.Listener(static (ref EditorView v) => v.Editor.ShowGrid = !v.Editor.ShowGrid));
        view.Item(layout.IsClosed("history") ? "Show History" : "Hide History", ev.Listener(static (ref EditorView v) => v.ToggleHistory()), "Ctrl+H");

        return WindowControls.Attach(bar, ui, Editor.Game);
    }

    private readonly Node Levels(UIStack ui, EventStack ev)
    {
        ref var list = ref ui.State<LevelList>(0, out _);
        list.Tower = Editor.Tower;
        list.Current = Editor.CurrentLevel;
        list.OnSelect = ev.Callback(static (ref EditorView v, Level level) => v.Editor.SetLevel(level));
        list.OnAdd = ev.Callback(static (ref EditorView v) => v.Editor.NewLevelOpen.Value = true);
        return ui.Mount<LevelList>(0, keepAlive: true);
    }

    private readonly Node ViewportPanel(UIStack ui, EventStack ev)
    {
        ref var toolbar = ref ui.State<Toolbar>(0, out _);
        toolbar.Selected = Editor.ToolSelected;
        toolbar.Symmetry = Editor.ToolModifier == ToolModifierFlags.Symmetry;
        toolbar.OnAction = ev.Callback(static (ref EditorView v, ToolAction action) => v.Editor.RunTool(action));

        return ui.View(Styles.Column.Grow().MinH(0)).Child([
            ui.Mount<Toolbar>(0, keepAlive: true),
            ui.Image(Editor.Canvas(), ImageFit.Contain, Viewport)
                .Sampling(ImageSampling.Nearest)
                .Ref(Editor.ViewportRef)
                .OnHover(ev.Listener(static (ref EditorView v, in PointerEvent e) => v.Editor.ViewportHovered = e.Hovered))
                .OnDrag(ev.Listener(static (ref EditorView v, in PointerEvent e) => { })),
        ]);
    }

    private readonly Node LayersPanel(UIStack ui, EventStack ev)
    {
        ref var layers = ref ui.State<LayerList>(0, out _);
        layers.Current = Editor.CurrentLayer;
        layers.Visibility = Editor.Visibility;
        layers.OnSelect = ev.Callback(static (ref EditorView v, Layers layer) => v.Editor.OnLayerSelected(layer));
        layers.OnToggleVisibility = ev.Callback(static (ref EditorView v, Layers layer) => v.Editor.ToggleVisibility(layer));

        Node panel = ui.View(Styles.Column).Child(ui.Mount<LayerList>(0, keepAlive: true));
        switch (Editor.CurrentLayer)
        {
        case Layers.Entities:
            panel.Child([EntityList(ui, ev), Inspector(ui, ev), ui.Text("Press E to open the entity menu.").Styled(Styles.Muted)]);
            break;
        case Layers.SolidTiles:
            panel.Child([ui.Separator("Solid Tiles"), Palette(ui, ev, 0, Editor.SolidBrush)]);
            break;
        case Layers.BGTiles:
            panel.Child([ui.Separator("BG Tiles"), Palette(ui, ev, 1, Editor.BgBrush)]);
            break;
        }
        return panel;
    }

    private readonly Node EntityList(UIStack ui, EventStack ev)
    {
        ref var list = ref ui.State(0, static () => new EntityList(), out _);
        list.Selected = Editor.ActorSelected;
        list.OnSelect = ev.Callback(static (ref EditorView v, Actor actor) => v.Editor.OnSelectActor(actor));
        return ui.Mount<EntityList>(0, keepAlive: true);
    }

    private readonly Node Inspector(UIStack ui, EventStack ev)
    {
        if (Editor.Conflicts?.Count > 1)
        {
            ref var conflicts = ref ui.State<ConflictList>(0, out _);
            conflicts.Actors = Editor.Conflicts;
            conflicts.OnPick = ev.Callback(static (ref EditorView v, LevelActor actor) => v.Editor.Select(actor));
            return ui.Mount<ConflictList>(0);
        }

        var actor = Editor.SelectedActor;
        if (actor == null || actor.Scene == null)
        {
            return Node.None;
        }

        int key = actor.GetHashCode();
        ref var inspector = ref ui.State<EntityInspector>(key, out _);
        if (inspector.Actor != actor)
        {
            inspector = EntityInspector.For(actor);
        }
        inspector.OnChange = ev.Callback(static (ref EditorView v, ActorChange change) => v.Editor.RecordChange(change));
        return ui.Mount<EntityInspector>(key);
    }

    private readonly Node Palette(UIStack ui, EventStack ev, int key, TileBrush brush)
    {
        ref var palette = ref ui.State<TilePalette>(key, out _);
        palette.Texture = brush.Texture;
        palette.Selection = brush.Selection;
        palette.OnSelect = ev.Callback(static (ref EditorView v, Riateu.Graphics.Rectangle rect) => v.Editor.SelectBrush(rect));
        return ui.Mount<TilePalette>(key);
    }

    private readonly Node History(UIStack ui, EventStack ev)
    {
        ref var history = ref ui.State<HistoryPanel>(0, out _);
        history.History = Editor.CurrentLevel?.History;
        history.LevelName = Editor.CurrentLevel?.FileName;
        history.OnJump = ev.Callback(static (ref EditorView v, int position) => v.Editor.JumpTo(position));
        return ui.Mount<HistoryPanel>(0, keepAlive: true);
    }

    private readonly Node Dialogs(UIStack ui, EventStack ev)
    {
        ref var newLevel = ref ui.State(0, static () => new NewLevelDialog(), out _);
        newLevel.Open = Editor.NewLevelOpen;
        newLevel.Tower = Editor.Tower;
        newLevel.OnCreate = ev.Callback(static (ref EditorView v, NewLevel level) => v.Editor.CreateLevel(level));

        ref var newTower = ref ui.State<CreateNewTower>(0, out _);
        if (newTower.Open == null)
        {
            newTower = CreateNewTower.Create(Editor.Game, Editor.SaveState, Editor.NewTowerOpen);
        }
        newTower.OnCreateTower = ev.Callback(static (ref EditorView v, CreateNewTowerCallback tower) => v.Editor.OnCreateTower(tower));

        ref var export = ref ui.State(0, static () => new ExportDialog(), out _);
        export.Open = Editor.ExportOpen;
        export.Theme = Editor.Tower?.Theme;
        export.OnExport = ev.Callback(static (ref EditorView v, ExportOverride options) => v.Editor.OnExport(options));

        ref var settings = ref ui.State(0, static () => new TowerSettingsDialog(), out _);
        settings.Open = Editor.SettingsOpen;
        settings.Tower = Editor.Tower;
        settings.Quest = Editor.Quest;
        settings.OnSave = ev.Callback(static (ref EditorView v, TowerData data) => v.Editor.SaveTowerSettings(data));
        settings.OnSaveQuest = ev.Callback(static (ref EditorView v) => v.Editor.SaveQuest());

        ref var entityMenu = ref ui.State(0, static () => new EntityMenu(), out _);
        entityMenu.Open = Editor.EntityMenuOpen;
        entityMenu.OnSelect = ev.Callback(static (ref EditorView v, Actor actor) => v.Editor.OnSelectActor(actor));

        ref var fallback = ref ui.State(0, static () => new FallbackThemeDialog(), out _);
        fallback.Open = Editor.FallbackOpen;
        fallback.OnApply = ev.Callback(static (ref EditorView v, int theme) => v.Editor.ApplyFallbackTheme(theme));

        return ui.Fragment([
            ui.Mount<NewLevelDialog>(0, keepAlive: true),
            ui.Mount<CreateNewTower>(0, keepAlive: true),
            ui.Mount<ExportDialog>(0, keepAlive: true),
            ui.Mount<TowerSettingsDialog>(0, keepAlive: true),
            ui.Mount<EntityMenu>(0, keepAlive: true),
            ui.Mount<FallbackThemeDialog>(0, keepAlive: true),
        ]);
    }

    private readonly void ToggleHistory()
    {
        if (!layout.IsClosed("history"))
        {
            layout.Close("history");
            return;
        }
        layout.Open("history");
        layout.Float("history", 12, Editor.Game.MainWindow.Height - 372, 280, 360);
    }
}
