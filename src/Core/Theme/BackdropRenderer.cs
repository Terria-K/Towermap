using System.Collections.Generic;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public class BackdropRenderer 
{
    private List<Element> elements = new List<Element>();

    public void SetTheme(Theme theme) 
    {
        elements.Clear();
        theme.BackdropRender(this);
    }

    public void AddElement(string elementName, Vector2 position = default, float opacity = 1, Point size = default) 
    {
        TextureQuad quad;
        if (size != default) 
        {
            var copyQuad = Resource.BGAtlas[elementName];
            quad = new TextureQuad(Resource.BGAtlasTexture, new Rectangle(copyQuad.Source.X, copyQuad.Source.Y, size.X, size.Y));
        }
        else 
        {
            quad = Resource.BGAtlas[elementName];
        }

        elements.Add(new Element() { Texture = quad, Position = position, Opacity = opacity });
    }

    public void Draw(Batch spriteBatch) 
    {
        foreach (var el in elements) 
        {
            spriteBatch.Draw(el.Texture, el.Position, Color.White * el.Opacity);
        }
    }

    private struct Element 
    {
        public TextureQuad Texture;
        public Vector2 Position;
        public float Opacity;
    }
}