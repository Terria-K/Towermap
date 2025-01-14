using System.Collections.Generic;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public class Actor 
{
    public string Name;
    public TextureQuad Texture;
    public int Width;
    public int Height;
    public Vector2 Origin;
    public ActorRender OnRender;
    public bool ResizeableX;
    public bool ResizeableY;
    public bool HasNodes;
    public Dictionary<string, object> CustomValues;
}
