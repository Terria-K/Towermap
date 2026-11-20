using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Numerics;
using Cosmiria;
using Riateu;
using Riateu.Graphics;
using Riateu.Inputs;
using Riateu.IO;

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

public partial class EditorScene : Scene
{
    private readonly TowermapGame game;
    private readonly SaveState saveState;

    public readonly BindableValue<bool> NewLevelOpen = new(false);
    public readonly BindableValue<bool> NewTowerOpen = new(false);
    public readonly BindableValue<bool> ExportOpen = new(false);
    public readonly BindableValue<bool> SettingsOpen = new(false);
    public readonly BindableValue<bool> EntityMenuOpen = new(false);
    public readonly BindableValue<bool> FallbackOpen = new(false);
    public readonly bool[] Visibility = [true, true, true, true, true];
    public readonly QuestData Quest = new();
    public readonly ElementRef ViewportRef = new();
    public bool ViewportHovered;
    public bool ShowGrid = true;
    public TileBrush SolidBrush { get; private set; }
    public TileBrush BgBrush { get; private set; }
    public List<LevelActor> Conflicts { get; private set; }

    private Autotiler SolidAutotiler;
    private Autotiler BgAutotiler;
    private readonly BackdropRenderer backdropRenderer;

    public Actor ActorSelected => actorSelected;
    public LevelActor SelectedActor => currentSelected;
    public Tower Tower => tower;
    public Level CurrentLevel => currentLevel;
    public TowermapGame Game => game;
    public SaveState SaveState => saveState;

    private Actor actorSelected;
    private bool isDrawing;
    private bool pressedInViewport;
    private readonly ActorResizer resizer = new();
    private GridEdit gridEdit;
    private TileEdit tileEdit;

    private Tower tower;
    private LevelActor actorToRemove;
    private LevelActor currentSelected;
    private readonly List<LevelActor> listSelected = [];
    private Level currentLevel;
    private readonly TileRect tileRect = new();
    private readonly TileSelection selection = new();
    private bool hasSelection;
    private readonly HashSet<ScreenWrapError> wrapErrors = [];
    private Tool toolSelected;
    private RenderTarget canvas;
    private Batch canvasBatch;

    public UIContext UI => game.UI;

    public Tool ToolSelected
    {
        get => toolSelected;
        set
        {
            ClearSelection();
            toolSelected = value;
            UI.Notify();
        }
    }
    public ToolModifierFlags ToolModifier;
    public Layers CurrentLayer = Layers.Solids;
    public bool HasRemovedEntity;
    public PhantomActor PhantomActor;

    public EditorScene(TowermapGame game, SaveState state) : base(game)
    {
        this.game = game;
        saveState = state;
        PhantomActor = new PhantomActor(this);
        VanillaActor.Init(saveState);

        Themes.TryGetTheme("Flight", out var theme);
        backdropRenderer = new BackdropRenderer();
        backdropRenderer.SetTheme(theme);
    }

    public override void Begin()
    {
        SolidBrush = new TileBrush("tilesets/flight");
        BgBrush = new TileBrush("tilesets/flightBG");
        SolidAutotiler = new Autotiler();
        BgAutotiler = new Autotiler();
    }

    public void SetTowerTheme(string name)
    {
        if (!Themes.TryGetTheme(name, out var theme))
        {
            FallbackOpen.Value = true;
            return;
        }
        tower.SetTheme(theme);
        SetTheme(theme);
    }

    private void SetTheme(Theme theme)
    {
        backdropRenderer.SetTheme(theme);

        var solidTileData = Resource.TilesetData.Tilesets[theme.Tileset];
        var bgTileData = Resource.TilesetData.Tilesets[theme.BGTileset];
        SolidAutotiler.Init(solidTileData);
        BgAutotiler.Init(bgTileData);
        SolidBrush.SetTheme(solidTileData);
        BgBrush.SetTheme(bgTileData);

        foreach (var level in tower.Levels)
        {
            if (level.SolidTiles != null)
            {
                level.SolidTiles.SetTheme(solidTileData);
                level.SolidTiles.UpdateTiles();
                level.Solids.SetTheme(solidTileData);
                level.Solids.UpdateTiles(SolidAutotiler);
            }

            if (level.BGTiles != null)
            {
                level.BGTiles.SetTheme(bgTileData);
                level.BGTiles.UpdateTiles();
                level.BGs.SetTheme(bgTileData);
                level.BGs.UpdateTiles(BgAutotiler, level.Solids.Bits);
            }
        }
        UI.Notify();
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
        Conflicts = null;
        if (currentSelected != null)
        {
            currentSelected.Selected = false;
        }
        currentSelected = actor;
        currentSelected.Selected = true;
        UI.Notify();
    }

