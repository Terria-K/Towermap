using System.Collections.Generic;
using System.Numerics;

namespace Towermap;

public struct LevelActorInfo(ulong id, string name, int x, int y, Dictionary<string, object> values, Vector2[] nodes)
{
    public ulong ID = id;
    public string Name = name;
    public int X = x;
    public int Y = y;
    public Dictionary<string, object> Values = values;
    public Vector2[] Nodes = nodes;
}