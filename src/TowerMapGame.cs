using System;
using System.IO;
using Cosmiria;
using Riateu;
using Riateu.Content;
using Riateu.Graphics;
using Towermap.TowerFall;

namespace Towermap;

public class TowermapGame : GameApp
{
    public UIContext UI { get; private set; }
    public EditorScene Editor { get; private set; }

    public SaveState SaveState => saveState;

    private SaveState saveState;

    public TowermapGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings) {}

    public override void LoadContent(AssetStorage storage)
    {
        string fonts = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");
        Resource.Font ??= storage.LoadFont(Path.Combine(fonts, "Carlito-Regular.ttf"), 15, SpriteFont.DefineCharset(0x20, 0x100));
        Resource.IconFont ??= storage.LoadFont(Path.Combine(fonts, "fa-solid-900.ttf"), 15, SpriteFont.DefineCharset(0x20, 0x7f));

        var savePath = Path.Join(SaveIO.SavePath.AsSpan(), "towersave.json");
        saveState = File.Exists(savePath)
            ? SaveIO.LoadJson<SaveState>("towersave.json", SaveStateContext.Default.SaveState)
            : new SaveState();

        if (string.IsNullOrEmpty(saveState.TFPath))
        {
            return;
        }

        string content = Path.Combine(saveState.TFPath, "Content", "Atlas");
        string atlasDir = saveState.DarkWorld ? Path.Combine(saveState.TFPath, "DarkWorldContent", "Atlas") : content;

        Resource.TowerFallTexture = storage.LoadTexture(Path.Combine(atlasDir, "atlas.png"));
        Resource.BGAtlasTexture = storage.LoadTexture(Path.Combine(content, "bgAtlas.png"));
        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, Path.Combine(atlasDir, "atlas.xml"));
        Resource.BGAtlas = TowerFallAtlas.LoadAtlas(Resource.BGAtlasTexture, Path.Combine(content, "bgAtlas.xml"));
        AtlasImages.Load(Path.Combine(atlasDir, "atlas.png"));

        var particle = Resource.Atlas["particle"];
        Resource.Pixel = new TextureQuad(Resource.TowerFallTexture, new Rectangle(particle.Source.X, particle.Source.Y, 1, 1));
        Resource.TilesetData = TilesetData.Load(Path.Combine(content, "GameData", "tilesetData.xml"));
    }

    public override void Initialize()
    {
        UI = new UIContext(GraphicsDevice, MainWindow, Resource.Font);
        UI.Fonts.Add(Resource.IconFont);
        Styles.Apply(UI.Theme);
        UI.SetRoot(new App { Game = this });

        if (!string.IsNullOrEmpty(saveState.TFPath))
        {
            InitEditor();
        }
    }

    public void InitEditor()
    {
        Themes.InitThemes(saveState);
        Editor = new EditorScene(this, saveState);
        Editor.Begin();
        UI.Notify();
    }

    public void SetTFPath(string path)
    {
        saveState.TFPath = path;
        SaveIO.SaveJson<SaveState>("towersave.json", saveState, SaveStateContext.Default.SaveState);
        ReloadContent();
        InitEditor();
    }

    public override void Destroy()
    {
        base.Destroy();
        Editor?.End();
        UI.Dispose();
        Resource.TowerFallTexture?.Dispose();
    }

    public override void Update(float delta)
    {
        UI.Update();
        Editor?.Update(delta);
    }

    public override void Render()
    {
        CommandBuffer commandBuffer = GraphicsDevice.AcquireCommandBuffer();
        var swapchainTarget = commandBuffer.AcquireSwapchainTarget(MainWindow);
        if (swapchainTarget != null)
        {
            Editor?.Render(commandBuffer, swapchainTarget);
            UI.Render(commandBuffer, swapchainTarget, LoadOp.Clear, Color.Black);
        }
        GraphicsDevice.Submit(commandBuffer);
    }

    [STAThread]
    private static void Main()
    {
        var game = new TowermapGame(new WindowSettings("Towermap", 1280, 640, WindowMode.Windowed,
            new Flags(Resizable: true, Borderless: true), OnHitTest), GraphicsSettings.Fast);
        game.Run();
    }

    private static HitTestResult OnHitTest(Window window, Point area)
    {
        const int edge = 5;
        bool left = area.X < edge;
        bool right = area.X >= window.Width - edge;
        bool top = area.Y < edge;
        bool bottom = area.Y >= window.Height - edge;

        return (left, right, top, bottom) switch
        {
            (true, _, true, _) => HitTestResult.TopLeft,
            (_, true, true, _) => HitTestResult.TopRight,
            (true, _, _, true) => HitTestResult.BottomLeft,
            (_, true, _, true) => HitTestResult.BottomRight,
            (true, _, _, _) => HitTestResult.Left,
            (_, true, _, _) => HitTestResult.Right,
            (_, _, true, _) => HitTestResult.Top,
            (_, _, _, true) => HitTestResult.Bottom,
            _ when area.Y < Styles.MenuBarHeight && area.X > Styles.MenuBarWidth && area.X < window.Width - WindowControls.Width => HitTestResult.Draggable,
            _ => HitTestResult.Normal
        };
    }
}
