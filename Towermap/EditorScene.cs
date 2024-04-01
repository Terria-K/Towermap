using System;
using System.Collections.Generic;
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

public enum Tool
{
    Pen,
    Rect
}

public class EditorScene : Scene
{
    private ImGuiRenderer imGui;
    private ActorManager actorManager;

#region Elements
    private ImGuiElement menuBar;
    private LevelSelection levelSelection;
    private LayersPanel layers;
    private TilePanel solidTilesPanel;
    private TilePanel bgTilesPanel;
    private Tools tools;
    private Entities entities;
    private EntityData entityData;
#endregion

    private EditorCanvas mainCanvas;
    private GridTiles solids;
    private Spritesheet solidSpriteSheet;
    private Autotiler SolidAutotiler;
    private GridTiles bgs;
    private Spritesheet bgSpriteSheet;
    private Autotiler bgAutotiler;
    private Tiles BGTiles;
    private Tiles SolidTiles;
    private Actor actorSelected;
    private PhantomActor phantomActor;
    private XmlElement level;
    private StaticText emptyText;
    private History history;
    private bool isDrawing;
    private IntPtr imGuiTexture;
    private Batch batch;

#region Level State
    private string currentPath;
    private LevelActor currentSelected;
    private List<LevelActor> listSelected = [];
    private bool hasSelection;

    public Tool ToolSelected;
    public Layers CurrentLayer = Layers.Solids;
    public bool HasRemovedEntity;
#endregion


    public EditorScene(GameApp game) : base(game)
    {
        batch = new Batch(game.GraphicsDevice, 1024, 640);
        actorManager = new ActorManager();
        VanillaActor.Init(actorManager);
        history = new History();
        imGui = new ImGuiRenderer(game.GraphicsDevice, game.MainWindow, 1024, 640, ImGuiInit);
        imGuiTexture = imGui.BindTexture(Resource.TowerFallTexture);
        menuBar = new MenuBar()
            .Add(new MenuSlot("File")
                .Add(new MenuItem("New"))
                .Add(new MenuItem("Open", Open))
                .Add(new MenuItem("Save", () => Save()))
                .Add(new MenuItem("Save As", () => Save(true)))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuSlot("Settings")
                .Add(new MenuItem("Editor")))
            .Add(new MenuSlot("View"));

        levelSelection = new LevelSelection();
        levelSelection.OnSelect = OnLevelSelected;

        layers = new LayersPanel();
        layers.OnLayerSelect = OnLayerSelected;

        entities = new Entities(actorManager, imGuiTexture);
        entities.Enabled = false;
        entities.OnSelectActor = OnSelectActor;

        entityData = new EntityData(this);
        entityData.Enabled = false;
        layers.Add(entities);
        layers.Add(entityData);

        mainCanvas = new EditorCanvas(this, game.GraphicsDevice);

        emptyText = new StaticText(game.GraphicsDevice, Resource.Font, "No Level Selected", 24);

        tools = new Tools();

        tools.AddTool("Pen [1]", () => ToolSelected = Tool.Pen);
        tools.AddTool("Rect [2]", () => ToolSelected = Tool.Rect);
        tools.AddTool("HSym [3]", OnHorizontalSymmetry);
        tools.AddTool("VSym [4]", OnVerticalSymmetry);

        phantomActor = new PhantomActor(actorManager);
        Add(phantomActor);
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
        solidTilesPanel = new TilePanel(imGuiTexture, "tilesets/flight", "SolidTiles");
        bgTilesPanel = new TilePanel(imGuiTexture, "tilesets/flightBG", "BGTiles");

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


    public void SelectLevelActor(LevelActor actor) 
    {
        if (!hasSelection)
            listSelected.Clear();
        if (currentSelected != null) 
        {
            currentSelected.Selected = false;
        }
        listSelected.Add(actor);
        hasSelection = true;
    }

    public void Select(LevelActor actor) 
    {
        entityData.SelectActor(actor);
        currentSelected = actor;
        currentSelected.Selected = true;

    }

    public void UpdateSelected() 
    {
        if (hasSelection) 
        {
            if (listSelected.Count == 1) 
            {
                Select(listSelected[0]);
                listSelected.Clear();
            }
            else 
            {
                entityData.ConflictSelection(listSelected);
            }

            hasSelection = false;
        }
    }

#region Events
    private void OnSelectActor(Actor actor) 
    {
        actorSelected = actor;
        phantomActor.SetActor(actorSelected);
        ToolSelected = Tool.Pen;
    }

    private void OnVerticalSymmetry() 
    {
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
        {
            Layers.Solids => (solids, SolidAutotiler, null),
            Layers.BG => (bgs, bgAutotiler, solids.Bits),
            _ => (null, null, null) 
        };

        if (currentTiles == null)
            return;

        CommitHistory();

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
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
        {
            Layers.Solids => (solids, SolidAutotiler, null),
            Layers.BG => (bgs, bgAutotiler, solids.Bits),
            _ => (null, null, null) 
        };

        if (currentTiles == null)
            return;

        CommitHistory();

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
            CurrentLayer = Layers.Solids;
            break;
        case "BG":
            CurrentLayer = Layers.BG;
            break;
        case "Entities":
            CurrentLayer = Layers.Entities;
            phantomActor.Active = true;
            phantomActor.Visible = true;
            entityData.Enabled = true;
            entities.Enabled = true;
            return;
        case "SolidTiles":
            CurrentLayer = Layers.SolidTiles;
            break;
        case "BGTiles":
            CurrentLayer = Layers.BGTiles;
            break;
        }
        entities.Enabled = false;
        entityData.Enabled = false;
        phantomActor.Active = false;
        phantomActor.Visible = false;
    }

