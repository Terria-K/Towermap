using System;
using System.Numerics;
using System.Xml;
using Riateu.Graphics;

namespace Towermap;

public class Theme
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string TowerType { get; set; }
    public Vector2 MapPosition { get; set; }
    public string Music { get; set; }
    public float DarknessOpacity { get; set; }
    public string DarknessColor { get; set; }
    public float Wind { get; set; }
    public string World { get; set; }
    public bool Cold { get; set; }
    public bool Raining { get; set; }
    public string Tileset { get; set; }
    public string BGTileset { get; set; }
    public string Lanterns { get; set; }
    public string DrillParticleColor { get; set; }
    public string CrackedBlockColor { get; set; }
    public string Background { get; set; }

    public Action<BackdropRenderer> BackdropRender { get; set; }
    public TextureQuad SolidTilesQuad;
    public TextureQuad BGTilesQuad;

    public Theme(string solidTilesetID, string bgTilesetID) 
    {
        Tileset = solidTilesetID;
        BGTileset = bgTilesetID;
        var solidTileset = Resource.TilesetData.Tilesets[solidTilesetID];
        var bgTileset = Resource.TilesetData.Tilesets[bgTilesetID];
        SolidTilesQuad = Resource.Atlas[solidTileset.Image];
        BGTilesQuad = Resource.Atlas[bgTileset.Image];
    }

    public void SetRenderer(Action<BackdropRenderer> backdropRenderer)
    {
        BackdropRender = backdropRenderer;
    }
}