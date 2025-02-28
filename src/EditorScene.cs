using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Numerics;
using ImGuiNET;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;
using Riateu.Inputs;
using System.Runtime.InteropServices;

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
    Rect,
    Node
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
    private TowerSettings towerSettings;
    private NewTower newTower;
#endregion
    private Autotiler SolidAutotiler;
    private Autotiler BgAutotiler;


    private Actor actorSelected;
    private bool isDrawing;
    private IntPtr imGuiTexture;
    private Batch batch;
    private Batch levelBatch;
    private RenderTarget target;

#region Level State
    private Tower tower;
    private LevelActor actorToRemove;
    private LevelActor currentSelected;
    private List<LevelActor> listSelected = [];
    private Level currentLevel;
    private TileRect tileRect;
    private bool hasSelection;
    private bool[] visibility = [true, true, true, true, true];

    public Tool ToolSelected;
    public Layers CurrentLayer = Layers.Solids;
    public bool HasRemovedEntity;

    public PhantomActor PhantomActor;
#endregion

    private BackdropRenderer backdropRenderer;

    private EntityMenu entityMenu;

    private bool openFallbackTheme = false;
    private int fallbackSelected = 0;
    private SaveState saveState;
    private MenuSelectionItem recentItems;

    public EditorScene(GameApp game, ImGuiRenderer renderer, SaveState state) : base(game)
    {
        saveState = state;
        target = new RenderTarget(game.GraphicsDevice, (uint)WorldUtils.WorldWidth + 100, (uint)WorldUtils.WorldHeight);

        levelBatch = new Batch(game.GraphicsDevice, (int)WorldUtils.WorldWidth + 100, (int)WorldUtils.WorldHeight);
        batch = new Batch(game.GraphicsDevice, 1280, 640);
        actorManager = new ActorManager();
        PhantomActor = new PhantomActor(this, actorManager);
        tileRect = new TileRect();
        VanillaActor.Init(actorManager, saveState);
        imGui = renderer;
        imGuiTexture = imGui.BindTexture(Resource.TowerFallTexture);
        recentItems = new MenuSelectionItem("Open Recent");
        foreach (var recentTower in saveState.RecentTowers)
        {
            recentItems.Add(new MenuItem(recentTower, () => OpenTowerFile(recentTower)));
        }

        menuBar = new MenuBar()
            .Add(new MenuSlot("File")
                .Add(new MenuItem("New", New))
                .Add(new MenuItem("Open", Open))
                .Add(recentItems)
                .Add(new MenuItem("Save", () => Save()))
                .Add(new MenuItem("Save As", () => Save(true)))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuSlot("Settings")
                .Add(new MenuItem("Theme", OnOpenTheme))
                .Add(new MenuItem("Editor")))
            .Add(new MenuSlot("View"));

        levelSelection = new LevelSelection();
        levelSelection.OnSelect = OnLevelSelected;
        levelSelection.OnCreated = OnLevelCreated;

        layers = new LayersPanel();
        layers.OnLayerSelect = OnLayerSelected;
        layers.ShowOrHide = OnShowOrHideLayer;

        entities = new Entities(actorManager, imGuiTexture);
        entities.Enabled = false;
        entities.OnSelectActor = OnSelectActor;

        entityData = new EntityData(this);
        entityData.Enabled = false;
        layers.Add(entities);
        layers.Add(entityData);

        Themes.TryGetTheme("Flight", out var theme);
        backdropRenderer = new BackdropRenderer();
        backdropRenderer.SetTheme(theme);
        towerSettings = new TowerSettings();
        towerSettings.OnSave = OnSettingsChangeTheme;
        towerSettings.SetTheme(theme);

        tools = new Tools();

        tools.AddTool(FA6.Pen, () => ToolSelected = Tool.Pen);
        tools.AddTool(FA6.UpDownLeftRight, () => ToolSelected = Tool.Rect);
        tools.AddTool(FA6.LeftRight, OnHorizontalSymmetry);
        tools.AddTool(FA6.UpDown, OnVerticalSymmetry);
        tools.AddTool(FA6.CircleNodes, () => ToolSelected = Tool.Node);

        entityMenu = new EntityMenu(actorManager, imGuiTexture);
        entityMenu.OnSelectActor = OnSelectActor;
        entityMenu.Enabled = false;

        newTower = new NewTower(saveState);
        newTower.OnCreateTower = OnCreateTower;
        newTower.Enabled = false;
    }

    public override void Begin()
    {
        solidTilesPanel = new TilePanel(imGuiTexture, "tilesets/flight", "SolidTiles");
        bgTilesPanel = new TilePanel(imGuiTexture, "tilesets/flightBG", "BGTiles");
        SolidAutotiler = new Autotiler();
        BgAutotiler = new Autotiler();
    }

    public void SetTowerTheme(string name) 
    {
        if (!Themes.TryGetTheme(name, out var theme)) 
        {
            openFallbackTheme = true;
            return;
        }
        tower.SetTheme(theme);
        SetTheme(theme);
    }

    private void SetTheme(Theme theme) 
    {
        towerSettings.SetTheme(theme);
        backdropRenderer.SetTheme(theme);
        XmlDocument doc = new XmlDocument();

        doc.Load(Path.Combine(saveState.TFPath, "Content", "Atlas", "GameData", "tilesetData.xml"));
        var tilesetData = doc["TilesetData"];

        foreach (var level in levelSelection.Levels) 
        {
            foreach (XmlElement tileset in tilesetData.GetElementsByTagName("Tileset")) 
            {
                var id = tileset.GetAttribute("id");
                if (id == theme.SolidTilesetID) 
                {
                    SolidAutotiler.Init(tileset);
                    solidTilesPanel.SetTheme(tileset);
                    if (level.SolidTiles != null)
                    {
                        level.SolidTiles.SetTheme(tileset);
                        level.SolidTiles.UpdateTiles();
                        level.Solids.SetTheme(tileset);
                        level.Solids.UpdateTiles(SolidAutotiler);
                    }
                }

                else if (id == theme.BGTilesetID)
                {
                    BgAutotiler.Init(tileset);
                    bgTilesPanel.SetTheme(tileset);
                    if (level.BGTiles != null)
                    {
                        level.BGTiles.SetTheme(tileset);
                        level.BGTiles.UpdateTiles();
                        level.BGs.SetTheme(tileset);
                        level.BGs.UpdateTiles(BgAutotiler, level.Solids.Bits);
                    }
                }
            }
        }
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
        if (currentSelected != null) 
        {
            currentSelected.Selected = false;
        }
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
    private void OnLevelCreated()
    {
        if (currentLevel != null)
        {
            Save();
        }
        SetTheme(tower.Theme);
    }

    private void OnCreateTower(string towerName, string towerPath, int currentMode, int currentTheme)
    {
        var directory = Path.Combine(towerPath, towerName);
        Directory.CreateDirectory(directory);

        XmlDocument tower = new XmlDocument();
        var rootElement = tower.CreateElement("tower");
        tower.AppendChild(rootElement);

        var theme = tower.CreateElement("theme");
        theme.InnerText = Themes.ThemeNames[currentTheme];
        rootElement.AppendChild(theme);

        switch (currentMode)
        {
        case 0: // Versus
            var treasure = tower.CreateElement("treasure");
            treasure.InnerText = "Arrows,Shield,Wings,TimeOrb,DarkOrb,LavaOrb";
            rootElement.AppendChild(treasure);
            break;
        case 1: // Quest
            XmlDocument data = new XmlDocument();
            var dataRootElement = data.CreateElement("data");
            data.AppendChild(dataRootElement);

            var normal = data.CreateElement("normal");
            normal.AppendChild(data.CreateComment("Add all of your wave group sequence here"));
            dataRootElement.AppendChild(normal);

            var hardcore = data.CreateElement("hardcore");
            hardcore.AppendChild(data.CreateComment("Add all of your wave group sequence here"));
            dataRootElement.AppendChild(hardcore);
            data.Save(Path.Combine(directory, "data.xml"));
            break;
        case 2: // Dark World
            // WIP
            break;
        }

        var filepath = Path.Combine(directory, "tower.xml");
        tower.Save(filepath);

        OpenTowerFile(filepath);
        saveState.AddToRecent(filepath);
        SaveIO.SaveJson<SaveState>("towersave.json", saveState, SaveStateContext.Default.SaveState);
        recentItems.Add(new MenuItem(filepath, () => OpenTowerFile(filepath)));
    }

    private void OnShowOrHideLayer(int id, bool visible)
    {
        visibility[id] = visible; 
    }

    private void OnSettingsChangeTheme(int num)
    {
        SetTowerTheme(Themes.ThemeNames[num]);
    }

    private void OnSelectActor(Actor actor) 
    {
        actorSelected = actor;
        PhantomActor.SetActor(actorSelected);
        ToolSelected = Tool.Pen;
    }

    private void OnVerticalSymmetry() 
    {
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
        {
            Layers.Solids => (currentLevel.Solids, SolidAutotiler, null),
            Layers.BG => (currentLevel.BGs, BgAutotiler, currentLevel.Solids.Bits),
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
                    UpdateTiles(x, bitY, currentLevel.Solids.Bits);
                }
            }
        }
    }

    private void OnHorizontalSymmetry() 
    {
        (GridTiles currentTiles, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
        {
            Layers.Solids => (currentLevel.Solids, SolidAutotiler, null),
            Layers.BG => (currentLevel.BGs, BgAutotiler, currentLevel.Solids.Bits),
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
                    UpdateTiles(bitX, y, currentLevel.Solids.Bits);
                }
            }
        }
    }

    private void OnLayerSelected(string layer)
    {
        switch (layer) 
        {
        case FA6.BorderAll + " Solids":
            CurrentLayer = Layers.Solids;
            break;
        case FA6.BorderNone + " BG":
            CurrentLayer = Layers.BG;
            break;
        case FA6.Person + " Entities":
            CurrentLayer = Layers.Entities;
            PhantomActor.Active = true;
            entityData.Enabled = true;
            entities.Enabled = true;
            return;
        case FA6.SquareFull + " SolidTiles":
            CurrentLayer = Layers.SolidTiles;
            break;
        case FA6.SquareMinus + " BGTiles":
            CurrentLayer = Layers.BGTiles;
            break;
        }
        entities.Enabled = false;
        entityData.Enabled = false;
        PhantomActor.Active = false;
    }

    private void OnLevelSelected(Level level)
    {
        SetLevel(level);
    }