    public void UpdateSelected()
    {
        if (listSelected.Count == 1)
        {
            Select(listSelected[0]);
            listSelected.Clear();
        }
        else
        {
            Conflicts = [.. listSelected];
            UI.Notify();
        }

        hasSelection = false;
    }

    public void OnExport(ExportOverride exportOverride)
    {
        FileDialog.Save(path => UI.Post(() => {
            LevelExport.TowerExport(tower, path, exportOverride);
            if (currentLevel != null)
            {
                currentLevel.Unsaved = false;
                SetLevel(currentLevel);
            }
        }), null, new Property("Export as a Workshop Tower", null, new DialogFilter("Tower", "tower")));
    }

    public void CreateLevel(NewLevel level)
    {
        tower.CreateLevel(level.Name, level.Width);
        OnLevelCreated();
    }

    public void RunTool(ToolAction action)
    {
        switch (action)
        {
        case ToolAction.Pen:
            ToolSelected = Tool.Pen;
            break;
        case ToolAction.Rect:
            ToolSelected = Tool.Rect;
            break;
        case ToolAction.Node:
            ToolSelected = Tool.Node;
            break;
        case ToolAction.MirrorHorizontal:
            OnHorizontalSymmetry();
            break;
        case ToolAction.MirrorVertical:
            OnVerticalSymmetry();
            break;
        case ToolAction.Symmetry:
            ToolModifier ^= ToolModifierFlags.Symmetry;
            UI.Notify();
            break;
        }
    }

    public void SaveTowerSettings(TowerData data)
    {
        SetTowerTheme(Themes.ThemeNames[data.ThemeID]);
        tower.ArrowRates = data.ArrowRates;
        tower.Save();
    }

    public void SaveQuest() => Quest.Save(tower.TowerPath);

    public void ApplyFallbackTheme(int index)
    {
        Themes.TryGetTheme(Themes.ThemeNames[index], out var theme);
        tower.SetTheme(theme);
        SetTheme(theme);
        FallbackOpen.Value = false;
    }

    public void ShowExport()
    {
        if (tower != null)
        {
            ExportOpen.Value = true;
        }
    }

    public void RecordChange(ActorChange change) => RecordActor(change.Actor, change.Before, change.MergeKey);

    public void ToggleVisibility(Layers layer)
    {
        Visibility[(int)layer] = !Visibility[(int)layer];
        UI.Notify();
    }

    public void SelectBrush(Rectangle rect)
    {
        (CurrentLayer == Layers.SolidTiles ? SolidBrush : BgBrush).Selection = rect;
        UI.Notify();
    }

    private void OnLevelCreated()
    {
        if (currentLevel != null)
        {
            Save();
        }
        SetTheme(tower.Theme);
    }

    public void OnCreateTower(CreateNewTowerCallback info)
    {
        var directory = Path.Combine(info.TowerPath, info.TowerName);
        Directory.CreateDirectory(directory);

        XmlDocument document = new XmlDocument();
        var rootElement = document.CreateElement("tower");
        document.AppendChild(rootElement);

        var theme = document.CreateElement("theme");
        theme.InnerText = Themes.ThemeNames[info.TowerTheme];
        rootElement.AppendChild(theme);

        switch (info.TowerMode)
        {
        case 0:
            var treasure = document.CreateElement("treasure");
            treasure.InnerText = "Arrows,Shield,Wings,TimeOrb,DarkOrb,LavaOrb";
            rootElement.AppendChild(treasure);
            break;
        case 1:
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
        }

        var filepath = Path.Combine(directory, "tower.xml");
        document.Save(filepath);
        OpenTowerFile(filepath);
        AddToRecent(filepath);
    }

    public void OnSelectActor(Actor actor)
    {
        actorSelected = actor;
        PhantomActor.SetActor(actorSelected);
        ToolSelected = Tool.Pen;
    }

