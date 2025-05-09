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
using Riateu.IO;
using SDL3;

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

[Flags]
public enum ToolModifierFlags 
{
    None,
    Symmetry
}

public class EditorScene : Scene
{
    private ImGuiRenderer imGui;

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
    private EditorWindow editorWindow;
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
    private TileGridMover tileGridMover;
    private bool hasSelection;
    private bool[] visibility = [true, true, true, true, true];
    private HashSet<ScreenWrapError> wrapErrors = new HashSet<ScreenWrapError>();
    private Tool toolSelected;

    public Tool ToolSelected 
    {
        get => toolSelected;
        set 
        {
            tileGridMover.Reset();
            toolSelected = value;
        }
    }
    public ToolModifierFlags ToolModifier;
    public Layers CurrentLayer = Layers.Solids;
    public bool HasRemovedEntity;

    public PhantomActor PhantomActor;
#endregion

    private BackdropRenderer backdropRenderer;

    private EntityMenu entityMenu;
    private bool showGrid = true;
    private bool openFallbackTheme = false;
    private int fallbackSelected = 0;
    private SaveState saveState;
    private MenuSelectionItem recentItems;
    private ExportOption exportOption;

    public EditorScene(GameApp game, ImGuiRenderer renderer, SaveState state) : base(game)
    {
        saveState = state;
        target = new RenderTarget(game.GraphicsDevice, (uint)WorldUtils.WorldWidth + 100, (uint)WorldUtils.WorldHeight);

        levelBatch = new Batch(game.GraphicsDevice, (int)WorldUtils.WorldWidth + 100, (int)WorldUtils.WorldHeight);
        batch = new Batch(game.GraphicsDevice, 1280, 640);
        PhantomActor = new PhantomActor(this);
        tileRect = new TileRect();
        tileGridMover = new TileGridMover();
        VanillaActor.Init(saveState);
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
                .Add(new MenuItem("Export As .tower", () => {
                    if (tower == null)
                    {
                        return;
                    }

                    exportOption.SetTower(tower);
                    exportOption.Enabled = true;
                }))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuSlot("Settings")
                .Add(new MenuItem("Theme", OnOpenTheme))
                .Add(new MenuItem("Editor")))
            .Add(new MenuSlot("View")
                .Add(new MenuItem("Toggle Grid", showGrid, toggle => showGrid = toggle))
            );

        levelSelection = new LevelSelection();
        levelSelection.OnSelect = OnLevelSelected;
        levelSelection.OnCreated = OnLevelCreated;

        layers = new LayersPanel();
        layers.OnLayerSelect = OnLayerSelected;
        layers.ShowOrHide = OnShowOrHideLayer;

        entities = new Entities(imGuiTexture);
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
        towerSettings.OnSave = OnSettingsApply;
        towerSettings.SetTheme(theme);

        tools = new Tools();

        tools.AddTool(FA6.Pen, () => ToolSelected = Tool.Pen, Tools.ToolType.Selectable);
        tools.AddTool(FA6.UpDownLeftRight, () => ToolSelected = Tool.Rect, Tools.ToolType.Selectable);
        tools.AddTool(FA6.CircleNodes, () => ToolSelected = Tool.Node, Tools.ToolType.Selectable);

        tools.AddTool(FA6.LeftRight, OnHorizontalSymmetry, Tools.ToolType.FireAndForget, 1);
        tools.AddTool(FA6.UpDown, OnVerticalSymmetry, Tools.ToolType.FireAndForget, 1);
        tools.AddTool(FA6.PenRuler, (toggle) => ToolModifier ^= ToolModifierFlags.Symmetry, Tools.ToolType.Toggleable, 2);

        entityMenu = new EntityMenu(imGuiTexture);
        entityMenu.OnSelectActor = OnSelectActor;
        entityMenu.Enabled = false;

        newTower = new NewTower(saveState);
        newTower.OnCreateTower = OnCreateTower;
        newTower.Enabled = false;

        exportOption = new ExportOption();
        exportOption.OnExport = OnExport;
        exportOption.Enabled = false;

        editorWindow = new EditorWindow(renderer, target);
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

