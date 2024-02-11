using System;
using System.IO;
using System.Xml;
using ImGuiNET;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;
using Riateu.ImGuiRend;

namespace Towermap;

public class EditorScene : Scene
{
    private ImGuiRenderer imGui;

#region Elements
    private ImGuiElement menuBar;
    private LevelSelection levelSelection;
#endregion

    private EditorCanvas mainCanvas;
    private Transform canvasTransform;
    private SolidTiles solids;
    private Spritesheet solidSpriteSheet;
    private XmlElement level;

    public EditorScene(GameApp game) : base(game)
    {
        imGui = new ImGuiRenderer(game.GraphicsDevice, game.MainWindow, 960, 640);
        menuBar = new MenuBar()
            .Add(new MenuSlot("File")
                .Add(new MenuItem("New"))
                .Add(new MenuItem("Open"))
                .Add(new MenuItem("Save"))
                .Add(new MenuItem("Save As"))
                .Add(new MenuItem("Quit", () => GameInstance.Quit())))
            .Add(new MenuItem("Edit"));
        
        levelSelection = new LevelSelection();
        levelSelection.OnSelect = OnLevelSelected;

        mainCanvas = new EditorCanvas(this, game.GraphicsDevice);

        canvasTransform = new Transform();
        canvasTransform.PosX = WorldUtils.WorldX;
        canvasTransform.PosY = WorldUtils.WorldY;
        canvasTransform.Scale = new Vector2(2);
    }

    public override void Begin()
    {
        levelSelection.SelectTower("../Assets");
        solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas["tilesets/flight"], 10, 10);
        solids = new SolidTiles(Resource.TowerFallTexture, solidSpriteSheet);
        Add(solids);

        SetLevel("../Assets/01.oel");
    }


    private void OnLevelSelected(string path)
    {
        SetLevel(path);
    }

    public void SetLevel(string path) 
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);

        solids.Clear();
        var loadingLevel = document["level"];
        try 
        {
            var solidTiles = loadingLevel["Solids"];
            var tiles = solids.GetTiles(solidTiles.InnerText);
            solids.SetTile(tiles);

            level = loadingLevel;
        }
        catch 
        {
            Logger.LogInfo($"Failed to load this level: '{Path.GetFileName(path)}'");
            var solidTiles = level["Solids"];
            var tiles = solids.GetTiles(solidTiles.InnerText);
            solids.SetTile(tiles);
        }
    }

    public override void Update(double delta)
    {
        imGui.Update(GameInstance.Inputs, ImGuiCallback);
        int x = Input.InputSystem.Mouse.X;
        int y = Input.InputSystem.Mouse.Y;

        if (Input.InputSystem.Mouse.LeftButton.IsDown) 
        {
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                solids.SetTile(gridX, gridY, 0);
            }

        }
        else if (Input.InputSystem.Mouse.RightButton.IsDown) 
        {
            int gridX = (int)Math.Floor((x - WorldUtils.WorldX) / (WorldUtils.TileSize * WorldUtils.WorldSize));
            int gridY = (int)Math.Floor((y - WorldUtils.WorldY) / (WorldUtils.TileSize * WorldUtils.WorldSize));

            if (InBounds(gridX, gridY)) 
            {
                solids.SetTile(gridX, gridY, -1);
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
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer, IBatch batch)
    {
        mainCanvas.Draw(buffer, batch);
        batch.Start();
        batch.Add(mainCanvas.CanvasTexture, GameContext.GlobalSampler, Vector2.Zero, 
            Color.White, canvasTransform.WorldMatrix);
        batch.FlushVertex(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, Color.Black));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
        batch.Draw(buffer);

        imGui.Draw(buffer);
        buffer.EndRenderPass();
    }

    public override void End()
    {
    }
}