#endregion

    public void SetLevel(Level level) 
    {
        if (level == null) 
        {
            level = null;
            return;
        }

        actorManager.ClearIDs();
        

        int seed = level.Seed;

        SolidAutotiler.Seed = seed;
        BgAutotiler.Seed = seed;

        if (level.Unsaved) 
        {
            currentLevel = level;
            if (currentLevel.Width == 420)
            {
                WorldUtils.TurnWide();
            }
            else 
            {
                WorldUtils.TurnStandard();
            }
            return;
        }

        level.Actors.Clear();
        
        XmlDocument document = new XmlDocument();
        document.Load(level.Path);

        var loadingLevel = document["level"];
#if !DEBUG
        try 
        {
#endif
            currentLevel = level;

            if (int.TryParse(loadingLevel.GetAttribute("width"), out int width))
            {
                currentLevel.Width = width;
            }
            else 
            {
                currentLevel.Width = 320;
            }

            if (int.TryParse(loadingLevel.GetAttribute("height"), out int height))
            {
                currentLevel.Height = height;
            }
            else 
            {
                currentLevel.Height = 240;
            }

            if (currentLevel.Width == 420)
            {
                if (!WorldUtils.TurnWide())
                {
                    var solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, towerSettings.Theme.SolidTilesQuad, 10, 10);
                    var bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, towerSettings.Theme.BGTilesQuad, 10, 10);
                    level.Solids = new GridTiles(Resource.TowerFallTexture, solidSpriteSheet, 42, 24);

                    level.BGs = new GridTiles(Resource.TowerFallTexture, bgSpriteSheet, 42, 24);
                    level.SolidTiles = new Tiles(Resource.TowerFallTexture, solidSpriteSheet, 42, 24);
                    level.BGTiles = new Tiles(Resource.TowerFallTexture, bgSpriteSheet, 42, 24);
                }
            }
            else 
            {
                WorldUtils.TurnStandard();
                var solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, towerSettings.Theme.SolidTilesQuad, 10, 10);
                var bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, towerSettings.Theme.BGTilesQuad, 10, 10);
                level.Solids = new GridTiles(Resource.TowerFallTexture, solidSpriteSheet, 32, 24);

                level.BGs = new GridTiles(Resource.TowerFallTexture, bgSpriteSheet, 32, 24);
                level.SolidTiles = new Tiles(Resource.TowerFallTexture, solidSpriteSheet, 32, 24);
                level.BGTiles = new Tiles(Resource.TowerFallTexture, bgSpriteSheet, 32, 24);
            }

            var solid = loadingLevel["Solids"];
            currentLevel.Solids.SetGrid(solid.InnerText);

            var bg = loadingLevel["BG"];
            currentLevel.BGs.SetGrid(bg.InnerText);

            var solidTiles = loadingLevel["SolidTiles"];
            currentLevel.SolidTiles.SetTiles(solidTiles.InnerText);

            var bgTiles = loadingLevel["BGTiles"];
            currentLevel.BGTiles.SetTiles(bgTiles.InnerText);

            SpawnEntities(loadingLevel);
