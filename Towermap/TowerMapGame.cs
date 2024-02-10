using MoonWorks;
using Riateu;

namespace Towermap;

public class TowermapGame : GameApp
{
    public TowermapGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void Initialize()
    {
        Scene = new EditorScene(this);
    }
}