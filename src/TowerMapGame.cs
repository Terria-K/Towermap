using System;
using System.IO;
using System.Runtime.InteropServices;
using ImGuiNET;
using Riateu;
using Riateu.Content;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public class TowermapGame : GameApp
{
    private Scene Scene;
    private SaveState saveState;
    public TowermapGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings) {}

    public override void LoadContent(AssetStorage storage)
    {
        var savePath = Path.Join(SaveIO.SavePath.AsSpan(), "towersave.json");
        if (File.Exists(savePath))
        {
            saveState = SaveIO.LoadJson<SaveState>("towersave.json", SaveStateContext.Default.SaveState);
        }
        else 
        {
            saveState = new SaveState();
        }

        if (string.IsNullOrEmpty(saveState.TFPath))
        {
            return;
        }

        string bgAtlasXml = Path.Combine(saveState.TFPath, "Content", "Atlas", "bgAtlas.xml");
        string bgAtlasPath = Path.Combine(saveState.TFPath, "Content", "Atlas", "bgAtlas.png");
        string atlasXml;
        string atlasPath;
        if (saveState.DarkWorld)
        {
            atlasPath = Path.Combine(saveState.TFPath, "DarkWorldContent", "Atlas", "atlas.png");
            atlasXml = Path.Combine(saveState.TFPath, "DarkWorldContent", "Atlas", "atlas.xml");
        }
        else 
        {
            atlasXml = Path.Combine(saveState.TFPath, "Content", "Atlas", "atlas.xml");
            atlasPath = Path.Combine(saveState.TFPath, "Content", "Atlas", "atlas.png");
        }

        Resource.TowerFallTexture = storage.LoadTexture(atlasPath);
        Resource.BGAtlasTexture = storage.LoadTexture(bgAtlasPath);
        Resource.Atlas = TowerFallAtlas.LoadAtlas(Resource.TowerFallTexture, atlasXml);
        Resource.BGAtlas = TowerFallAtlas.LoadAtlas(Resource.BGAtlasTexture, bgAtlasXml);
        var particle = Resource.Atlas["particle"];
        Resource.Pixel = new TextureQuad(Resource.TowerFallTexture, new Rectangle(particle.Source.X, particle.Source.Y, 1, 1));
    }

    public override void Initialize()
    {
        ImGuiRenderer renderer = new ImGuiRenderer(GraphicsDevice, MainWindow, 1280, 640, ImGuiInit);

        if (string.IsNullOrEmpty(saveState.TFPath))
        {
            Scene = new PromptScene(this, renderer, saveState);
            Scene.Begin();
            return;
        }

        InitEditor(renderer, saveState);
    }

    public void InitEditor(ImGuiRenderer renderer, SaveState saveState)
    {
        Themes.InitThemes(saveState);
        ChangeScene(new EditorScene(this, renderer, saveState));
    }

    private unsafe void ImGuiInit(ImGuiIOPtr io) 
    {
        ImFontConfig* config = ImGuiNative.ImFontConfig_ImFontConfig();
        config->MergeMode = 1;
        config->PixelSnapH = 1;
        config->FontDataOwnedByAtlas = 0;

        config->GlyphMaxAdvanceX = float.MaxValue;
        config->RasterizerMultiply = 1.0f;
        config->OversampleH = 2;
        config->OversampleV = 1;

        ushort* ranges = stackalloc ushort[3];
        ranges[0] = FA6.IconMin;
        ranges[1] = FA6.IconMax;
        ranges[2] = 0;


        byte *iconFontRange = (byte*)NativeMemory.Alloc(6);
        NativeMemory.Copy(ranges, iconFontRange, 6);
        config->GlyphRanges = (ushort*)iconFontRange;
        FA6.IconFontRanges = (IntPtr)iconFontRange;

        byte[] fontDataBuffer = Convert.FromBase64String(FA6.IconFontData);

        fixed (byte *buffer = &fontDataBuffer[0])
        {
            var fontPtr = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(new IntPtr(buffer), fontDataBuffer.Length, 11, config, FA6.IconFontRanges);
        }

        ImGuiNative.ImFontConfig_destroy(config);
    }

    public override void Destroy()
    {
        base.Destroy();
        Scene.End();
        Resource.TowerFallTexture.Dispose();
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

    public void ChangeScene(Scene scene)
    {
        Scene?.End();
        scene.Begin();
        Scene = scene;
    }

    [STAThread]
    private static void Main()
    {
        var game = new TowermapGame(new Riateu.WindowSettings("Towermap", 1280, 640, WindowMode.Windowed, new Flags 
        {
            Borderless = true
        }, OnHitTest), Riateu.GraphicsSettings.Debug);
        game.Run();
    }


    private static HitTestResult OnHitTest(Window window, Point area) 
    {
        if (area.Y < 20)
        {
            return HitTestResult.Draggable;
        }

        return HitTestResult.Normal;
    }
}