        foreach (var level in tower.Levels) 
        {
            var solidTileData = Resource.TilesetData.Tilesets[theme.Tileset];
            var bgTileData = Resource.TilesetData.Tilesets[theme.BGTileset];

            SolidAutotiler.Init(solidTileData);
            solidTilesPanel.SetTheme(solidTileData);
            if (level.SolidTiles != null)
            {
                level.SolidTiles.SetTheme(solidTileData);
                level.SolidTiles.UpdateTiles();
                level.Solids.SetTheme(solidTileData);
                level.Solids.UpdateTiles(SolidAutotiler);
            }

            BgAutotiler.Init(bgTileData);
            bgTilesPanel.SetTheme(bgTileData);
            if (level.BGTiles != null)
            {
                level.BGTiles.SetTheme(bgTileData);
                level.BGTiles.UpdateTiles();
                level.BGs.SetTheme(bgTileData);
                level.BGs.UpdateTiles(BgAutotiler, level.Solids.Bits);
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
    private void OnExport(ExportOverride exportOverride)
    {
        FileDialog.Save((s) => {
            LevelExport.TowerExport(tower, s, exportOverride);
            if (currentLevel != null)
            {
                currentLevel.Unsaved = false;
                SetLevel(currentLevel);
            }
        }, null, new Property("Export as a Workshop Tower", null, new DialogFilter("Tower", "tower")));
    }
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

    private void OnSettingsApply(TowerData data)
    {
        SetTowerTheme(Themes.ThemeNames[data.ThemeID]);
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

        level.ClearIDs();
        

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

#if !DEBUG
        try 
        {
#endif
        level.LoadLevel(tower.Theme);
        level.SetActorScene(this);
        currentLevel = level;
        editorWindow.Title = level.FileName;

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
        CommitHistory();
        actorToRemove = actor;
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
        VerifySolids();
    }

    private void VerifySolids()
    {
        wrapErrors.Clear();
        var bits = currentLevel.Solids.Bits;
        // Top-Down
        for (int x = 0; x < WorldUtils.WorldWidth / 10; x++)
        {
            if (bits[x, 0])
            {
                // Check the downside
                if (bits[x, ((int)WorldUtils.WorldHeight / 10) - 1])
                {
                    continue;
                }
                wrapErrors.Add(new ScreenWrapError(x, ((int)WorldUtils.WorldHeight / 10) - 1));
            }

            if (bits[x, ((int)WorldUtils.WorldHeight / 10) - 1])
            {
                // Check the upside
                if (bits[x, 0])
                {
                    continue;
                }
                wrapErrors.Add(new ScreenWrapError(x, 0));
            }
        }

        // Left-Right
        for (int y = 0; y < WorldUtils.WorldHeight / 10; y++)
        {
            if (bits[0, y])
            {
                // Check the leftside
                if (bits[((int)WorldUtils.WorldWidth / 10) - 1, y])
                {
                    continue;
                }
                wrapErrors.Add(new ScreenWrapError(((int)WorldUtils.WorldWidth / 10) - 1, y));
            }

            if (bits[((int)WorldUtils.WorldWidth / 10) - 1, y])
            {
                // Check the rightside
                if (bits[0, y])
                {
                    continue;
                }
                wrapErrors.Add(new ScreenWrapError(0, y));
            }
        }
    }

    public void CommitHistory(bool shouldClearRedo = true) 
    {
        var historyCommit = CurrentLayer switch {
            Layers.Solids => new History.Commit { Solids = currentLevel.Solids.Bits },
            Layers.BG => new History.Commit { BGs = currentLevel.BGs.Bits },
            Layers.BGTiles => new History.Commit { BGTiles = currentLevel.BGTiles.Ids },
            Layers.SolidTiles => new History.Commit { SolidTiles = currentLevel.SolidTiles.Ids },
            Layers.Entities => new History.Commit { Actors = currentLevel.Actors, CurrentSelectedActor = currentSelected },
            _ => throw new InvalidOperationException()
        };

        currentLevel.PushCommit(historyCommit, CurrentLayer, shouldClearRedo);
    }

    public void PullHistory() 
    {
        currentLevel.PushRedoCommit(new History.Commit {
            Solids = currentLevel.Solids.Bits,
            BGs = currentLevel.BGs.Bits,
            BGTiles = currentLevel.BGTiles.Ids,
            SolidTiles = currentLevel.SolidTiles.Ids,
            Actors = currentLevel.Actors,
            CurrentSelectedActor = currentSelected
        }, CurrentLayer);
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
                currentLevel.RetriveID(actorToRemove.ID);
                actorToRemove = null;
            }
        }
        
        solidTilesPanel.Update();
        bgTilesPanel.Update();

        if (CurrentLayer == Layers.Entities && Input.Keyboard.IsPressed(KeyCode.E) && !ImGui.GetIO().WantTextInput)
        {
            entityMenu.Enabled = !entityMenu.Enabled;
        }

        if (currentLevel == null || openFallbackTheme)
        {
            return;
        }

        HasRemovedEntity = false;

        if (hasSelection) 
        {
            UpdateSelected();
        }

        var io = ImGui.GetIO();

        bool leftMouseDown = io.MouseDown[0];
        bool leftMouseUp = !leftMouseDown;
        bool rightMouseDown = io.MouseDown[2];
        bool rightMouseUp = !rightMouseDown;
        bool leftMouseReleased = io.MouseReleased[0];
        bool rightMouseReleased = io.MouseReleased[2];

        if (isDrawing && (leftMouseUp && rightMouseUp)) 
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
                bool isRedo = Input.Keyboard.IsHeld(KeyCode.LeftShift);
                if (currentLevel.PopCommit(out var res, isRedo))
                {
                    if (!isRedo)
                    {
                        PullHistory();
                    }
                    else
                    {
                        CommitHistory(false);
                    }
                    switch (res.Layer) 
                    {
                    case Layers.Solids:
                        currentLevel.Solids.Clear();
                        currentLevel.Solids.Bits = res.Solids.Clone();
                        currentLevel.Solids.UpdateTiles(SolidAutotiler);
                        VerifySolids();
                        break;
                    case Layers.BG:
                        currentLevel.BGs.Clear();
                        currentLevel.BGs.Bits = res.BGs.Clone();
                        currentLevel.BGs.UpdateTiles(BgAutotiler, currentLevel.Solids.Bits);
                        VerifySolids();
                        break;
                    case Layers.BGTiles:
                        currentLevel.BGTiles.Clear();
                        currentLevel.BGTiles.SetTiles(res.BGTiles);
                        break;
                    case Layers.SolidTiles:
                        currentLevel.SolidTiles.Clear();
                        currentLevel.SolidTiles.SetTiles(res.SolidTiles);
                        break;
                    case Layers.Entities:
                        currentLevel.Actors.Clear();
                        currentLevel.Actors = res.Actors;
                        currentSelected = res.CurrentSelectedActor;
                        break;
                    }
                }
            }
        }

