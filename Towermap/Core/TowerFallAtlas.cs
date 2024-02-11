using System.Collections.Generic;
using System.Xml;
using MoonWorks.Graphics;
using Riateu.Graphics;

namespace Towermap;

public class TowerFallAtlas 
{
    private Dictionary<string, int> lookup = new();
    private SpriteTexture[] textures;
    public IReadOnlyDictionary<string, int> Lookup => lookup;
    public SpriteTexture[] Textures => textures;
    public SpriteTexture this[string name] => Get(name);

    public static TowerFallAtlas LoadAtlas(Texture texture, string xmlPath) 
    {
        var tfAtlas = new TowerFallAtlas();
        var document = new XmlDocument();
        document.Load(xmlPath);
        XmlElement textureAtlas = document["TextureAtlas"];
        int i = 0;
        XmlNodeList subTextures = textureAtlas.GetElementsByTagName("SubTexture");
        tfAtlas.textures = new SpriteTexture[subTextures.Count];
        foreach (XmlElement subTexture in subTextures) 
        {
            string name = subTexture.GetAttribute("name");
            int x = int.Parse(subTexture.GetAttribute("x"));
            int y = int.Parse(subTexture.GetAttribute("y"));
            int width = int.Parse(subTexture.GetAttribute("width"));
            int height = int.Parse(subTexture.GetAttribute("height"));

            SpriteTexture spTexture = new SpriteTexture(texture, new Rect(x, y, width, height));
            tfAtlas.textures[i] = spTexture;
            tfAtlas.lookup[name] = i;
            i++;
        }
        return tfAtlas;
    }

    public SpriteTexture Get(string name) 
    {
        return textures[lookup[name]];
    }
}