using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Towermap;

public delegate void ActorRender(LevelActor actor, Vector2 position, IBatch spriteBatch);

public class ActorManager 
{
    public Dictionary<string, Actor> Actors = new();

    public void AddActor(ActorInfo info, ActorRender onRender = null) 
    {
        var actor = new Actor() 
        {
            Name = info.Name,
            Texture = Resource.Atlas[info.Texture],
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
    public SpriteTexture Texture;
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