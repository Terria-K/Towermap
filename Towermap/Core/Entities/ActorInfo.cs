using System.Collections.Generic;

namespace Towermap;

public struct ActorInfo 
{
    public string Name;
    public string Texture;
    public int Width;
    public int Height;
    public int OriginX;
    public int OriginY;
    public bool ResizeableX;
    public bool ResizeableY;
    public bool HasNodes;
    public Dictionary<string, object> CustomValues;

    public ActorInfo(string name, string texture, int width = 20, int height = 20, int originX = 0, int originY = 0, bool resizeableX = false, bool resizeableY = false, bool hasNodes = false, Dictionary<string, object> customValues = null) 
    {
        Name = name;
        Texture = texture;
        Width = width;
        Height = height;
        OriginX = originX;
        OriginY = originY;
        ResizeableX = resizeableX;
        ResizeableY = resizeableY;
        HasNodes = hasNodes;
        CustomValues = customValues;
    }
}