        int x = (int)io.MousePos.X;
        int y = (int)io.MousePos.Y;

        if (tileRect.Started) 
        {
            if (leftMouseUp && rightMouseUp)
            {
                TileMouseReleased();
            }
            else 
            {
                tileRect.Update(x, y);
            }
        }

        if (tileGridMover.Started)
        {
            if (leftMouseUp && rightMouseUp)
            {
                tileGridMover.Started = false;
                var rect = tileGridMover.CalculateResult();

                if (CurrentLayer is Layers.Solids or Layers.BG)
                {
                    Span<bool> data = stackalloc bool[(rect.Width /10) * (rect.Height / 10)];

                    var rectangle = tileRect.ResultRect;
                    for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
                    {
                        for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
                        {
                            int gx = WorldUtils.ToGrid(rectangle.X) + dx;
                            int gy = WorldUtils.ToGrid(rectangle.Y) + dy;

                            GridTiles gridTiles = CurrentLayer switch {
                                Layers.Solids => currentLevel.Solids,
                                Layers.BG => currentLevel.BGs,
                                _ => throw new NotImplementedException()
                            };


                            data[dx * (rectangle.Height / 10) + dy] = gridTiles.Bits[gx, gy];
                        }
                    }

                    CommitHistory();

                    PlaceGridBatch(tileRect.ResultRect, false);
                    PlaceGridBatch(data, rect);
                }
                else if (CurrentLayer is Layers.SolidTiles or Layers.BGTiles)
                {
                    Span<int> data = stackalloc int[(rect.Width /10) * (rect.Height / 10)];

                    Tiles tiles = CurrentLayer switch {
                        Layers.SolidTiles => currentLevel.SolidTiles,
                        Layers.BGTiles => currentLevel.BGTiles,
                        _ => throw new NotImplementedException()
                    };

                    var rectangle = tileRect.ResultRect;
                    for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
                    {
                        for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
                        {
                            int gx = WorldUtils.ToGrid(rectangle.X) + dx;
                            int gy = WorldUtils.ToGrid(rectangle.Y) + dy;

                            data[dx * (rectangle.Height / 10) + dy] = tiles.Ids[gx, gy];
                        }
                    }

                    CommitHistory();
                    PlaceDecorBatch(data, tileRect.ResultRect, tiles, false);
                    PlaceDecorBatch(data, rect, tiles, true);
                }


                tileRect.ResultRect = rect;
                tileGridMover.SetRect(rect);
            }
            else 
            {
                tileGridMover.Update(x, y);
            }
        }

