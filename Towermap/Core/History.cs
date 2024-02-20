using System.Collections.Generic;
using Riateu;

namespace Towermap;


public class History 
{
    private Stack<Commit> layouts = new();

    public class Commit
    {
        public Array2D<bool> SolidTiles;
        public Array2D<bool> BGTiles;
    }

    public void PushCommit(Commit commit) 
    {
        var bgTiles = commit.BGTiles.Clone();
        var solidTiles = commit.SolidTiles.Clone();

        layouts.Push(new Commit() 
        {
            SolidTiles = solidTiles,
            BGTiles = bgTiles
        });
    }

    public bool PopCommit(out Commit result) 
    {
        return layouts.TryPop(out result);
    }
}