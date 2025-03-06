using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using Riateu;
using Riateu.Graphics;

namespace Towermap;

public class Level 
{
    public List<LevelActor> Actors { get; set; }
    public string Path { get; private set; }
    public string FileName { get; private set; }
    public bool Unsaved { get; set; }

    public int Seed 
    {
        get 
        {
            var pathSpan = Path.AsSpan();
            int seed = 0;
            for (int i = 0; i < pathSpan.Length; i++) 
            {
                seed += (int)pathSpan[i] + i;
            }

            return seed;
        }
    }


    public GridTiles Solids;
    public GridTiles BGs;
    public Tiles BGTiles;
    public Tiles SolidTiles;
    public int Width;
    public int Height;
    public bool LoadedIn;
    public ulong TotalIDs = 0;
    private Queue<ulong> unusedIds = new();

    private History history = new History();

    public Level(string path) 
    {
        Path = path;
        FileName = System.IO.Path.GetFileName(Path);
        Actors = new List<LevelActor>();
    }

    public void AddActor(LevelActor actor) 
    {
        Unsaved = true;
        Actors.Add(actor);
    }

    public void RemoveActor(LevelActor actor) 
    {
        Unsaved = true;
        Actors.Remove(actor);
    }

    public void PushCommit(History.Commit commit, Layers layer, bool shouldClearRedo = false) 
    {
        Unsaved = true;
        history.PushCommit(commit, layer, false, shouldClearRedo);
    }

    public void PushRedoCommit(History.Commit commit, Layers layer) 
    {
        Unsaved = true;
        history.PushCommit(commit, layer, true, false);
    }

    public bool PopCommit(out History.Commit commit, bool redo) 
    {
        if (history.PopCommit(out commit, redo)) 
        {
            Unsaved = true;
            return true;
        }
        return false;
    }


    public ulong GetID() 
    {
        if (unusedIds.TryDequeue(out ulong result)) 
        {
            return result;
        }
        return TotalIDs++;
    }

    public void RetriveID(ulong id) 
    {
        unusedIds.Enqueue(id);
    }

    public void ClearIDs() 
    {
        TotalIDs = 0;
        unusedIds.Clear();
    }

    public void SetActorScene(Scene scene)
    {
        foreach (var actor in Actors)
        {
            actor.Scene = scene;
        }
    }
    
    public void LoadLevel(Theme theme)
    {
        LoadedIn = true;
        Actors.Clear();
        
        XmlDocument document = new XmlDocument();
        document.Load(Path);

        var loadingLevel = document["level"];

        Width = loadingLevel.AttrInt("width", 320);
        Height = loadingLevel.AttrInt("width", 240);

        if (Width == 420)
        {
            if (!WorldUtils.TurnWide())
            {
                var solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, theme.SolidTilesQuad, 10, 10);
                var bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, theme.BGTilesQuad, 10, 10);
                Solids = new GridTiles(Resource.TowerFallTexture, solidSpriteSheet, 42, 24);

                BGs = new GridTiles(Resource.TowerFallTexture, bgSpriteSheet, 42, 24);
                SolidTiles = new Tiles(Resource.TowerFallTexture, solidSpriteSheet, 42, 24);
                BGTiles = new Tiles(Resource.TowerFallTexture, bgSpriteSheet, 42, 24);
            }
        }
        else 
        {
            WorldUtils.TurnStandard();
            var solidSpriteSheet = new Spritesheet(Resource.TowerFallTexture, theme.SolidTilesQuad, 10, 10);
            var bgSpriteSheet = new Spritesheet(Resource.TowerFallTexture, theme.BGTilesQuad, 10, 10);
            Solids = new GridTiles(Resource.TowerFallTexture, solidSpriteSheet, 32, 24);

            BGs = new GridTiles(Resource.TowerFallTexture, bgSpriteSheet, 32, 24);
            SolidTiles = new Tiles(Resource.TowerFallTexture, solidSpriteSheet, 32, 24);
            BGTiles = new Tiles(Resource.TowerFallTexture, bgSpriteSheet, 32, 24);
        }

        var solid = loadingLevel["Solids"];
        Solids.SetGrid(solid.InnerText);

        var bg = loadingLevel["BG"];
        BGs.SetGrid(bg.InnerText);

        var solidTiles = loadingLevel["SolidTiles"];
        SolidTiles.SetTiles(solidTiles.InnerText);

        var bgTiles = loadingLevel["BGTiles"];
        BGTiles.SetTiles(bgTiles.InnerText);

        SpawnEntities(loadingLevel);
    }


    private void SpawnEntities(XmlElement xml) 
    {
        var entities = xml["Entities"];
        ulong id = 0;
        HashSet<ulong> idTaken = new();

        foreach (XmlElement entity in entities) 
        {
            var entityName = entity.Name;

            var actor = ActorManager.GetEntity(entityName);
            if (actor == null)
            {
                Logger.Error($"{entityName} is not registered to ActorManager");
                continue;
            }

            ulong entityID = entity.AttrULong("id");
            int x = entity.AttrInt("x");
            int y = entity.AttrInt("y");

            int width = entity.AttrInt("width");
            int height = entity.AttrInt("height");
            List<Vector2> nodes = actor.HasNodes ? new List<Vector2>() : null;



            if (entity.HasChildNodes) 
            {
                nodes = new List<Vector2>();
                foreach (XmlElement child in entity.ChildNodes) 
                {
                    int nx = child.AttrInt("x");
                    int ny = child.AttrInt("y");

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
                    else 
                    {
                        // Need to prevent stackalloc stackoverflow
                        static void AddStringToData(string value, XmlAttribute attr, Dictionary<string, object> customDatas)
                        {
                            ReadOnlySpan<char> val = value;
                            Span<char> loweredVal = stackalloc char[val.Length];
                            val.ToLowerInvariant(loweredVal);

                            if (loweredVal.SequenceEqual("true")) 
                            {
                                customDatas.Add(attr.Name, true);
                            }
                            else if (loweredVal.SequenceEqual("false")) 
                            {
                                customDatas.Add(attr.Name, false);
                            }
                            else 
                            {
                                customDatas.Add(attr.Name, value);
                            }
                        }

                        AddStringToData(value, attr, customDatas);
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
            Actors.Add(levelActor);

            if (entityID > id) 
            {
                id = entityID;
            }
            idTaken.Add(entityID);
        }

        TotalIDs = id;
        for (ulong i = 0; i <= id; i++) 
        {
            if (idTaken.Contains(i))
                continue;
            RetriveID(i);
        }
    }
}
   