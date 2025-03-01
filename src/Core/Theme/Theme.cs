using System;
using System.Xml;
using Riateu.Graphics;

namespace Towermap;

public class Theme
{
    public string Name { get; }
    public string SolidTilesetID { get; }
    public string BGTilesetID { get; }
    public Action<BackdropRenderer> BackdropRender { get; }
    public TextureQuad SolidTilesQuad;
    public TextureQuad BGTilesQuad;

    public Theme(string name, string solidTilesetID, string bgTilesetID, Action<BackdropRenderer> backdropRender) 
    {
        Name = name;
        SolidTilesetID = solidTilesetID;
        BGTilesetID = bgTilesetID;
        BackdropRender = backdropRender;
        var solidTileset = Resource.TilesetData.Tilesets[solidTilesetID];
        var bgTileset = Resource.TilesetData.Tilesets[bgTilesetID];
        SolidTilesQuad = Resource.Atlas[solidTileset.Image];
        BGTilesQuad = Resource.Atlas[bgTileset.Image];
        
    }
}