#if !DEBUG
        }
        catch (Exception ex)
        {
            string path = level.Path;
            Logger.Error($"Failed to load this level: '{Path.GetFileName(path)}'");
            Logger.Error(ex.ToString());
        }
#endif

        currentLevel.Solids.UpdateTiles(SolidAutotiler);
        currentLevel.BGs.UpdateTiles(BgAutotiler, currentLevel.Solids.Bits);
    }

    public void RemoveActor(LevelActor actor) 
    {
        actorToRemove = actor;
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
                Logger.Error($"{entityName} is not registered to ActorManager");
                continue;
            }
            ulong entityID = ulong.Parse(entity.GetAttribute("id"));
            var x = int.Parse(entity.GetAttribute("x"));
            var y = int.Parse(entity.GetAttribute("y"));
            int width = 0;
            int height = 0;
            List<Vector2> nodes = actor.HasNodes ? new List<Vector2>() : null;

            if (entity.HasAttribute("width")) 
            {
                width = int.Parse(entity.GetAttribute("width"));
            }

            if (entity.HasAttribute("height")) 
            {
                height = int.Parse(entity.GetAttribute("height"));
            }

            if (entity.HasChildNodes) 
            {
                nodes = new List<Vector2>();
                foreach (XmlElement child in entity.ChildNodes) 
                {
                    var nx = int.Parse(child.GetAttribute("x"));
                    var ny = int.Parse(child.GetAttribute("y"));

                    Vector2 node = new Vector2(nx, ny);
                    nodes.Add(node);
                }
            }

            Dictionary<string, object> customDatas = new Dictionary<string, object>();

            if (actor.CustomValues != null)
            {
                foreach (XmlAttribute attr in entity.Attributes)
                {
                    if (!actor.CustomValues.ContainsKey(attr.Name)) 
                    {
                        continue;
                    }

                    var value = attr.Value;
                    if (int.TryParse(value, out var output)) 
                    {
                        customDatas.Add(attr.Name, output);
                    }
                    else if (float.TryParse(value, out var output2)) 
                    {
                        customDatas.Add(attr.Name, output2);
                    }
                    else if (attr.Value.ToLowerInvariant() == "true") 
                    {
                        customDatas.Add(attr.Name, true);
                    }
                    else if (attr.Value.ToLowerInvariant() == "false") 
                    {
                        customDatas.Add(attr.Name, false);
                    }
                    else 
                    {
                        customDatas.Add(attr.Name, value);
                    }
                }
            }

            var levelActor = new LevelActor(Resource.TowerFallTexture, actor, entityID);
            levelActor.PosX = x;
            levelActor.PosY = y;
            levelActor.Data.HasNodes = actor.HasNodes;
            levelActor.Nodes = nodes;
            levelActor.CustomData = customDatas;
            if (actor.ResizeableX) 
            {
                levelActor.Data.ResizeableX = true;
                levelActor.Width = width;
            }
            if (actor.ResizeableY) 
            {
                levelActor.Data.ResizeableY = true;
                levelActor.Height = height;
            }
            levelActor.Scene = this;
            currentLevel.Actors.Add(levelActor);

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
        currentLevel.Solids.UpdateTile(gridX - 1, gridY, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX - 1, gridY));
        currentLevel.Solids.UpdateTile(gridX + 1, gridY, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX + 1, gridY));

        currentLevel.Solids.UpdateTile(gridX, gridY - 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX, gridY - 1));
        currentLevel.Solids.UpdateTile(gridX, gridY + 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX, gridY + 1));

        currentLevel.Solids.UpdateTile(gridX - 1, gridY - 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX - 1, gridY - 1));
        currentLevel.Solids.UpdateTile(gridX + 1, gridY + 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX + 1, gridY + 1));

        currentLevel.Solids.UpdateTile(gridX - 1, gridY + 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX - 1, gridY + 1));
        currentLevel.Solids.UpdateTile(gridX + 1, gridY - 1, SolidAutotiler.Tile(currentLevel.Solids.Bits, gridX + 1, gridY - 1));

        currentLevel.BGs.UpdateTile(gridX - 1, gridY, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX - 1, gridY, also));
        currentLevel.BGs.UpdateTile(gridX + 1, gridY, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX + 1, gridY, also));

        currentLevel.BGs.UpdateTile(gridX, gridY - 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX, gridY - 1, also));
        currentLevel.BGs.UpdateTile(gridX, gridY + 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX, gridY + 1, also));

        currentLevel.BGs.UpdateTile(gridX - 1, gridY - 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX - 1, gridY - 1, also));
        currentLevel.BGs.UpdateTile(gridX + 1, gridY + 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX + 1, gridY + 1, also));

        currentLevel.BGs.UpdateTile(gridX - 1, gridY + 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX - 1, gridY + 1, also));
        currentLevel.BGs.UpdateTile(gridX + 1, gridY - 1, BgAutotiler.Tile(currentLevel.BGs.Bits, gridX + 1, gridY - 1, also));
    }

    public void CommitHistory() 
    {
        var historyCommit = CurrentLayer switch {
            Layers.Solids => new History.Commit { Solids = currentLevel.Solids.Bits },
            Layers.BG => new History.Commit { BGs = currentLevel.BGs.Bits },
            Layers.BGTiles => new History.Commit { BGTiles = currentLevel.BGTiles.Ids },
            Layers.SolidTiles => new History.Commit { SolidTiles = currentLevel.SolidTiles.Ids },
            _ => throw new InvalidOperationException()
        };
        currentLevel.PushCommit(historyCommit, CurrentLayer);
    }

    public override void Process(double delta)
    {
        if (currentLevel != null && visibility[2]) 
        {
            foreach (var actor in currentLevel.Actors) 
            {
                actor.Update(delta);
            }
            if (actorToRemove != null) 
            {
                currentLevel.RemoveActor(actorToRemove);
                HasRemovedEntity = true;
                actorManager.RetriveID(actorToRemove.ID);
                actorToRemove = null;
            }
        }
        
        solidTilesPanel.Update();
        bgTilesPanel.Update();

        if (CurrentLayer == Layers.Entities && Input.Keyboard.IsPressed(KeyCode.E))
        {
            entityMenu.Enabled = !entityMenu.Enabled;
        }

        if (currentLevel == null || openFallbackTheme)
        {
            return;
        }

        if (hasSelection) 
        {
            UpdateSelected();
        }


        if (isDrawing && Input.Mouse.AnyPressedButton.IsUp) 
        {
            isDrawing = false;
        }

        if (Input.Keyboard.IsPressed(KeyCode.D1)) 
        {
            ToolSelected = Tool.Pen;
        }
        else if (Input.Keyboard.IsPressed(KeyCode.D2)) 
        {
            ToolSelected = Tool.Rect;
        }
        else if (Input.Keyboard.IsDown(KeyCode.LeftShift) && Input.Keyboard.IsPressed(KeyCode.H)) 
        {
            OnHorizontalSymmetry();
        }
        else if (Input.Keyboard.IsDown(KeyCode.LeftShift) && Input.Keyboard.IsPressed(KeyCode.V)) 
        {
            OnVerticalSymmetry();
        }
        else if (Input.Keyboard.IsPressed(KeyCode.D5)) 
        {
            ToolSelected = Tool.Node;
        }

        if (Input.Keyboard.IsDown(KeyCode.LeftControl)) 
        {
            if (Input.Keyboard.IsDown(KeyCode.LeftShift)) 
            {
                if (Input.Keyboard.IsPressed(KeyCode.S))  
                {
                    Save(true);
                }
            }
            else if (Input.Keyboard.IsPressed(KeyCode.S)) 
            {
                Save();
            }

            if (!isDrawing && Input.Keyboard.IsPressed(KeyCode.Z)) 
            {
                if (currentLevel.PopCommit(out var res))
                {
                    switch (res.Layer) 
                    {
                    case Layers.Solids:
                        currentLevel.Solids.Clear();
                        currentLevel.Solids.Bits = res.Solids.Clone();
                        currentLevel.Solids.UpdateTiles(SolidAutotiler);
                        break;
                    case Layers.BG:
                        currentLevel.BGs.Clear();
                        currentLevel.BGs.Bits = res.BGs.Clone();
                        currentLevel.BGs.UpdateTiles(BgAutotiler, currentLevel.Solids.Bits);
                        break;
                    case Layers.BGTiles:
                        currentLevel.BGTiles.Clear();
                        currentLevel.BGTiles.SetTiles(res.BGTiles);
                        break;
                    case Layers.SolidTiles:
                        currentLevel.SolidTiles.Clear();
                        currentLevel.SolidTiles.SetTiles(res.SolidTiles);
                        break;
                    }
                }
            }
        }

        int x = Input.Mouse.X;
        int y = Input.Mouse.Y;

        if (tileRect.Started) 
        {
            tileRect.Update(x, y);
        }

        if (Input.Mouse.LeftButton.Released && tileRect.Started) 
        {
            TileMouseReleased(0);
        }

        if (Input.Mouse.RightButton.Released && tileRect.Started) 
        {
            TileMouseReleased(1);
        }

        if (ImGui.GetIO().WantCaptureMouse) 
        {
            HasRemovedEntity = false;
            return;
        }

        // Try to not update when it is not visible
        if (!visibility[(int)CurrentLayer]) 
        {
            HasRemovedEntity = false;
            return;
        }

        PhantomActor.Update(delta);

        if (Input.Mouse.LeftButton.Pressed) 
        {
            if (ToolSelected == Tool.Pen)
            {
                switch (CurrentLayer) 
                {
                case Layers.Entities:
                    {
                        int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                        int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                        if (WorldUtils.InBounds(gridX, gridY) && actorSelected != null) 
                        {
                            ulong id = actorManager.GetID();
                            var actor = new LevelActor(Resource.TowerFallTexture, actorSelected, id);
                            actor.Scene = this;
                            actor.PosX = PhantomActor.PosX;
                            actor.PosY = PhantomActor.PosY;
                            currentLevel.AddActor(actor);
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
            else if (ToolSelected == Tool.Node && currentSelected != null && currentSelected.Data.HasNodes) 
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                if (WorldUtils.InBounds(gridX, gridY)) 
                {
                    int posX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
                    int posY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
                    Vector2 position = new Vector2(posX, posY);
                    currentSelected.AddNode(position);
                    currentLevel.Unsaved = true;
                }
            }
        }

        if (!tileRect.Started) 
        {
            if (Input.Mouse.LeftButton.IsDown) 
            {
                if (ToolSelected == Tool.Pen)
                {
                    if (Input.Keyboard.IsHeld(KeyCode.LeftShift)) 
                    {
                        int posX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                        int posY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                        tileRect.Start(posX, posY, TileRect.Type.Place);
                    }
                    else 
                    {
                        Place(x, y, true);
                    }
                }
            }
            else if (Input.Mouse.RightButton.IsDown) 
            {
                if (Input.Keyboard.IsHeld(KeyCode.LeftShift)) 
                {
                    int posX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                    int posY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                    tileRect.Start(posX, posY, TileRect.Type.Remove);
                }
                else 
                {
                    Place(x, y, false);
                }
            }
        }

        HasRemovedEntity = false;
    }

    private void TileMouseReleased(int buttonID) 
    {
        const int LeftClicked = 0;
        const int RightClicked = 1;
        tileRect.Started = false;
        tileRect.AdjustIfNeeded();

        CommitHistory();

        for (int dx = 0; dx < tileRect.Width / 10; dx += 1) 
        {
            for (int dy = 0; dy < tileRect.Height / 10; dy += 1) 
            {
                int gridX = WorldUtils.ToGrid(tileRect.StartX) + dx;
                int gridY = WorldUtils.ToGrid(tileRect.StartY) + dy;

                if (WorldUtils.InBounds(gridX, gridY)) 
                {
                    if (buttonID == LeftClicked) 
                    {
                        PlaceGrid(gridX, gridY, true);
                    }
                    else if (buttonID == RightClicked) 
                    {
                        PlaceGrid(gridX, gridY, false);
                    }
                }
            }
        }

        tileRect.Width = 0;
        tileRect.Height = 0;
    }

    private void PlaceGrid(int gridX, int gridY, bool placeTile) 
    {
        (GridTiles currentTile, Autotiler autotiler, Array2D<bool> also) = CurrentLayer switch 
        {
            Layers.Solids => (currentLevel.Solids, SolidAutotiler, null),
            _ => (currentLevel.BGs, BgAutotiler, currentLevel.Solids.Bits)
        };
        {
            if (currentTile.SetGrid(gridX, gridY, placeTile)) 
            {
                currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY, also));
                UpdateTiles(gridX, gridY, currentLevel.Solids.Bits);
            }
            isDrawing = true;
        }
    }

    private void PlaceDecor(int gridX, int gridY, bool placeTile, TilePanel panel, Tiles tiles) 
    {
        if (!panel.IsWindowHovered)
        {
            var data = panel.GetData();
            if (!isDrawing && WorldUtils.InBounds(gridX, gridY)) 
            {
                CommitHistory();
            }
            for (int nx = 0; nx < data.Columns; nx++) 
            {
                for (int ny = 0; ny < data.Rows; ny++) 
                {
                    int relNx = gridX + nx;
                    int relNy = gridY + ny;
                    if (WorldUtils.InBounds(relNx, relNy)) 
                    {
                        int tile = placeTile ? data[ny, nx] : -1;
                        tiles.SetTile(relNx, relNy, tile);
                    }
                }
            }
            isDrawing = true;
        }
    }

    private void Place(int x, int y, bool placeTile) 
    {
        switch (CurrentLayer) 
        {
        case Layers.Solids:
        case Layers.BG:
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

                if (!isDrawing) 
                {
                    CommitHistory();
                }

                if (WorldUtils.InBounds(gridX, gridY)) 
                {
                    PlaceGrid(gridX, gridY, placeTile);
                }
            }

            break;
        case Layers.BGTiles:
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                PlaceDecor(gridX, gridY, placeTile, bgTilesPanel, currentLevel.BGTiles);
            }
            break;
        case Layers.SolidTiles:
            {
                int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));
                PlaceDecor(gridX, gridY, placeTile, solidTilesPanel, currentLevel.SolidTiles);
            }

            break;
        }
    }

    private void OnOpenTheme() 
    {
        towerSettings.Enabled = true;
    }

    private void ImGuiCallback()
    {
#if DEBUG
        ImGui.ShowDemoWindow();
#endif

        if (towerSettings.Enabled) 
        {
            ImGui.OpenPopup("Theme Settings");
        }

        if (newTower.Enabled) 
        {
            ImGui.OpenPopup("New Tower");
        }

        if (openFallbackTheme) 
        {
            ImGui.OpenPopup("Fallback Theme");
        }

        newTower.DrawGui();
        towerSettings.DrawGui();

        if (ImGui.BeginPopupModal("Fallback Theme", ref openFallbackTheme, ImGuiWindowFlags.AlwaysAutoResize)) 
        {
            ImGui.Text("Theme not found, fallback to another one: ");
            ImGui.Combo("Fallback Theme", ref fallbackSelected, Themes.ThemeNames, Themes.ThemeNames.Length);

            if (ImGui.Button("OK")) 
            {
                var name = Themes.ThemeNames[fallbackSelected];
                Themes.TryGetTheme(name, out var theme);
                tower.SetTheme(theme);
                SetTheme(tower.Theme);
                openFallbackTheme = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        
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
        entityMenu.UpdateGui();
    }

    public override void Render(CommandBuffer commandBuffer, RenderTarget swapchain)
    {
        imGui.Update(GameInstance.InputDevice, ImGuiCallback);

        {
            levelBatch.Begin(Resource.BGAtlasTexture, DrawSampler.PointClamp);
            backdropRenderer.Draw(levelBatch);
            levelBatch.End();

            levelBatch.Begin(Resource.TowerFallTexture, DrawSampler.PointClamp);

            DrawGrid();
            if (currentLevel != null) 
            {
                if (visibility[1])
                {
                    currentLevel.BGs.Draw(levelBatch);
                }

                if (visibility[0]) 
                {
                    currentLevel.Solids.Draw(levelBatch);
                }

                if (visibility[4])
                {
                    currentLevel.BGTiles.Draw(levelBatch);
                }

                if (visibility[3])
                {
                    currentLevel.SolidTiles.Draw(levelBatch);
                }

                if (visibility[2])
                {
                    foreach (var actor in currentLevel.Actors) 
                    {
                        actor.Draw(levelBatch);
                    }
                }
            }
            tileRect.Draw(levelBatch);
            PhantomActor.Draw(levelBatch);
            levelBatch.End();

            levelBatch.Flush(commandBuffer);

            var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(target, Color.Black, true));
            renderPass.BindGraphicsPipeline(GameContext.BatchMaterial.ShaderPipeline);
            levelBatch.Render(renderPass);
            commandBuffer.EndRenderPass(renderPass);
        }
        
        {
            batch.Begin(target, DrawSampler.PointClamp);
            batch.Draw(
                new TextureQuad(
                    new Point(420, 240),
                    new Rectangle(0, 0, (int)WorldUtils.WorldWidth, (int)WorldUtils.WorldHeight)), 
                new Vector2(WorldUtils.WorldX, WorldUtils.WorldY), 
                Color.White, 
                new Vector2(2)
            );
            batch.End();

            batch.Flush(commandBuffer);

            var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(swapchain, Color.Black, true));
            renderPass.BindGraphicsPipeline(GameContext.BatchMaterial.ShaderPipeline);
            batch.Render(renderPass);
            imGui.Render(commandBuffer, renderPass);
            commandBuffer.EndRenderPass(renderPass);
        }
    }

    private void DrawGrid() 
    {
        for (int i = 0; i < WorldUtils.WorldWidth / 10; i++) 
        {
            DrawUtils.Line(levelBatch, new Vector2(i * 10, 0), new Vector2(i * 10, WorldUtils.WorldHeight), Color.White * 0.1f);
        }

        for (int i = 0; i < WorldUtils.WorldHeight / 10; i++) 
        {
            DrawUtils.Line(levelBatch, new Vector2(0, i * 10), new Vector2(WorldUtils.WorldWidth, i * 10), Color.White * 0.1f);
        }
    }

    private void OpenTowerFile(string filepath)
    {
        tower = new Tower();
        if (tower.Load(filepath)) 
        {
            levelSelection.SelectTower(tower);
            SetTheme(tower.Theme);
        }
        else 
        {
            levelSelection.SelectTower(tower);
            openFallbackTheme = true;
        }
        var dataPath = Path.Combine(Path.GetDirectoryName(filepath), "data.xml");
        if (File.Exists(dataPath))
        {
            XmlDocument document = new XmlDocument();
            document.Load(dataPath);
            towerSettings.SetData(document["data"]);
        }

        SetLevel(null);
    }

    private void New()
    {
        newTower.Enabled = true;
    }

    private void Open() 
    {
        FileDialog.OpenFile((filepath) => {
            OpenTowerFile(filepath);
            saveState.AddToRecent(filepath);
            SaveIO.SaveJson<SaveState>("towersave.json", saveState, SaveStateContext.Default.SaveState);
            recentItems.Add(new MenuItem(filepath, () => OpenTowerFile(filepath)));
        }, null, new Filter("Tower Xml File", "xml"));
    }

    public void Save(bool specifyPath = false) 
    {
        XmlDocument document = new XmlDocument();
        string solid = currentLevel.Solids.Save();
        string bg = currentLevel.BGs.Save();
        var rootElement = document.CreateElement("level");
        rootElement.SetAttribute("width", WorldUtils.WorldWidth.ToString());
        rootElement.SetAttribute("height", WorldUtils.WorldHeight.ToString());
        document.AppendChild(rootElement);

        var Solids = document.CreateElement("Solids");
        Solids.SetAttribute("exportMode", "Bitstring");
        Solids.InnerText = solid;

        var BG = document.CreateElement("BG");
        BG.SetAttribute("exportMode", "Bitstring");
        BG.InnerText = bg;

        var SolidTiles = document.CreateElement("SolidTiles");
        SolidTiles.SetAttribute("exportMode", "TrimmedCSV");
        SolidTiles.InnerText = currentLevel.SolidTiles.Save();

        var BGTiles = document.CreateElement("BGTiles");
        BGTiles.SetAttribute("exportMode", "TrimmedCSV");
        BGTiles.InnerText = currentLevel.BGTiles.Save();

        var Entities = document.CreateElement("Entities");

        foreach (var entity in currentLevel.Actors) 
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
            if (actorInfo.Nodes != null) 
            {
                foreach (var node in actorInfo.Nodes) 
                {
                    var xmlNode = document.CreateElement("node");
                    xmlNode.SetAttribute("x", node.X.ToString());
                    xmlNode.SetAttribute("y", node.Y.ToString());
                    element.AppendChild(xmlNode);
                }
            }
            Entities.AppendChild(element);
        }

        rootElement.AppendChild(Solids);
        rootElement.AppendChild(BG);
        rootElement.AppendChild(SolidTiles);
        rootElement.AppendChild(BGTiles);
        rootElement.AppendChild(Entities);

        if (specifyPath) 
        {
            FileDialog.Save((filepath) => {
                document.Save(filepath);
                currentLevel.Unsaved = false;
            }, null, new Filter("Tower Xml", "xml"));
        }
        else 
        {
            document.Save(currentLevel.Path);
            currentLevel.Unsaved = false;
        }

    }

    public override void End() 
    {
        imGui.Destroy();
        target.Dispose();
    }
}