    private (GridTiles Tiles, Autotiler Autotiler) CurrentGrid() => CurrentLayer switch
    {
        Layers.Solids => (currentLevel.Solids, SolidAutotiler),
        Layers.BG => (currentLevel.BGs, BgAutotiler),
        _ => (null, null)
    };

    public void OnVerticalSymmetry()
    {
        var (grid, _) = currentLevel == null ? default : CurrentGrid();
        if (grid == null)
            return;

        int rows = (int)WorldUtils.WorldHeight / 10;
        for (int y = 0; y < rows / 2; y++)
        {
            for (int x = 0; x < WorldUtils.WorldWidth / 10; x++)
            {
                SetCell(grid, x, rows - y - 1, grid.Bits[x, y]);
            }
        }
        FlushEdits();
        RefreshGrids();
    }

    public void OnHorizontalSymmetry()
    {
        var (grid, _) = currentLevel == null ? default : CurrentGrid();
        if (grid == null)
            return;

        int columns = (int)WorldUtils.WorldWidth / 10;
        for (int y = 0; y < WorldUtils.WorldHeight / 10; y++)
        {
            for (int x = 0; x < columns / 2; x++)
            {
                SetCell(grid, columns - x - 1, y, grid.Bits[x, y]);
            }
        }
        FlushEdits();
        RefreshGrids();
    }

    public void OnLayerSelected(Layers layer)
    {
        ClearSelection();
        CurrentLayer = layer;
        PhantomActor.Active = layer == Layers.Entities;
    }

    public void SetLevel(Level level)
    {
        if (level == null)
        {
            return;
        }
        ClearSelection();

        level.ClearIDs();
        SolidAutotiler.Seed = level.Seed;
        BgAutotiler.Seed = level.Seed;

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
            UI.Notify();
            return;
        }

#if !DEBUG
        try
        {
#endif
        level.LoadLevel(tower.Theme);
        level.SetActorScene(this);
        currentLevel = level;
#if !DEBUG
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load this level: '{Path.GetFileName(level.Path)}'");
            Logger.Error(ex.ToString());
        }
#endif

