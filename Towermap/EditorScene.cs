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
    private Tools tools;
#endregion

    private EditorCanvas mainCanvas;
    private Transform canvasTransform;
    private GridTiles solids;
    private Spritesheet solidSpriteSheet;
    private Autotiler SolidAutotiler;
    private GridTiles bgs;
    private Spritesheet bgSpriteSheet;
    private Autotiler bgAutotiler;
    private Tiles BGTiles;
    private Tiles SolidTiles;
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
                .Add(new MenuItem("Save As", () => Save(true)))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuSlot("Settings"))
            .Add(new MenuSlot("View"));

        tools = new Tools();
        
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

        tools.AddTool("Pen [1]", () => {});
        tools.AddTool("Rect [2]", () => {});
        tools.AddTool("HSym [3]", OnHorizontalSymmetry);
        tools.AddTool("VSym [4]", OnVerticalSymmetry);
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
        solids = new GridTiles(Resource.TowerFallTexture, solidSpriteSheet);
        Add(solids);
        SolidAutotiler = new Autotiler();
        SolidAutotiler.Init("../Assets/tilesetData.xml", 6);

        bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas["tilesets/flightBG"], 10, 10);
        bgs = new GridTiles(Resource.TowerFallTexture, bgSpriteSheet);
        bgs.Depth = 1;
        Add(bgs);
        bgAutotiler = new Autotiler();
        bgAutotiler.Init("../Assets/tilesetData.xml", 7);

        SolidTiles = new Tiles(Resource.TowerFallTexture, solidSpriteSheet);
        Add(SolidTiles);
        BGTiles = new Tiles(Resource.TowerFallTexture, bgSpriteSheet);
        Add(BGTiles);
    }

