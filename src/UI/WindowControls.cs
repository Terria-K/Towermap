using Cosmiria;
using Riateu;
using SDL3;

namespace Towermap;

public struct WindowControls : IComponent
{
    public const int ButtonWidth = 46;
    public const int Width = ButtonWidth * 3;

    private static readonly Style Bar = default(Style).FlexRow().SelfStretch().Mr(-4).My(-2);
    private static readonly Style Button = default(Style).W(ButtonWidth).FlexRow().ItemsCenter().JustifyCenter()
        .TextColorOf(Tailwind.Neutral300);
    private static readonly Style ButtonHover = default(Style).Bg(Styles.Hover).TextColorOf(Tailwind.White);
    private static readonly Style Raise = default(Style).Translate(0, -4);
    private static readonly Style CloseHover = default(Style).Bg(Tailwind.Red600).TextColorOf(Tailwind.White);

    public GameApp Game;

    public static Node Attach(MenuBar bar, UIStack ui, GameApp game)
    {
        ui.State<WindowControls>(0, out _).Game = game;
        bar.Root.Child(ui.View(default(Style).Grow())).Child(ui.Mount<WindowControls>(0));
        return bar;
    }

    public Node Render(UIStack ui, EventStack ev)
    {
        return ui.View(Bar).Child([
            Control(ui, FA6.WindowMinimize, ButtonHover, ev.Listener(static (ref WindowControls w) => SDL.SDL_MinimizeWindow(w.Game.MainWindow.Handle)), Raise),
            Control(ui, IsMaximized(Game) ? FA6.WindowRestore : FA6.WindowMaximize, ButtonHover, ev.Listener(static (ref WindowControls w) => ToggleMaximize(w.Game))),
            Control(ui, FA6.Xmark, CloseHover, ev.Listener(static (ref WindowControls w) => w.Game.Quit())),
        ]);
    }

    public static void ToggleMaximize(GameApp game)
    {
        if (IsMaximized(game))
        {
            SDL.SDL_RestoreWindow(game.MainWindow.Handle);
        }
        else
        {
            SDL.SDL_MaximizeWindow(game.MainWindow.Handle);
        }
    }

    private static bool IsMaximized(GameApp game) =>
        (SDL.SDL_GetWindowFlags(game.MainWindow.Handle) & SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0;

    private static Node Control(UIStack ui, string icon, in Style hover, EventHandle onClick, in Style iconStyle = default) =>
        ui.View(Button).Hover(hover).OnClick(onClick).Child(ui.Icon(icon).Styled(iconStyle));
}
