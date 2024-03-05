using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Towermap;

public class Actor 
{
    public string Name;
    public Quad Texture;
    public int Width;
    public int Height;
    public Vector2 Origin;
    public ActorRender OnRender;
    public bool ResizeableX;
    public bool ResizeableY;
    public Dictionary<string, object> CustomValues;
}
