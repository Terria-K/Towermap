using Riateu;
using Riateu.Content;
using Riateu.Graphics;

namespace Towermap;

public class TowermapGame : GameApp
{
    private EditorScene Scene;
    public TowermapGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings) {}

    public override void LoadContent(AssetStorage storage)
    {
        Resource.TowerFallTexture = storage.LoadTexture("../Assets/atlas.png");
        Resource.BGAtlasTexture = storage.LoadTexture("../Assets/bgAtlas.png");
        Resource.Font = storage.LoadFont("../Assets/font/PressStart2P-Regular.ttf", 12);
        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, "../Assets/atlas.xml");
        Resource.BGAtlas = TowerFallAtlas.LoadAtlas(Resource.BGAtlasTexture, "../Assets/bgAtlas.xml");
        var particle = Resource.Atlas["particle"];
        Resource.Pixel = new TextureQuad(Resource.TowerFallTexture, new Rectangle(particle.Source.X, particle.Source.Y, 1, 1));
    }

    public override void Initialize()
    {
        Themes.InitThemes();
        Scene = new EditorScene(this);
        Scene.Begin();
    }

    public override void Destroy()
    {
        base.Destroy();
        Scene.End();
        Resource.TowerFallTexture.Dispose();
        Resource.Font.Texture.Dispose();
    }

    public override void Update(float delta)
    {
        Scene.Update(delta);
    }

    public override void Render()
    {
        CommandBuffer commandBuffer = GraphicsDevice.AcquireCommandBuffer();
        var swapchainTarget = commandBuffer.AcquireSwapchainTarget(MainWindow);
        if (swapchainTarget != null)
        {
            Scene.Render(commandBuffer, swapchainTarget);
        }
        GraphicsDevice.Submit(commandBuffer);
    }
}