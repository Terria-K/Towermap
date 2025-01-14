using System;
using System.Collections.Generic;

namespace Towermap;

public class Level 
{
    public List<LevelActor> Actors { get; private set; }
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

    public void PushCommit(History.Commit commit, Layers layer) 
    {
        Unsaved = true;
        history.PushCommit(commit, layer);
    }

    public bool PopCommit(out History.Commit commit) 
    {
        if (history.PopCommit(out commit)) 
        {
            Unsaved = true;
            return true;
        }
        return false;
    }
}