    private void OnLevelSelected(string path)
    {
        SetLevel(path);
    }
#endregion

    public void SetLevel(string path) 
    {
        actorManager.ClearIDs();
        foreach (var entity in EntityList) 
        {
            if (entity is LevelActor) 
            {
                Remove(entity);
            }
        }
        
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

            var bg = loadingLevel["BG"];
            bgs.SetGrid(bg.InnerText);

            var solidTiles = loadingLevel["SolidTiles"];
            SolidTiles.SetTiles(solidTiles.InnerText);

            var bgTiles = loadingLevel["BGTiles"];
            BGTiles.SetTiles(bgTiles.InnerText);

            SpawnEntities(loadingLevel);

            level = loadingLevel;
            currentPath = path;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load this level: '{Path.GetFileName(path)}'");
            Logger.LogError(ex.ToString());
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

                SpawnEntities(loadingLevel);
            }
        }

        var pathSpan = path.AsSpan();
        int seed = 0;
        for (int i = 0; i < pathSpan.Length; i++) 
        {
            seed += (int)pathSpan[i] + i;
        }

        SolidAutotiler.SetInitialSeed(seed);
        bgAutotiler.SetInitialSeed(seed);

        solids.UpdateTiles(SolidAutotiler);
        bgs.UpdateTiles(bgAutotiler, solids.Bits);
    }

    public void RemoveActor(LevelActor actor) 
    {
        Remove(actor);
        HasRemovedEntity = true;
        actorManager.RetriveID(actor.ID);
    }

    private void SpawnEntities(XmlElement xml) 
    {
        var entities = xml["Entities"];
        ulong id = 0;
        HashSet<ulong> idTaken = new();

        foreach (XmlElement entity in entities) 
        {
            var entityName = entity.Name;

            var actor = actorManager.GetEntity(entityName);
            if (actor == null)
            {
                Logger.LogError($"{entityName} is not registered to ActorManager");
                continue;
            }
            ulong entityID = ulong.Parse(entity.GetAttribute("id"));
            var x = int.Parse(entity.GetAttribute("x"));
            var y = int.Parse(entity.GetAttribute("y"));
            int width = 0;
            int height = 0;

            if (entity.HasAttribute("width")) 
            {
                width = int.Parse(entity.GetAttribute("width"));
            }

            if (entity.HasAttribute("height")) 
            {
                height = int.Parse(entity.GetAttribute("height"));
            }

            var levelActor = new LevelActor(Resource.TowerFallTexture, actor, actor.Texture, entityID);
            levelActor.PosX = x;
            levelActor.PosY = y;
            if (actor.ResizeableX)
                levelActor.Width = width;
            if (actor.ResizeableY)
                levelActor.Height = height;
            Add(levelActor);
            if (entityID > id) 
            {
                id = entityID;
            }
            idTaken.Add(entityID);
        }

        actorManager.TotalIDs = id;
        for (ulong i = 0; i <= id; i++) 
        {
            if (idTaken.Contains(i))
                continue;
            actorManager.RetriveID(i);
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

    public void CommitHistory() 
    {
        var historyCommit = CurrentLayer switch {
            Layers.Solids => new History.Commit { Solids = solids.Bits },
            Layers.BG => new History.Commit { BGs = bgs.Bits },
            Layers.BGTiles => new History.Commit { BGTiles = BGTiles.Ids },
            Layers.SolidTiles => new History.Commit { SolidTiles = SolidTiles.Ids },
            _ => throw new InvalidOperationException()
        };
        history.PushCommit(historyCommit, CurrentLayer);
    }

    public override void Update(double delta)
    {
        imGui.Update(GameInstance.Inputs, ImGuiCallback);
        solidTilesPanel.Update();
        bgTilesPanel.Update();

        if (level == null)
        {
            return;
        }

        if (hasSelection) 
        {
            UpdateSelected();
        }


        if (isDrawing && Input.InputSystem.Mouse.AnyPressedButton.IsUp) 
        {
            isDrawing = false;
        }

        if (Input.InputSystem.Keyboard.IsDown(KeyCode.LeftControl)) 
        {
            if (Input.InputSystem.Keyboard.IsDown(KeyCode.LeftShift)) 
            {
                if (Input.InputSystem.Keyboard.IsPressed(KeyCode.S))  
                {
                    Save(true);
                }
            }
            else if (Input.InputSystem.Keyboard.IsPressed(KeyCode.S)) 
            {
                Save();
            }

            if (!isDrawing && Input.InputSystem.Keyboard.IsPressed(KeyCode.Z)) 
            {
                if (history.PopCommit(out var res))
                {
                    switch (res.Layer) 
                    {
                    case Layers.Solids:
                        solids.Clear();
                        solids.Bits = res.Solids.Clone();
                        solids.UpdateTiles(SolidAutotiler);
                        break;
                    case Layers.BG:
                        bgs.Clear();
                        bgs.Bits = res.BGs.Clone();
                        bgs.UpdateTiles(bgAutotiler, solids.Bits);
                        break;
                    case Layers.BGTiles:
                        BGTiles.Clear();
                        BGTiles.SetTiles(res.BGTiles);
                        break;
                    case Layers.SolidTiles:
                        SolidTiles.Clear();
                        SolidTiles.SetTiles(res.SolidTiles);
                        break;
                    }
                }
            }
        }
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;

        if (Input.InputSystem.Mouse.LeftButton.IsPressed) 
        {
            if (ToolSelected == Tool.Pen)
            switch (CurrentLayer) 
            {
            case Layers.Entities:
                {
                    int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                    int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                    if (InBounds(gridX, gridY)) 
                    {
                        var actor = phantomActor.PlaceActor(this);
                        if (actor != null) 
                        {
                            Select(actor);
                        }
                        ToolSelected = Tool.Rect;
                    }

                }
                break;
            }
        }

        if (Input.InputSystem.Mouse.LeftButton.IsDown) 
        {
            if (ToolSelected == Tool.Pen)
            {
                Place(x, y, true);
            }
        }
        else if (Input.InputSystem.Mouse.RightButton.IsDown) 
        {
            if (ToolSelected == Tool.Pen)
            {
                Place(x, y, false);
            }
        }
        HasRemovedEntity = false;
    }

    private void Place(int x, int y, bool placeTile) 
    {
        switch (CurrentLayer) 
        {
        case Layers.Solids:
        case Layers.BG:
            (GridTiles currentTile, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
            {
                Layers.Solids => (solids, SolidAutotiler, null),
                _ => (bgs, bgAutotiler, solids.Bits)
            };
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

                if (InBounds(gridX, gridY)) 
                {
                    if (!isDrawing) 
                    {
                        CommitHistory();
                    }

                    if (currentTile.SetGrid(gridX, gridY, placeTile)) 
                    {
                        currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY, also));
                        UpdateTiles(gridX, gridY, solids.Bits);
                    }
                    isDrawing = true;
                }
            }
            break;
        case Layers.BGTiles:
        case Layers.SolidTiles:
            if (!bgTilesPanel.IsWindowHovered && !solidTilesPanel.IsWindowHovered)
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                var data = bgTilesPanel.GetData();
                if (!isDrawing && InBounds(gridX, gridY)) 
                {
                    CommitHistory();
                }
                for (int nx = 0; nx < data.Columns; nx++) 
                {
                    for (int ny = 0; ny < data.Rows; ny++) 
                    {
                        int relNx = gridX + nx;
                        int relNy = gridY + ny;
                        if (InBounds(relNx, relNy)) 
                        {
                            int tile = placeTile ? data[ny, nx] : -1;
                            BGTiles.SetTile(relNx, relNy, tile);
                        }
                    }
                }
                isDrawing = true;
            }
            break;
        }
    }

    private static bool InBounds(int gridX, int gridY) 
    {
        return gridX > -1 && gridY > -1 && gridX < 32 && gridY < 24;
    }

    private void ImGuiCallback()
    {
#if DEBUG
        ImGui.ShowDemoWindow();
#endif

        menuBar.UpdateGui();
        levelSelection.UpdateGui();
        tools.UpdateGui();
        layers.UpdateGui();
        switch (CurrentLayer) 
        {
        case Layers.SolidTiles:
            solidTilesPanel.UpdateGui();
            break;
        case Layers.BGTiles:
            bgTilesPanel.UpdateGui();
            break;
        }
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer)
    {
        mainCanvas.Draw(buffer, batch);
        batch.Begin();
        batch.Add(mainCanvas.CanvasTexture, GameContext.GlobalSampler, new Vector2(WorldUtils.WorldX, WorldUtils.WorldY), Color.White, 
            new Vector2(2));
        if (level == null) 
        {
            emptyText.Draw(batch, new Vector2(WorldUtils.WorldX + 40, WorldUtils.WorldY + (210)));
        }
        batch.End(buffer);

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

        var Solids = document.CreateElement("Solids");
        Solids.SetAttribute("exportMode", "Bitstring");
        Solids.InnerText = solid;

        var BG = document.CreateElement("BG");
        BG.SetAttribute("exportMode", "Bitstring");
        BG.InnerText = bg;

        var SolidTiles = document.CreateElement("SolidTiles");
        SolidTiles.SetAttribute("exportMode", "TrimmedCSV");
        SolidTiles.InnerText = this.SolidTiles.Save();

        var BGTiles = document.CreateElement("BGTiles");
        BGTiles.SetAttribute("exportMode", "TrimmedCSV");
        BGTiles.InnerText = this.BGTiles.Save();

        var Entities = document.CreateElement("Entities");
        foreach (var entity in EntityList) 
        {
            if (entity is not LevelActor actor)
                continue;
            var actorInfo = actor.Save();
            var element = document.CreateElement(actorInfo.Name);
            element.SetAttribute("id", actorInfo.ID.ToString());
            element.SetAttribute("x", actorInfo.X.ToString());
            element.SetAttribute("y", actorInfo.Y.ToString());
            foreach (var value in actorInfo.Values) 
            {
                element.SetAttribute(value.Key, value.Value.ToString());
            }
            Entities.AppendChild(element);
        }

        rootElement.AppendChild(Solids);
        rootElement.AppendChild(BG);
        rootElement.AppendChild(SolidTiles);
        rootElement.AppendChild(BGTiles);
        rootElement.AppendChild(Entities);
        string path;
        if (specifyPath) 
        {
            NFDResult result = FileDialog.Save(null, "xml");
            if (result.IsOk) 
            {
                path = result.Path;
            }
            return;
        }
        else 
        {
            path = currentPath;
        }

        document.Save(path + ".test");
    }

    public override void End()
    {
        emptyText.Dispose();
    }
}