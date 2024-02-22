using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using Riateu;

namespace Towermap;

public static class Resource 
{
    public static Texture TowerFallTexture;
    public static TowerFallAtlas Atlas;
    public static Font Font;
}

public class TowermapGame : GameApp
{
    public TowermapGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void Initialize()
    {
        SDL2.SDL.SDL_CaptureMouse(SDL2.SDL.SDL_bool.SDL_FALSE);
        Scene = new EditorScene(this);
    }

    public override void LoadContent()
    {
        CommandBuffer buffer = GraphicsDevice.AcquireCommandBuffer();
        Resource.TowerFallTexture = Texture.FromImageFile(GraphicsDevice, buffer, "../Assets/atlas.png");
        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, "../Assets/atlas.xml");
        Resource.Font = Font.Load(GraphicsDevice, buffer, "../Assets/font/PressStart2P-Regular.ttf");
        GraphicsDevice.Submit(buffer);
    }
}