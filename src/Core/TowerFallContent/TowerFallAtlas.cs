using System.Collections.Generic;
using System.Xml;
using Riateu.Graphics;

namespace Towermap.TowerFall;

public sealed class TowerFallAtlas 
{
    private Dictionary<string, int> lookup = new();
    private TextureQuad[] textures;
    public IReadOnlyDictionary<string, int> Lookup => lookup;
    public TextureQuad[] Textures => textures;
    public TextureQuad this[string name] => Get(name);

    public static TowerFallAtlas LoadAtlas(Texture texture, string xmlPath) 
    {
        var tfAtlas = new TowerFallAtlas();
        var document = new XmlDocument();
        document.Load(xmlPath);
        XmlElement textureAtlas = document["TextureAtlas"];
        int i = 0;
        XmlNodeList subTextures = textureAtlas.GetElementsByTagName("SubTexture");
        tfAtlas.textures = new TextureQuad[subTextures.Count];
        foreach (XmlElement subTexture in subTextures) 
        {
            string name = subTexture.Attr("name");
            int x = subTexture.AttrInt("x");
            int y = subTexture.AttrInt("y");
            int width = subTexture.AttrInt("width");
            int height = subTexture.AttrInt("height");

            TextureQuad spTexture = new TextureQuad(texture, new Rectangle(x, y, width, height));
            tfAtlas.textures[i] = spTexture;
            tfAtlas.lookup[name] = i;
            i++;
        }
        return tfAtlas;
    }

    public TextureQuad Get(string name) 
    {
        return textures[lookup[name]];
    }
}
