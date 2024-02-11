using MoonWorks;
using MoonWorks.Graphics;
using Riateu;

namespace Towermap;

public static class Resource 
{
    public static Texture TowerFallTexture;
    public static TowerFallAtlas Atlas;
}

public class TowermapGame : GameApp
{
    public TowermapGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void Initialize()
    {
        Scene = new EditorScene(this);
    }

    public override void LoadContent()
    {
        CommandBuffer buffer = GraphicsDevice.AcquireCommandBuffer();
        Resource.TowerFallTexture = Texture.FromImageFile(GraphicsDevice, buffer, "../Assets/atlas.png");
        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, "../Assets/atlas.xml");
        GraphicsDevice.Submit(buffer);
    }
}