        if (!editorWindow.IsItemHovered) 
        {
            return;
        }

        // Try to not update when it is not visible
        if (!visibility[(int)CurrentLayer]) 
        {
            return;
        }

        int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
        int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));


        int worldPosX = (int)(Math.Floor(((x - WorldUtils.WorldX) / WorldUtils.WorldSize) / 5.0f) * 5.0f);
        int worldPosY = (int)(Math.Floor(((y - WorldUtils.WorldY) / WorldUtils.WorldSize) / 5.0f) * 5.0f);

        bool tileGridHovered = tileGridMover.IsHovered(worldPosX, worldPosY);

        if (tileGridHovered && leftMouseDown)
        {
            int posX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
            int posY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
            tileGridMover.Start(new Vector2(posX, posY));
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
                        void Spawn(Vector2 position)
                        {
                            CommitHistory();
                            ulong id = currentLevel.GetID();
                            var actor = new LevelActor(Resource.TowerFallTexture, actorSelected, id);
                            actor.Scene = this;
                            actor.PosX = position.X;
                            actor.PosY = position.Y;
                            currentLevel.AddActor(actor);
                            if (actor != null) 
                            {
                                Select(actor);
                            }
                            ToolSelected = Tool.Rect;
                        }

                        if (WorldUtils.InBounds(gridX, gridY) && actorSelected != null) 
                        {
                            Spawn(PhantomActor.Position);
                            if (ToolModifier == ToolModifierFlags.Symmetry)
                            {
                                var opposite = WorldUtils.Opposite(PhantomActor.Position);
                                opposite -= new Vector2(PhantomActor.Width, 0);
                                opposite += new Vector2(PhantomActor.OriginX + PhantomActor.OriginX, 0);
                                Spawn(opposite);
                            }
                        }

                    }
                    break;
                }
            }
            else if (ToolSelected == Tool.Node && currentSelected != null && currentSelected.Data.HasNodes) 
            {
                if (WorldUtils.InBounds(gridX, gridY)) 
                {
                    Vector2 position = new Vector2(worldPosX, worldPosY);
                    currentSelected.AddNode(position);
                    currentLevel.Unsaved = true;
                }
            }
        }

        if (!tileRect.Started) 
        {
            if (leftMouseDown) 
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
                else if (ToolSelected == Tool.Rect && CurrentLayer != Layers.Entities && !tileGridMover.Started)
                {
                    int posX = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                    int posY = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
                    
                    tileRect.Start(posX, posY, TileRect.Type.Move);
                }
            }
            else if (rightMouseDown) 
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
    }

    private void TileMouseReleased() 
    {
        tileRect.Started = false;


        TileRect.Type type = tileRect.ButtonType;

        if (type != TileRect.Type.Move)
        {
            CommitHistory();
        }

        var x = tileRect.ResultRect.X;
        var y = tileRect.ResultRect.Y;
        var width = tileRect.ResultRect.Width;
        var height = tileRect.ResultRect.Height;

        switch (type)
        {
        case TileRect.Type.Place:
            PlaceGridBatch(tileRect.ResultRect, true);
            break;
        case TileRect.Type.Remove:
            PlaceGridBatch(tileRect.ResultRect, false);
            break;
        case TileRect.Type.Move:
            tileGridMover.SetRect(tileRect.ResultRect);
            break;
        }
    }

    private void PlaceDecorBatch(Span<int> grids, Rectangle rectangle, Tiles tiles, bool placeTile)
    {
        int x = rectangle.X;
        int y = rectangle.Y;
        for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
        {
            for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
            {
                int gridX = WorldUtils.ToGrid(x) + dx;
                int gridY = WorldUtils.ToGrid(y) + dy;

                if (!WorldUtils.InBounds(gridX, gridY))
                {
                    continue;
                }

                int tile = placeTile ? grids[dx * (rectangle.Height / 10) + dy] : -1;
                tiles.SetTile(gridX, gridY, tile);
                isDrawing = true;
            }
        }
    }

    private void PlaceGridBatch(Span<bool> grids, Rectangle rectangle)
    {
        int x = rectangle.X;
        int y = rectangle.Y;
        for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
        {
            for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
            {
                int gridX = WorldUtils.ToGrid(x) + dx;
                int gridY = WorldUtils.ToGrid(y) + dy;

                if (!WorldUtils.InBounds(gridX, gridY))
                {
                    continue;
                }

                PlaceGrid(gridX, gridY, grids[dx * (rectangle.Height / 10) + dy]);
            }
        }
    }

    private void PlaceGridBatch(Rectangle rectangle, bool placeTile)
    {
        int x = rectangle.X;
        int y = rectangle.Y;
        for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
        {
            for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
            {
                int gridX = WorldUtils.ToGrid(x) + dx;
                int gridY = WorldUtils.ToGrid(y) + dy;

                if (!WorldUtils.InBounds(gridX, gridY))
                {
                    continue;
                }

                PlaceGrid(gridX, gridY, placeTile);

                if (ToolModifier == ToolModifierFlags.Symmetry)
                {
                    Point opposite = WorldUtils.Opposite(gridX, gridY);
                    PlaceGrid(opposite.X - 1, opposite.Y, placeTile);
                }
            }
        }
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

                    if (ToolModifier == ToolModifierFlags.Symmetry)
                    {
                        Point opposite = WorldUtils.Opposite(gridX, gridY);
                        PlaceGrid(opposite.X - 1, opposite.Y, placeTile);
                    }
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
        SetupDockspace();

        if (towerSettings.Enabled) 
        {
            ImGui.OpenPopup("Theme Settings");
        }

        if (newTower.Enabled) 
        {
            ImGui.OpenPopup("New Tower");
        }

        if (exportOption.Enabled)
        {
            ImGui.OpenPopup("Export Option");
        }

        if (openFallbackTheme) 
        {
            ImGui.OpenPopup("Fallback Theme");
        }

        exportOption.DrawGui();
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
        editorWindow.DrawGui(() => {
            tools.UpdateGui();
        });
        ImGui.End();
    }

    private void SetupDockspace()
    {
        var windowFlags = 
            ImGuiWindowFlags.MenuBar
            | ImGuiWindowFlags.NoDocking;
        
        var mainViewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
        ImGui.SetNextWindowPos(mainViewport.Pos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(1280, 640));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        windowFlags |= ImGuiWindowFlags.NoTitleBar 
            | ImGuiWindowFlags.NoCollapse 
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus;

        bool dockSpaceTrue = true;
        ImGui.Begin("Dockspace", ref dockSpaceTrue, windowFlags); 
        ImGui.PopStyleVar(2);

        // Dockspace
        ImGuiIOPtr ioPtr = ImGui.GetIO();

        bool needRebuild = false;

        if ((ioPtr.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0) 
        {
            var dockspaceID = ImGui.GetID("MyDockSpace");
            needRebuild = DockNative.igDockBuilderGetNode(dockspaceID) == IntPtr.Zero;
            ImGui.DockSpace(dockspaceID, System.Numerics.Vector2.Zero);
        }

        if (needRebuild)
        {
            Vector2 workCenter = ImGui.GetMainViewport().GetWorkCenter();
            uint id = ImGui.GetID("MyDockSpace");
            DockNative.igDockBuilderRemoveNode(id);
            DockNative.igDockBuilderAddNode(id);

            Vector2 size = new Vector2(1280, 640);
            Vector2 nodePos = new Vector2(workCenter.X - size.X * 0.5f, workCenter.Y - size.Y * 0.5f);

            DockNative.igDockBuilderSetNodeSize(id, size);
            DockNative.igDockBuilderSetNodePos(id, nodePos);

            uint dock1 = DockNative.igDockBuilderSplitNode(id, ImGuiDir.Left, 0.15f, out _, out id);
            uint dock2 = DockNative.igDockBuilderSplitNode(id, ImGuiDir.Right, 0.2f, out _, out id);
            uint dock3 = DockNative.igDockBuilderSplitNode(id, ImGuiDir.Left, 0.5f, out _, out id);

            DockNative.igDockBuilderDockWindow("Levels", dock1);
            DockNative.igDockBuilderDockWindow("###Editor Viewport", dock3);
            DockNative.igDockBuilderDockWindow("Layers", dock2);

            DockNative.igDockBuilderFinish(id);
        }
    }

    public override void Render(CommandBuffer commandBuffer, RenderTarget swapchain)
    {
        imGui.Update(GameInstance.InputDevice, ImGuiCallback);

        {
            levelBatch.Begin(Resource.BGAtlasTexture, DrawSampler.PointClamp);
            backdropRenderer.Draw(levelBatch);
            levelBatch.End();

            levelBatch.Begin(Resource.TowerFallTexture, DrawSampler.PointClamp);

            if (currentLevel != null) 
            {
                if (visibility[1])
                {
                    currentLevel.BGs.Draw(levelBatch);
                }

                if (visibility[0]) 
                {
                    currentLevel.Solids.Draw(levelBatch);
                    foreach (var error in wrapErrors)
                    {
                        DrawUtils.Rect(
                            levelBatch, 
                            new Vector2(error.X * 10, error.Y * 10), 
                            Color.Red, 
                            new Vector2(10, 10), 
                            Vector2.Zero
                        );
                    }
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
            tileGridMover.Draw(levelBatch, currentLevel, CurrentLayer);
            PhantomActor.Draw(levelBatch);

            if (showGrid)
            {
                DrawGrid();
            }

            levelBatch.End();

            levelBatch.Flush(commandBuffer);

            var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(target, Color.Black, true));
            renderPass.BindGraphicsPipeline(GameContext.BatchMaterial.ShaderPipeline);
            levelBatch.Render(renderPass);
            commandBuffer.EndRenderPass(renderPass);
        }
        
        {
            var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(swapchain, Color.Black, true));
            renderPass.BindGraphicsPipeline(GameContext.BatchMaterial.ShaderPipeline);
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
            towerSettings.SetTower(tower);
            SetTheme(tower.Theme);
        }
        else 
        {
            levelSelection.SelectTower(tower);
            towerSettings.SetTower(tower);
            openFallbackTheme = true;
        }
        var dataPath = Path.Combine(Path.GetDirectoryName(filepath), "data.xml");
        if (File.Exists(dataPath))
        {
            XmlDocument document = new XmlDocument();
            document.Load(dataPath);
            towerSettings.SetData(document["data"]);
        }

        currentLevel = null;
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
        }, null, new Property("Open a Tower", null, new DialogFilter("Tower Xml File", "xml")));
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
            }, null, new Property() 
                {
                    Filter = new DialogFilter("Tower Xml", "xml")
                } 
            );
        }
        else 
        {
            document.Save(currentLevel.Path);
            currentLevel.Unsaved = false;
        }

        if (tower.Type == Tower.TowerType.Quest)
        {
            towerSettings.SetAllGroupDirty();
        }
    }

    public override void End() 
    {
        imGui.Destroy();
        target.Dispose();
    }

    private record struct ScreenWrapError(int X, int Y);
}