        currentLevel.Solids.UpdateTiles(SolidAutotiler);
        currentLevel.BGs.UpdateTiles(BgAutotiler, currentLevel.Solids.Bits);
        VerifySolids();
        UI.Notify();
    }

    public void RemoveActor(LevelActor actor)
    {
        actorToRemove = actor;
    }

    private void UpdateTiles(int gridX, int gridY, Array2D<bool> also)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int x = gridX + dx;
                int y = gridY + dy;
                currentLevel.Solids.UpdateTile(x, y, SolidAutotiler.Tile(currentLevel.Solids.Bits, x, y));
                currentLevel.BGs.UpdateTile(x, y, BgAutotiler.Tile(currentLevel.BGs.Bits, x, y, also));
            }
        }
        VerifySolids();
    }

    private void VerifySolids()
    {
        wrapErrors.Clear();
        var bits = currentLevel.Solids.Bits;
        int right = (int)WorldUtils.WorldWidth / 10 - 1;
        int bottom = (int)WorldUtils.WorldHeight / 10 - 1;

        for (int x = 0; x <= right; x++)
        {
            if (bits[x, 0] != bits[x, bottom])
            {
                wrapErrors.Add(new ScreenWrapError(x, bits[x, 0] ? bottom : 0));
            }
        }

        for (int y = 0; y <= bottom; y++)
        {
            if (bits[0, y] != bits[right, y])
            {
                wrapErrors.Add(new ScreenWrapError(bits[0, y] ? right : 0, y));
            }
        }
    }

    private bool SetCell(GridTiles grid, int x, int y, bool value)
    {
        bool before = grid.Bits[x, y];
        if (!grid.SetGrid(x, y, value))
        {
            return false;
        }
        if (gridEdit?.Grid != grid)
        {
            FlushEdits();
            gridEdit = new GridEdit(grid, CurrentLayer);
        }
        gridEdit.Record(x, y, before, value);
        return true;
    }

    private void SetDecor(Tiles tiles, int x, int y, int tile)
    {
        int before = tiles.Ids[x, y];
        if (before == tile)
        {
            return;
        }
        if (tileEdit?.Tiles != tiles)
        {
            FlushEdits();
            tileEdit = new TileEdit(tiles, CurrentLayer);
        }
        tiles.SetTile(x, y, tile);
        tileEdit.Record(x, y, before, tile);
    }

    private void FlushEdits()
    {
        DropSelection();
        if (gridEdit != null)
        {
            currentLevel?.Record(gridEdit);
            gridEdit = null;
        }
        if (tileEdit != null)
        {
            currentLevel?.Record(tileEdit);
            tileEdit = null;
        }
        UI.Notify();
    }

    public void RecordActor(LevelActor actor, ActorState before, string mergeKey = null)
    {
        var edit = new ActorEdit(actor, before, ActorState.Of(actor), mergeKey);
        if (currentLevel.History.TryMerge(edit))
        {
            currentLevel.Unsaved = true;
        }
        else
        {
            currentLevel.Record(edit);
        }
        UI.Notify();
    }

    private void RefreshGrids()
    {
        currentLevel.Solids.Clear();
        currentLevel.Solids.UpdateTiles(SolidAutotiler);
        currentLevel.BGs.Clear();
        currentLevel.BGs.UpdateTiles(BgAutotiler, currentLevel.Solids.Bits);
        VerifySolids();
    }

    public void Undo(bool redo)
    {
        if (currentLevel != null)
        {
            JumpTo(currentLevel.History.Position + (redo ? 1 : -1));
        }
    }

    public void JumpTo(int position)
    {
        if (currentLevel == null || isDrawing || UI.HasTextFocus)
        {
            return;
        }

        FlushEdits();
        var history = currentLevel.History;
        position = Math.Clamp(position, 0, history.Commands.Count);
        if (position == history.Position)
        {
            return;
        }

        bool grids = false;
        bool entities = false;
        while (history.Position != position)
        {
            bool undoing = position < history.Position;
            var command = undoing ? history.Undo() : history.Redo();
            if (command is SelectionMove move)
            {
                selection.Set(undoing ? move.Before : move.After, move.Layer);
            }
            grids |= command.Layer is Layers.Solids or Layers.BG;
            entities |= command.Layer == Layers.Entities;
        }

        currentLevel.Unsaved = true;
        if (grids)
        {
            RefreshGrids();
        }
        if (entities)
        {
            SyncSelection();
        }
        UI.Notify();
    }

    private void SyncSelection()
    {
        if (currentSelected == null)
        {
            return;
        }
        if (currentLevel.Actors.Contains(currentSelected))
        {
            return;
        }
        currentSelected.Selected = false;
        currentSelected = null;
        Conflicts = null;
    }

    public override void Process(double delta)
    {
        UpdateViewportTransform();

        bool[] visibility = Visibility;
        if (currentLevel != null && visibility[(int)Layers.Entities])
        {
            UpdateResize();
            foreach (var actor in currentLevel.Actors)
            {
                actor.Update(delta);
            }
            if (actorToRemove != null)
            {
                int index = currentLevel.Actors.IndexOf(actorToRemove);
                currentLevel.RemoveActor(actorToRemove);
                currentLevel.Record(new RemoveActor(currentLevel, actorToRemove, index));
                if (actorToRemove == currentSelected)
                {
                    SyncSelection();
                }
                HasRemovedEntity = true;
                currentLevel.RetriveID(actorToRemove.ID);
                actorToRemove = null;
                UI.Notify();
            }
        }

        bool typing = UI.HasTextFocus;
        if (!typing && CurrentLayer == Layers.Entities && Input.Keyboard.IsPressed(KeyCode.E))
        {
            EntityMenuOpen.Value = !EntityMenuOpen.Value;
        }

        if (!typing)
        {
            HandleShortcuts();
        }

        if (currentLevel == null || FallbackOpen.Value)
        {
            return;
        }

        HasRemovedEntity = false;

        if (hasSelection)
        {
            UpdateSelected();
        }

        bool leftDown = Input.Mouse.LeftButton.IsDown;
        bool rightDown = Input.Mouse.RightButton.IsDown;
        bool released = !leftDown && !rightDown;

        if (Input.Mouse.LeftButton.Pressed || Input.Mouse.RightButton.Pressed)
        {
            pressedInViewport = ViewportHovered;
        }
        else if (released)
        {
            pressedInViewport = false;
        }

        if (isDrawing && released)
        {
            isDrawing = false;
            FlushEdits();
        }

        int x = Input.Mouse.X;
        int y = Input.Mouse.Y;

        if (tileRect.Started)
        {
            if (released)
            {
                TileMouseReleased();
            }
            else
            {
                tileRect.Update(x, y);
            }
        }

        if (selection.Dragging)
        {
            if (released)
            {
                selection.EndDrag();
                UI.Notify();
            }
            else
            {
                selection.Drag(WorldUtils.Snap(WorldUtils.ToWorldX(x), 10), WorldUtils.Snap(WorldUtils.ToWorldY(y), 10));
            }
        }

        if (!ViewportHovered || !pressedInViewport && !released || !visibility[(int)CurrentLayer])
        {
            return;
        }

        float worldX = WorldUtils.ToWorldX(x);
        float worldY = WorldUtils.ToWorldY(y);
        int gridX = WorldUtils.ToGrid(worldX);
        int gridY = WorldUtils.ToGrid(worldY);
        int snapX = WorldUtils.Snap(worldX, 10);
        int snapY = WorldUtils.Snap(worldY, 10);

        if (Input.Mouse.LeftButton.Pressed && ToolSelected == Tool.Rect && CurrentLayer != Layers.Entities)
        {
            if (selection.Layer == CurrentLayer && selection.Contains(worldX, worldY))
            {
                if (!selection.Floating)
                {
                    LiftSelection();
                }
                selection.BeginDrag(snapX, snapY);
            }
            else
            {
                ClearSelection();
            }
        }

        PhantomActor.Update(delta);

        if (Input.Mouse.LeftButton.Pressed && WorldUtils.InBounds(gridX, gridY))
        {
            if (ToolSelected == Tool.Pen && CurrentLayer == Layers.Entities && actorSelected != null)
            {
                Spawn(PhantomActor.Position);
                if (ToolModifier == ToolModifierFlags.Symmetry)
                {
                    var opposite = WorldUtils.Opposite(PhantomActor.Position);
                    opposite -= new Vector2(PhantomActor.Width, 0);
                    opposite += new Vector2(PhantomActor.OriginX * 2, 0);
                    Spawn(opposite);
                }
            }
            else if (ToolSelected == Tool.Node && currentSelected != null && currentSelected.Data.HasNodes)
            {
                currentSelected.AddNode(new Vector2(WorldUtils.Snap(worldX, 5), WorldUtils.Snap(worldY, 5)));
            }
        }

        if (tileRect.Started)
        {
            return;
        }

        bool shift = Input.Keyboard.IsHeld(KeyCode.LeftShift);
        if (leftDown)
        {
            if (ToolSelected == Tool.Pen)
            {
                if (shift)
                {
                    tileRect.Start(snapX, snapY, TileRect.Type.Place);
                }
                else
                {
                    Place(gridX, gridY, true);
                }
            }
            else if (ToolSelected == Tool.Rect && CurrentLayer != Layers.Entities && !selection.Dragging)
            {
                tileRect.Start(snapX, snapY, TileRect.Type.Move);
            }
        }
        else if (rightDown)
        {
            if (shift)
            {
                tileRect.Start(snapX, snapY, TileRect.Type.Remove);
            }
            else
            {
                Place(gridX, gridY, false);
            }
        }
    }

    public bool IsResizing => resizer.Active;

    private bool CanResize => CurrentLayer == Layers.Entities && ToolSelected == Tool.Rect
        && currentSelected != null && (currentSelected.Data.ResizeableX || currentSelected.Data.ResizeableY);

    private void UpdateResize()
    {
        var mouse = new Vector2(WorldUtils.ToWorldX(Input.Mouse.X), WorldUtils.ToWorldY(Input.Mouse.Y));
        if (resizer.Active)
        {
            if (Input.Mouse.LeftButton.IsDown)
            {
                resizer.Update(mouse);
            }
            else
            {
                resizer.End(this);
            }
            UI.Notify();
        }
        else if (CanResize && ViewportHovered && Input.Mouse.LeftButton.Pressed)
        {
            resizer.TryStart(currentSelected, mouse);
        }
    }

    private void HandleShortcuts()
    {
        var keyboard = Input.Keyboard;
        bool shift = keyboard.IsDown(KeyCode.LeftShift);
        if (keyboard.IsPressed(KeyCode.D1))
        {
            ToolSelected = Tool.Pen;
        }
        else if (keyboard.IsPressed(KeyCode.D2))
        {
            ToolSelected = Tool.Rect;
        }
        else if (keyboard.IsPressed(KeyCode.Return) || keyboard.IsPressed(KeyCode.Escape))
        {
            ClearSelection();
        }
        else if (keyboard.IsPressed(KeyCode.D5))
        {
            ToolSelected = Tool.Node;
        }
        else if (shift && keyboard.IsPressed(KeyCode.H))
        {
            OnHorizontalSymmetry();
        }
        else if (shift && keyboard.IsPressed(KeyCode.V))
        {
            OnVerticalSymmetry();
        }
    }

    private void Spawn(Vector2 position)
    {
        var actor = new LevelActor(Resource.TowerFallTexture, actorSelected, currentLevel.GetID());
        actor.Scene = this;
        actor.PosX = position.X;
        actor.PosY = position.Y;
        currentLevel.AddActor(actor);
        currentLevel.Record(new AddActor(currentLevel, actor));
        Select(actor);
        ToolSelected = Tool.Rect;
    }

    private void LiftSelection()
    {
        switch (selection.Layer)
        {
        case Layers.Solids:
        case Layers.BG:
            selection.Lift(selection.Layer == Layers.Solids ? currentLevel.Solids : currentLevel.BGs);
            RefreshGrids();
            break;
        case Layers.SolidTiles:
        case Layers.BGTiles:
            selection.Lift(selection.Layer == Layers.SolidTiles ? currentLevel.SolidTiles : currentLevel.BGTiles);
            break;
        }
    }

    private void DropSelection()
    {
        if (!selection.Floating)
        {
            return;
        }
        var move = selection.Drop();
        currentLevel?.Record(move);
        if (move.Layer is Layers.Solids or Layers.BG)
        {
            RefreshGrids();
        }
        UI.Notify();
    }

    private void ClearSelection()
    {
        DropSelection();
        selection.Clear();
    }

    private void TileMouseReleased()
    {
        tileRect.Started = false;
        switch (tileRect.ButtonType)
        {
        case TileRect.Type.Place:
            PlaceGridBatch(tileRect.ResultRect, true);
            break;
        case TileRect.Type.Remove:
            PlaceGridBatch(tileRect.ResultRect, false);
            break;
        case TileRect.Type.Move:
            selection.Set(tileRect.ResultRect, CurrentLayer);
            break;
        }
        FlushEdits();
    }

    private void PlaceGridBatch(Rectangle rectangle, bool placeTile)
    {
        for (int dx = 0; dx < rectangle.Width / 10; dx++)
        {
            for (int dy = 0; dy < rectangle.Height / 10; dy++)
            {
                int gridX = WorldUtils.ToGrid(rectangle.X) + dx;
                int gridY = WorldUtils.ToGrid(rectangle.Y) + dy;
                if (WorldUtils.InBounds(gridX, gridY))
                {
                    PlaceSymmetric(gridX, gridY, placeTile);
                }
            }
        }
    }

    private void PlaceSymmetric(int gridX, int gridY, bool placeTile)
    {
        PlaceGrid(gridX, gridY, placeTile);
        if (ToolModifier == ToolModifierFlags.Symmetry)
        {
            Point opposite = WorldUtils.Opposite(gridX, gridY);
            PlaceGrid(opposite.X - 1, opposite.Y, placeTile);
        }
    }

    private void PlaceGrid(int gridX, int gridY, bool placeTile)
    {
        var (currentTile, autotiler) = CurrentLayer == Layers.Solids
            ? (currentLevel.Solids, SolidAutotiler)
            : (currentLevel.BGs, BgAutotiler);
        Array2D<bool> also = CurrentLayer == Layers.Solids ? null : currentLevel.Solids.Bits;

        if (SetCell(currentTile, gridX, gridY, placeTile))
        {
            currentTile.UpdateTile(gridX, gridY, autotiler.Tile(currentTile.Bits, gridX, gridY, also));
            UpdateTiles(gridX, gridY, currentLevel.Solids.Bits);
        }
        isDrawing = true;
    }

    private void PlaceDecor(int gridX, int gridY, bool placeTile, TileBrush panel, Tiles tiles)
    {
        var data = panel.GetData();
        for (int nx = 0; nx < data.Columns; nx++)
        {
            for (int ny = 0; ny < data.Rows; ny++)
            {
                int relNx = gridX + nx;
                int relNy = gridY + ny;
                if (WorldUtils.InBounds(relNx, relNy))
                {
                    SetDecor(tiles, relNx, relNy, placeTile ? data[ny, nx] : -1);
                }
            }
        }
        isDrawing = true;
    }

    private void Place(int gridX, int gridY, bool placeTile)
    {
        switch (CurrentLayer)
        {
        case Layers.Solids:
        case Layers.BG:
            if (WorldUtils.InBounds(gridX, gridY))
            {
                PlaceSymmetric(gridX, gridY, placeTile);
            }
            break;
        case Layers.BGTiles:
            PlaceDecor(gridX, gridY, placeTile, BgBrush, currentLevel.BGTiles);
            break;
        case Layers.SolidTiles:
            PlaceDecor(gridX, gridY, placeTile, SolidBrush, currentLevel.SolidTiles);
            break;
        }
    }

    public override void Render(CommandBuffer commandBuffer, RenderTarget swapchain)
    {
        var batch = EnsureCanvas();
        bool[] visibility = Visibility;

        batch.Begin(Resource.BGAtlasTexture, DrawSampler.PointClamp);
        backdropRenderer.Draw(batch);
        batch.End();

        batch.Begin(Resource.TowerFallTexture, DrawSampler.PointClamp);

        if (currentLevel != null)
        {
            if (visibility[(int)Layers.BG])
            {
                currentLevel.BGs.Draw(batch);
            }

            if (visibility[(int)Layers.Solids])
            {
                currentLevel.Solids.Draw(batch);
                foreach (var error in wrapErrors)
                {
                    DrawUtils.Rect(batch, new Vector2(error.X * 10, error.Y * 10), Color.Red, new Vector2(10, 10), Vector2.Zero);
                }
            }

            if (visibility[(int)Layers.BGTiles])
            {
                currentLevel.BGTiles.Draw(batch);
            }

            if (visibility[(int)Layers.SolidTiles])
            {
                currentLevel.SolidTiles.Draw(batch);
            }

            if (visibility[(int)Layers.Entities])
            {
                foreach (var actor in currentLevel.Actors)
                {
                    actor.Draw(batch);
                }
            }
        }

        tileRect.Draw(batch);
        if (selection.Layer == CurrentLayer)
        {
            selection.Draw(batch);
        }
        PhantomActor.Draw(batch);
        if (CanResize && visibility[(int)Layers.Entities])
        {
            ActorResizer.Draw(batch, currentSelected);
        }

        if (ShowGrid)
        {
            DrawGrid(batch);
        }

        batch.End();
        batch.Flush(commandBuffer);

        var renderPass = commandBuffer.BeginRenderPass(new ColorTargetInfo(canvas, Color.Black, true));
        renderPass.BindGraphicsPipeline(GameContext.BatchMaterial.ShaderPipeline);
        batch.Render(renderPass);
        commandBuffer.EndRenderPass(renderPass);
    }

    private static void DrawGrid(Batch batch)
    {
        for (int i = 0; i < WorldUtils.WorldWidth / 10; i++)
        {
            DrawUtils.Line(batch, new Vector2(i * 10, 0), new Vector2(i * 10, WorldUtils.WorldHeight), Color.White * 0.1f);
        }

        for (int i = 0; i < WorldUtils.WorldHeight / 10; i++)
        {
            DrawUtils.Line(batch, new Vector2(0, i * 10), new Vector2(WorldUtils.WorldWidth, i * 10), Color.White * 0.1f);
        }
    }

    public void OpenTowerFile(string filepath)
    {
        ClearSelection();
        tower = new Tower();
        bool themeFound = tower.Load(filepath);
        tower.RefreshLevels();
        if (themeFound)
        {
            SetTheme(tower.Theme);
        }
        else
        {
            FallbackOpen.Value = true;
        }

        var dataPath = Path.Combine(Path.GetDirectoryName(filepath), "data.xml");
        if (File.Exists(dataPath))
        {
            XmlDocument document = new XmlDocument();
            document.Load(dataPath);
            Quest.Load(document["data"]);
        }

        currentLevel = null;
        UI.Notify();
    }

    private void AddToRecent(string filepath)
    {
        saveState.AddToRecent(filepath);
        SaveIO.SaveJson<SaveState>("towersave.json", saveState, SaveStateContext.Default.SaveState);
    }

    public void Open()
    {
        FileDialog.OpenFile(filepath => UI.Post(() => {
            OpenTowerFile(filepath);
            AddToRecent(filepath);
        }), null, new Property("Open a Tower", null, new DialogFilter("Tower Xml File", "xml")));
    }

    public void Save(bool specifyPath = false)
    {
        if (currentLevel == null)
        {
            return;
        }
        FlushEdits();

        XmlDocument document = new XmlDocument();
        var rootElement = document.CreateElement("level");
        rootElement.SetAttribute("width", WorldUtils.WorldWidth.ToString());
        rootElement.SetAttribute("height", WorldUtils.WorldHeight.ToString());
        document.AppendChild(rootElement);

        var solids = document.CreateElement("Solids");
        solids.SetAttribute("exportMode", "Bitstring");
        solids.InnerText = currentLevel.Solids.Save();

        var bg = document.CreateElement("BG");
        bg.SetAttribute("exportMode", "Bitstring");
        bg.InnerText = currentLevel.BGs.Save();

        var solidTiles = document.CreateElement("SolidTiles");
        solidTiles.SetAttribute("exportMode", "TrimmedCSV");
        solidTiles.InnerText = currentLevel.SolidTiles.Save();

        var bgTiles = document.CreateElement("BGTiles");
        bgTiles.SetAttribute("exportMode", "TrimmedCSV");
        bgTiles.InnerText = currentLevel.BGTiles.Save();

        var entitiesElement = document.CreateElement("Entities");
        foreach (var actor in currentLevel.Actors)
        {
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
            entitiesElement.AppendChild(element);
        }

        rootElement.AppendChild(solids);
        rootElement.AppendChild(bg);
        rootElement.AppendChild(solidTiles);
        rootElement.AppendChild(bgTiles);
        rootElement.AppendChild(entitiesElement);

        var level = currentLevel;
        if (specifyPath)
        {
            FileDialog.Save(filepath => UI.Post(() => {
                document.Save(filepath);
                level.Unsaved = false;
            }), null, new Property { Filter = new DialogFilter("Tower Xml", "xml") });
        }
        else
        {
            document.Save(level.Path);
            level.Unsaved = false;
        }

        if (tower.Type == Tower.TowerType.Quest)
        {
            Quest.SetAllGroupDirty();
        }
        UI.Notify();
    }

    public override void End()
    {
        canvas?.Dispose();
        canvasBatch?.Dispose();
    }

    private void UpdateViewportTransform()
    {
        var bounds = ViewportRef.Bounds;
        if (bounds.Z <= 0 || bounds.W <= 0)
        {
            return;
        }
        float scale = MathF.Min(bounds.Z / WorldUtils.WorldWidth, bounds.W / WorldUtils.WorldHeight);
        WorldUtils.WorldSize = scale;
        WorldUtils.WorldX = bounds.X + (bounds.Z - WorldUtils.WorldWidth * scale) * 0.5f;
        WorldUtils.WorldY = bounds.Y + (bounds.W - WorldUtils.WorldHeight * scale) * 0.5f;
    }

    public Texture Canvas()
    {
        EnsureCanvas();
        return canvas;
    }

    private Batch EnsureCanvas()
    {
        uint width = (uint)WorldUtils.WorldWidth;
        uint height = (uint)WorldUtils.WorldHeight;
        if (canvas != null && canvas.Width == width && canvas.Height == height)
        {
            return canvasBatch;
        }

        canvas?.Dispose();
        canvasBatch?.Dispose();
        canvas = new RenderTarget(GraphicsDevice, width, height);
        canvasBatch = new Batch(GraphicsDevice, (int)width, (int)height);
        UI.Notify();
        return canvasBatch;
    }

    private record struct ScreenWrapError(int X, int Y);
}
