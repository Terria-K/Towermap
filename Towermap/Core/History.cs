using System.Collections.Generic;
using Riateu;

namespace Towermap;


public class History 
{
    private Stack<Commit> undoCommits = new();

    public struct Commit
    {
        public Array2D<bool> Solids;
        public Array2D<bool> BGs;
    }

    public void PushCommit(Commit commit) 
    {
        var bgTiles = commit.BGs.Clone();
        var solidTiles = commit.Solids.Clone();

        undoCommits.Push(new Commit() 
        {
            Solids = solidTiles,
            BGs = bgTiles
        });
    }

    public bool PopCommit(out Commit result) 
    {
        return undoCommits.TryPop(out result);
    }
}