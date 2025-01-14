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

    public Theme(XmlElement tilesetData, string name, string solidTilesetID, string bgTilesetID, Action<BackdropRenderer> backdropRender) 
    {
        Name = name;
        SolidTilesetID = solidTilesetID;
        BGTilesetID = bgTilesetID;
        BackdropRender = backdropRender;
        foreach (XmlElement tileset in tilesetData.ChildNodes) 
        {
            string id = tileset.GetAttribute("id");
            if (id == SolidTilesetID) 
            {
                SolidTilesQuad = Resource.Atlas[tileset.GetAttribute("image")];
            }
            else if (id == BGTilesetID) 
            {
                BGTilesQuad = Resource.Atlas[tileset.GetAttribute("image")];
            }
        }
    }
}