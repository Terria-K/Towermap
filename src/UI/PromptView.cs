using System.IO;
using Cosmiria;
using Riateu.IO;

namespace Towermap;

public struct PromptView() : IComponent
{
    public TowermapGame Game;

    private readonly TextBuffer path = new();
    private bool validPath;
    private bool darkWorldFound;
    private bool fortRiseFound;

    public static PromptView Create(TowermapGame game, SaveState saveState)
    {
        var view = new PromptView { Game = game };
        view.path.Set(saveState.TFPath ?? "");
        return view;
    }

    public Node Render(UIStack ui, EventStack ev)
    {
        ui.Effect(ev.Listener(static (ref PromptView p) => p.Validate()), path.Version);

        var bar = ui.MenuBar(ui.Own(-1));
        bar.Menu("File").Item("Quit", ev.Listener(static (ref PromptView p) => p.Game.Quit()));
        WindowControls.Attach(bar, ui, Game);

        return ui.View(default(Style).FlexCol().SizeFull().Bg(Styles.Black)).Child([
            bar,
            ui.View(default(Style).FlexCol().Grow().ItemsCenter().JustifyCenter().Gap(12)).Child([
                ui.Text("Towermap needs content from TowerFall. Enter the TowerFall path below."),
                ui.View(Styles.Row).Child([
                    ui.TextInput(path, "TowerFall folder", default(Style).W(400)),
                    ui.Button("Browse").OnClick(ev.Listener(static (ref PromptView p) => p.Browse())),
                ]),
                ui.Button("Proceed").OnClick(ev.Listener(static (ref PromptView p) => p.Game.SetTFPath(p.path.ToString())))
                    .Disabled(!validPath),
                ui.View(Styles.Column.W(400)).Child([
                    ui.Separator("Features Available"),
                    ui.Checkbox(darkWorldFound, "Dark World").Disabled(),
                    ui.Checkbox(fortRiseFound, "FortRise").Disabled(),
                ]),
            ]),
        ]);
    }

    private void Validate()
    {
        string dir = path.ToString();
        validPath = dir.Length > 0 && File.Exists(Path.Combine(dir, "TowerFall.exe"));
        darkWorldFound = validPath && Directory.Exists(Path.Combine(dir, "DarkWorldContent"));
        fortRiseFound = validPath && File.Exists(Path.Combine(dir, "PatchVersion.txt"));
    }

    private readonly void Browse()
    {
        var buffer = path;
        var ui = Game.UI;
        FileDialog.OpenFolder(folder => ui.Post(() => { buffer.Set(folder); ui.Notify(); }));
    }
}
