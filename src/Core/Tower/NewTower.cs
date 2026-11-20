using System;
using Cosmiria;
using Riateu.IO;

namespace Towermap;

public struct CreateNewTowerCallback
{
    public string TowerName;
    public string TowerPath;
    public int TowerMode;
    public int TowerTheme;
}

public struct CreateNewTower() : IComponent
{
    public BindableValue<bool> Open = new(false);
    private readonly TextBuffer towerName = new();
    private readonly TextBuffer towerPath = new();
    private readonly BindableValue<int> towerMode = new(0);
    private readonly BindableValue<int> theme = new(0);
    private string[] modes;
    private TowermapGame game;

    public Callback<CreateNewTowerCallback> OnCreateTower;

    public static CreateNewTower Create(
        TowermapGame game, 
        SaveState saveState, 
        BindableValue<bool> open
    )
    {
        return new CreateNewTower { 
            game = game,
            modes = saveState.DarkWorld ? ["Versus", "Quest", "DarkWorld"] : ["Versus", "Quest"],
            Open = open
        };
    }

    public Node Render(UIStack ui, EventStack ev)
    {
        var modal = ui.Modal(Open, "New Tower", style: default(Style).W(520));
        if (!modal.IsOpen)
        {
            return modal;
        }

        bool canCreate = towerName.Length > 0 && towerPath.Length > 0;
        modal.Body.Child([
            ui.Labeled("Tower Name", ui.TextInput(towerName, null, Styles.Field)),
            ui.Labeled("Tower Path", ui.View(Styles.Row.Grow()).Child([
                ui.TextInput(towerPath, null, Styles.Field),
                ui.Button("Browse").OnClick(ev.Listener(static (ref CreateNewTower t) => t.Browse()))
            ])),
            ui.Labeled("Tower Mode", ui.ComboBox(towerMode, modes)),
            ui.Labeled("Theme", ui.ComboBox(theme, Themes.ThemeNames)),
            ui.Button("Create").OnClick(ev.Listener(static (ref CreateNewTower t) => t.Create())).Disabled(!canCreate),
        ]);
        return modal;
    }

    private void Browse()
    {
        var s = this;
        FileDialog.OpenFolder(path => s.game.UI.Post(() => { s.towerPath.Set(path); s.game.UI.Notify(); }), null);
    }

    private void Create()
    {
        OnCreateTower.Invoke(new CreateNewTowerCallback
        {
            TowerName = towerName.ToString(),
            TowerPath = towerPath.ToString(),
            TowerMode = towerMode.Value,
            TowerTheme = theme.Value
        });
        Open.Value = false;
    }
}
