using Cosmiria;

namespace Towermap;

public struct FallbackThemeDialog() : IComponent
{
    public BindableValue<bool> Open;
    public Callback<int> OnApply;

    private readonly BindableValue<int> theme = new(0);

    public Node Render(UIStack ui, EventStack ev)
    {
        var modal = ui.Modal(Open, "Theme not found", dismissable: false);
        if (modal.IsOpen)
        {
            modal.Body.Child([
                ui.Text("Theme not found, fallback to another one:"),
                ui.Labeled("Fallback Theme", ui.ComboBox(theme, Themes.ThemeNames)),
                ui.Button("OK").Styled(default(Style).SelfEnd())
                    .OnClick(ev.Listener(static (ref FallbackThemeDialog d) => d.OnApply.Invoke(d.theme.Value))),
            ]);
        }
        return modal;
    }
}
