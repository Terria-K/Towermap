using System;
using System.IO;
using System.Xml;
using ImGuiNET;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public enum Layers
{
    Solids,
    BG,
    Entities,
    SolidTiles,
    BGTiles
}

public class EditorScene : Scene
{
    private ImGuiRenderer imGui;

#region Elements
    private ImGuiElement menuBar;
    private LevelSelection levelSelection;
    private LayersPanel layers;
#endregion

    private EditorCanvas mainCanvas;
    private Transform canvasTransform;
    private Tiles solids;
    private Spritesheet solidSpriteSheet;
    private Autotiler SolidAutotiler;
    private Tiles bgs;
    private Spritesheet bgSpriteSheet;
    private Autotiler bgAutotiler;
    private XmlElement level;
    private Layers currentLayerSelected = Layers.Solids;
    private StaticText emptyText;

#region Level State
    private string currentPath;
#endregion


    public EditorScene(GameApp game) : base(game)
    {
        imGui = new ImGuiRenderer(game.GraphicsDevice, game.MainWindow, 1024, 640, ImGuiInit);
        menuBar = new MenuBar()
            .Add(new MenuSlot("File")
                .Add(new MenuItem("New"))
                .Add(new MenuItem("Open", Open))
                .Add(new MenuItem("Save", () => Save()))
                .Add(new MenuItem("Save As"))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuSlot("Settings"))
            .Add(new MenuSlot("View"));
        
        levelSelection = new LevelSelection();
        levelSelection.OnSelect = OnLevelSelected;

        layers = new LayersPanel();
        layers.OnLayerSelect = OnLayerSelected;

        mainCanvas = new EditorCanvas(this, game.GraphicsDevice);

        canvasTransform = new Transform();
        canvasTransform.PosX = WorldUtils.WorldX;
        canvasTransform.PosY = WorldUtils.WorldY;
        canvasTransform.Scale = new Vector2(2);

        emptyText = new StaticText(game.GraphicsDevice, Resource.Font, "No Level Selected", 24);
    }

    private unsafe void ImGuiInit(ImGuiIOPtr io) 
    {
        const int FontAwesomeIconRangeStart = 0xe005;
        const int FontAwesomeIconRangeEnd = 0xf8ff;

        ushort[] ranges = [FontAwesomeIconRangeStart, FontAwesomeIconRangeEnd];
        fixed (ushort* rangesPtr = ranges)
        {
            ImFontConfig* iconConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            iconConfig->MergeMode = 1;
            iconConfig->PixelSnapH = 1;
            iconConfig->GlyphMinAdvanceX = 13 * 2.0f / 3.0f;
            io.Fonts.AddFontFromFileTTF("../Assets/font/fontawesome3.ttf", 13 * 2.0f / 3.0f,
                iconConfig, (IntPtr)rangesPtr);
        }
    }

    public override void Begin()
    {
        levelSelection.SelectTower("../Assets");
        solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas["tilesets/flight"], 10, 10);
        solids = new Tiles(Resource.TowerFallTexture, solidSpriteSheet);
        Add(solids);
        SolidAutotiler = new Autotiler(solidSpriteSheet);
        SolidAutotiler.Init("../Assets/tilesetData.xml", 6);

        bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas["tilesets/flightBG"], 10, 10);
        bgs = new Tiles(Resource.TowerFallTexture, bgSpriteSheet);
        bgs.Depth = 1;
        Add(bgs);
        bgAutotiler = new Autotiler(solidSpriteSheet);
        bgAutotiler.Init("../Assets/tilesetData.xml", 7);
    }

#region Events
    private void OnLayerSelected(string layer)
    {
        switch (layer) 
        {
        case "Solids":
            currentLayerSelected = Layers.Solids;
            break;
        case "BG":
            currentLayerSelected = Layers.BG;
            break;
        case "Entities":
            currentLayerSelected = Layers.Entities;
            break;
        case "SolidTiles":
            currentLayerSelected = Layers.SolidTiles;
            break;
        case "BGTiles":
            currentLayerSelected = Layers.BGTiles;
            break;
        }
    }

    private void OnLevelSelected(string path)
    {
        SetLevel(path);
    }
