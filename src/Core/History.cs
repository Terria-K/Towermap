using System.Collections.Generic;
using Riateu;

namespace Towermap;


public class History 
{
    private Stack<Commit> undoCommits = new();
    private Stack<Commit> redoCommits = new();

    public struct Commit
    {
        public Layers Layer;
        public Array2D<bool> Solids;
        public Array2D<bool> BGs;
        public Array2D<int> BGTiles;
        public Array2D<int> SolidTiles;
        public List<LevelActor> Actors;
        public LevelActor CurrentSelectedActor;
    }

    public void PushCommit(Commit commit, Layers layer, bool redo, bool shoudClearRedo) 
    {
        var bg = commit.BGs?.Clone();
        var solid = commit.Solids?.Clone();
        var bgTiles = commit.BGTiles?.Clone();
        var solidTiles = commit.SolidTiles?.Clone();
        List<LevelActor> list = null;
        LevelActor currentSelected = null;
        if (commit.Actors != null)
        {
            list = new List<LevelActor>();
            if (commit.CurrentSelectedActor != null)
            {
                var currSelected = commit.CurrentSelectedActor.Clone();
                foreach (var actor in commit.Actors)
                {
                    if (commit.CurrentSelectedActor != actor)
                    {
                        list.Add(actor.Clone());
                    }
                }
                currentSelected = currSelected;
                list.Add(currSelected);
            }
            else 
            {
                foreach (var actor in commit.Actors)
                {
                    if (commit.CurrentSelectedActor != actor)
                    {
                        list.Add(actor.Clone());
                    }
                }
            }
        }

        if (shoudClearRedo)
        {
            redoCommits.Clear();
        }

        if (redo)
        {
            redoCommits.Push(new Commit() 
            {
                Solids = solid,
                BGs = bg,
                BGTiles = bgTiles,
                SolidTiles = solidTiles,
                Actors = list,
                Layer = layer,
                CurrentSelectedActor = currentSelected
            });

            return;
        }

        undoCommits.Push(new Commit() 
        {
            Solids = solid,
            BGs = bg,
            BGTiles = bgTiles,
            SolidTiles = solidTiles,
            Actors = list,
            Layer = layer,
            CurrentSelectedActor = currentSelected
        });
    }

    public bool PopCommit(out Commit result, bool redo) 
    {
        if (redo) 
        {
            if (redoCommits.TryPop(out result))
            {
                return true;
            }
            return false;
        }
        if (undoCommits.TryPop(out result)) 
        {
            return true;
        }

        return false;
    }
}