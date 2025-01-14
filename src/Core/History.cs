using System.Collections.Generic;
using Riateu;

namespace Towermap;


public class History 
{
    private Stack<Commit> undoCommits = new();

    public struct Commit
    {
        public Layers Layer;
        public Array2D<bool> Solids;
        public Array2D<bool> BGs;
        public Array2D<int> BGTiles;
        public Array2D<int> SolidTiles;
    }

    public void PushCommit(Commit commit, Layers layer) 
    {
        var bg = commit.BGs?.Clone();
        var solid = commit.Solids?.Clone();
        var bgTiles = commit.BGTiles?.Clone();
        var solidTiles = commit.SolidTiles?.Clone();

        undoCommits.Push(new Commit() 
        {
            Solids = solid,
            BGs = bg,
            BGTiles = bgTiles,
            SolidTiles = solidTiles,
            Layer = layer
        });
    }

    public bool PopCommit(out Commit result) 
    {
        return undoCommits.TryPop(out result);
    }
}