#endregion

    public void SetLevel(string path) 
    {
        solids.Clear();
        bgs.Clear();
        if (path == null) 
        {
            level = null;
            return;
        }
        XmlDocument document = new XmlDocument();
        document.Load(path);

        var loadingLevel = document["level"];
        try 
        {
            var solidTiles = loadingLevel["Solids"];
            solids.SetGrid(solidTiles.InnerText);
            solids.UpdateTiles(SolidAutotiler);

            var bgTiles = loadingLevel["BG"];
            bgs.SetGrid(bgTiles.InnerText);
            bgs.UpdateTiles(bgAutotiler);

            level = loadingLevel;
            currentPath = path;
        }
        catch 
        {
            Logger.LogInfo($"Failed to load this level: '{Path.GetFileName(path)}'");
            if (level != null) 
            {
                var solidTiles = level["Solids"];
                solids.SetGrid(solidTiles.InnerText);
                solids.UpdateTiles(SolidAutotiler);

                var bgTiles = level["BG"];
                bgs.SetGrid(bgTiles.InnerText);
                bgs.UpdateTiles(bgAutotiler);
            }
        }
    }

    public override void Update(double delta)
    {
        imGui.Update(GameInstance.Inputs, ImGuiCallback);
        if (level == null)
        {
            return;
        }

        if (Input.InputSystem.Keyboard.IsDown(KeyCode.LeftControl) && Input.InputSystem.Keyboard.IsPressed(KeyCode.S)) 
        {
            Save();
        }
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;

        if (Input.InputSystem.Mouse.LeftButton.IsDown && currentLayerSelected is Layers.Solids or Layers.BG) 
        {
            (Tiles currentTile, Autotiler autotiler) = currentLayerSelected switch 
            {
                Layers.Solids => (solids, SolidAutotiler),
                _ => (bgs, bgAutotiler)
            };
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                if (currentTile.SetGrid(gridX, gridY, true)) 
                {
                    currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY));

                    currentTile.UpdateTile(gridX - 1, gridY, autotiler.Tile(currentTile.Bits, gridX - 1, gridY));
                    currentTile.UpdateTile(gridX + 1, gridY, autotiler.Tile(currentTile.Bits, gridX + 1, gridY));

                    currentTile.UpdateTile(gridX, gridY - 1, autotiler.Tile(currentTile.Bits, gridX, gridY - 1));
                    currentTile.UpdateTile(gridX, gridY + 1, autotiler.Tile(currentTile.Bits, gridX, gridY + 1));

                    currentTile.UpdateTile(gridX - 1, gridY - 1, autotiler.Tile(currentTile.Bits, gridX - 1, gridY - 1));
                    currentTile.UpdateTile(gridX + 1, gridY + 1, autotiler.Tile(currentTile.Bits, gridX + 1, gridY + 1));

                    currentTile.UpdateTile(gridX - 1, gridY + 1, autotiler.Tile(currentTile.Bits, gridX - 1, gridY + 1));
                    currentTile.UpdateTile(gridX + 1, gridY - 1, autotiler.Tile(currentTile.Bits, gridX + 1, gridY - 1));
                }
            }
        }
        else if (Input.InputSystem.Mouse.RightButton.IsDown && currentLayerSelected is Layers.Solids or Layers.BG) 
        {
            (Tiles currentTile, Autotiler autotiler) = currentLayerSelected switch 
            {
                Layers.Solids => (solids, SolidAutotiler),
                _ => (bgs, bgAutotiler)
            };
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                if (currentTile.SetGrid(gridX, gridY, false)) 
                {
                    currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY));

                    currentTile.UpdateTile(gridX - 1, gridY, autotiler.Tile(currentTile.Bits, gridX - 1, gridY));
                    currentTile.UpdateTile(gridX + 1, gridY, autotiler.Tile(currentTile.Bits, gridX + 1, gridY));

                    currentTile.UpdateTile(gridX, gridY - 1, autotiler.Tile(currentTile.Bits, gridX, gridY - 1));
                    currentTile.UpdateTile(gridX, gridY + 1, autotiler.Tile(currentTile.Bits, gridX, gridY + 1));

                    currentTile.UpdateTile(gridX - 1, gridY - 1, autotiler.Tile(currentTile.Bits, gridX - 1, gridY - 1));
                    currentTile.UpdateTile(gridX + 1, gridY + 1, autotiler.Tile(currentTile.Bits, gridX + 1, gridY + 1));

                    currentTile.UpdateTile(gridX - 1, gridY + 1, autotiler.Tile(currentTile.Bits, gridX - 1, gridY + 1));
                    currentTile.UpdateTile(gridX + 1, gridY - 1, autotiler.Tile(currentTile.Bits, gridX + 1, gridY - 1));
                }
            }
        }
    }

    private static bool InBounds(int gridX, int gridY) 
    {
        return gridX > -1 && gridY > -1 && gridX < 32 && gridY < 24;
    }

    private void ImGuiCallback()
    {
        ImGui.ShowDemoWindow();

        menuBar.UpdateGui();
        levelSelection.UpdateGui();
        layers.UpdateGui();
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer, IBatch batch)
    {
        mainCanvas.Draw(buffer, batch);
        batch.Start();
        batch.Add(mainCanvas.CanvasTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Color.White, canvasTransform.WorldMatrix);
        if (level == null) 
        {
            emptyText.Draw(batch, new Vector2(WorldUtils.WorldX + 40, WorldUtils.WorldY + (210)));
        }
        batch.FlushVertex(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, Color.Black));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
        batch.Draw(buffer);

        imGui.Draw(buffer);
        buffer.EndRenderPass();
    }

    private void Open() 
    {
        NFDResult result = FileDialog.OpenFile(null, "xml");
        if (result.IsOk) 
        {
            string path = Path.GetDirectoryName(result.Path);

            levelSelection.SelectTower(path);
            SetLevel(null);
        }
    }

    public void Save(bool specifyPath = false) 
    {
        XmlDocument document = new XmlDocument();
        string solid = solids.Save();
        string bg = bgs.Save();
        var rootElement = document.CreateElement("level");
        document.AppendChild(rootElement);

        var SolidTiles = document.CreateElement("Solids");
        SolidTiles.SetAttribute("exportMode", "Bitstring");
        SolidTiles.InnerText = solid;

        var BG = document.CreateElement("BG");
        BG.SetAttribute("exportMode", "Bitstring");
        BG.InnerText = bg;

        rootElement.AppendChild(SolidTiles);
        rootElement.AppendChild(BG);
        if (specifyPath) 
        {

        }

        document.Save(currentPath + ".test");
    }

    public override void End()
    {
    }
}