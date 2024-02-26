using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Towermap;

public delegate void ActorRender(LevelActor actor, Vector2 position, IBatch spriteBatch);

public class ActorManager 
{
    public Dictionary<string, Actor> Actors = new();

    public void AddActor(ActorInfo info, Point? textureSize = null, ActorRender onRender = null) 
    {
        var texture = Resource.Atlas[info.Texture];
        if (textureSize != null) 
        {
            var sx = texture.Source.X / (float)Resource.TowerFallTexture.Width;
            var sy = texture.Source.Y / (float)Resource.TowerFallTexture.Height;
            
            var sw = textureSize.Value.X / (float)Resource.TowerFallTexture.Width;
            var sh = textureSize.Value.Y / (float)Resource.TowerFallTexture.Height;

            var uv = new UV(new Vector2(sx, sy), new Vector2(sw, sh));
            texture = texture with {
                Width = textureSize.Value.X,
                Height = textureSize.Value.Y,
                UV = uv
            };
        }
        var actor = new Actor() 
        {
            Name = info.Name,
            Texture = texture,
            Width = info.Width,
            Height = info.Height,
            Origin = new Vector2(info.OriginX, info.OriginY),
            OnRender = onRender
        };
        Actors.Add(info.Name, actor);
    }

    public void PlaceEntity(string currentSelected, Vector2 position) 
    {
        var actor = Actors[currentSelected];
    }
}

public class Actor 
{
    public string Name;
    public Quad Texture;
    public int Width;
    public int Height;
    public Vector2 Origin;
    public ActorRender OnRender;
}


public struct ActorInfo 
{
    public string Name;
    public string Texture;
    public int Width;
    public int Height;
    public int OriginX;
    public int OriginY;

    public ActorInfo(string name, string texture, int width = 20, int height = 20, int originX = 0, int originY = 0) 
    {
        Name = name;
        Texture = texture;
        Width = width;
        Height = height;
        OriginX = originX;
        OriginY = originY;
    }
}