#region Events
    private void OnVerticalSymmetry() 
    {
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = currentLayerSelected switch 
        {
            Layers.Solids => (solids, SolidAutotiler, null),
            Layers.BG => (bgs, bgAutotiler, solids.Bits),
            _ => (null, null, null) 
        };

        if (currentTiles == null)
            return;

        for (int y = 0; y < WorldUtils.WorldHeight / 10 * 0.5f; y++) 
        {
            for (int x = 0; x < WorldUtils.WorldWidth / 10; x++)  
            {
                int bitY = ((int)(WorldUtils.WorldHeight / 10) - y) - 1;
                if (currentTiles.SetGrid(x, bitY, currentTiles.Bits[x, y]))
                {
                    currentTiles.UpdateTile(x, bitY, autotiler.Tile(currentTiles.Bits, x, bitY));
                    UpdateTiles(x, bitY, solids.Bits);
                }
            }
        }
    }

    private void OnHorizontalSymmetry() 
    {
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = currentLayerSelected switch 
        {
            Layers.Solids => (solids, SolidAutotiler, null),
            Layers.BG => (bgs, bgAutotiler, solids.Bits),
            _ => (null, null, null) 
        };

        if (currentTiles == null)
            return;

        for (int y = 0; y < WorldUtils.WorldHeight / 10; y++) 
        {
            for (int x = 0; x < WorldUtils.WorldWidth / 10 * 0.5f; x++)  
            {
                int bitX = ((int)(WorldUtils.WorldWidth / 10) - x) - 1;
                if (currentTiles.SetGrid(bitX, y, currentTiles.Bits[x, y]))
                {
                    currentTiles.UpdateTile(bitX, y, autotiler.Tile(currentTiles.Bits, bitX, y));
                    UpdateTiles(bitX, y, solids.Bits);
                }
            }
        }
    }

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
        SolidTiles.Clear();
        BGTiles.Clear();
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
            var solid = loadingLevel["Solids"];
            solids.SetGrid(solid.InnerText);
            solids.UpdateTiles(SolidAutotiler);

            var bg = loadingLevel["BG"];
            bgs.SetGrid(bg.InnerText);
            bgs.UpdateTiles(bgAutotiler, solids.Bits);

            var solidTiles = loadingLevel["SolidTiles"];
            SolidTiles.SetTiles(solidTiles.InnerText);

            var bgTiles = loadingLevel["BGTiles"];
            BGTiles.SetTiles(bgTiles.InnerText);

            level = loadingLevel;
            currentPath = path;
        }
        catch 
        {
            Logger.LogInfo($"Failed to load this level: '{Path.GetFileName(path)}'");
            if (level != null) 
            {
                var solid = level["Solids"];
                solids.SetGrid(solid.InnerText);
                solids.UpdateTiles(SolidAutotiler);

                var bg = level["BG"];
                bgs.SetGrid(bg.InnerText);
                bgs.UpdateTiles(bgAutotiler);

                var solidTiles = loadingLevel["SolidTiles"];
                SolidTiles.SetTiles(solidTiles.InnerText);

                var bgTiles = loadingLevel["BGTiles"];
                BGTiles.SetTiles(bgTiles.InnerText);
            }
        }
    }

    private void UpdateTiles(int gridX, int gridY, Array2D<bool> also) 
    {
        solids.UpdateTile(gridX - 1, gridY, SolidAutotiler.Tile(solids.Bits, gridX - 1, gridY));
        solids.UpdateTile(gridX + 1, gridY, SolidAutotiler.Tile(solids.Bits, gridX + 1, gridY));

        solids.UpdateTile(gridX, gridY - 1, SolidAutotiler.Tile(solids.Bits, gridX, gridY - 1));
        solids.UpdateTile(gridX, gridY + 1, SolidAutotiler.Tile(solids.Bits, gridX, gridY + 1));

        solids.UpdateTile(gridX - 1, gridY - 1, SolidAutotiler.Tile(solids.Bits, gridX - 1, gridY - 1));
        solids.UpdateTile(gridX + 1, gridY + 1, SolidAutotiler.Tile(solids.Bits, gridX + 1, gridY + 1));

        solids.UpdateTile(gridX - 1, gridY + 1, SolidAutotiler.Tile(solids.Bits, gridX - 1, gridY + 1));
        solids.UpdateTile(gridX + 1, gridY - 1, SolidAutotiler.Tile(solids.Bits, gridX + 1, gridY - 1));

        bgs.UpdateTile(gridX - 1, gridY, bgAutotiler.Tile(bgs.Bits, gridX - 1, gridY, also));
        bgs.UpdateTile(gridX + 1, gridY, bgAutotiler.Tile(bgs.Bits, gridX + 1, gridY, also));

        bgs.UpdateTile(gridX, gridY - 1, bgAutotiler.Tile(bgs.Bits, gridX, gridY - 1, also));
        bgs.UpdateTile(gridX, gridY + 1, bgAutotiler.Tile(bgs.Bits, gridX, gridY + 1, also));

        bgs.UpdateTile(gridX - 1, gridY - 1, bgAutotiler.Tile(bgs.Bits, gridX - 1, gridY - 1, also));
        bgs.UpdateTile(gridX + 1, gridY + 1, bgAutotiler.Tile(bgs.Bits, gridX + 1, gridY + 1, also));

        bgs.UpdateTile(gridX - 1, gridY + 1, bgAutotiler.Tile(bgs.Bits, gridX - 1, gridY + 1, also));
        bgs.UpdateTile(gridX + 1, gridY - 1, bgAutotiler.Tile(bgs.Bits, gridX + 1, gridY - 1, also));
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
            (GridTiles currentTile, Autotiler autotiler, Array2D<bool> also) = currentLayerSelected switch 
            {
                Layers.Solids => (solids, SolidAutotiler, null),
                _ => (bgs, bgAutotiler, solids.Bits)
            };
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                if (currentTile.SetGrid(gridX, gridY, true)) 
                {
                    currentTile.UpdateTile(gridX, gridY, SolidAutotiler.Tile(currentTile.Bits, gridX, gridY, also));
                    UpdateTiles(gridX, gridY, solids.Bits);
                }
            }
        }
        else if (Input.InputSystem.Mouse.RightButton.IsDown && currentLayerSelected is Layers.Solids or Layers.BG) 
        {
            (GridTiles currentTile, Autotiler autotiler, Array2D<bool> also) = currentLayerSelected switch 
            {
                Layers.Solids => (solids, SolidAutotiler, null),
                _ => (bgs, bgAutotiler, solids.Bits)
            };
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                if (currentTile.SetGrid(gridX, gridY, false)) 
                {
                    currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY));
                    UpdateTiles(gridX, gridY, solids.Bits);
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
        tools.UpdateGui();
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
        string path;
        if (specifyPath) 
        {
            NFDResult result = FileDialog.Save(null, "xml");
            if (result.IsOk) 
            {
                path = result.Path;
                return;
            }
        }
        else 
        {
            path = currentPath;
        }

        document.Save(currentPath + ".test");
    }

    public override void End()
    {
        emptyText.Dispose();
    }
}