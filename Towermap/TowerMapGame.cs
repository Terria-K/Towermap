using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public static class Resource 
{
    public static Texture TowerFallTexture;
    public static TowerFallAtlas Atlas;
    public static Quad Pixel;
    public static Font Font;
}

public class TowermapGame : GameApp
{
    public TowermapGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode) {}

    public override void Initialize()
    {
        Scene = new EditorScene(this);
    }

    public override void LoadContent()
    {
        var uploader = new ResourceUploader(GraphicsDevice);
        Resource.TowerFallTexture = uploader.CreateTexture2DFromCompressed("../Assets/atlas.png");
        uploader.Upload();
        uploader.Dispose();

        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, "../Assets/atlas.xml");

        CommandBuffer buffer = GraphicsDevice.AcquireCommandBuffer();
        Resource.Font = Font.Load(GraphicsDevice, buffer, "../Assets/font/PressStart2P-Regular.ttf");
        GraphicsDevice.Submit(buffer);

        var particle = Resource.Atlas["particle"];
        Resource.Pixel = new Quad(Resource.TowerFallTexture, new Rect(particle.Source.X, particle.Source.Y, 1, 1));
    }
}