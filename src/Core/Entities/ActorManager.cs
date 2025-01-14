using System;
using System.Collections.Generic;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public delegate void ActorRender(LevelActor actor, Vector2 position, Batch spriteBatch);

public class ActorManager 
{
    public ulong TotalIDs = 0;
    private Queue<ulong> unusedIds = new();
    public Dictionary<string, Actor> Actors = new();

    public void AddActor(ActorInfo info, Point? textureSize = null, ActorRender onRender = null) 
    {
        var texture = Resource.Atlas[info.Texture];
        if (textureSize != null) 
        {
            texture = new TextureQuad(
                Resource.TowerFallTexture, 
                new Rectangle(texture.Source.X, texture.Source.Y, textureSize.Value.X, textureSize.Value.Y)
            );
        }
        var actor = new Actor() 
        {
            Name = info.Name,
            Texture = texture,
            Width = info.Width,
            Height = info.Height,
            Origin = new Vector2(info.OriginX, info.OriginY),
            ResizeableX = info.ResizeableX,
            ResizeableY = info.ResizeableY,
            OnRender = onRender,
            HasNodes = info.HasNodes,
            CustomValues = info.CustomValues
        };
        
        Actors.Add(info.Name, actor);
    }

    public Actor GetEntity(string currentSelected) 
    {
        if (Actors.TryGetValue(currentSelected, out Actor selected)) 
        {
            return selected;
        }
        return null;
    }

    public ulong GetID() 
    {
        if (unusedIds.TryDequeue(out ulong result)) 
        {
            return result;
        }
        return TotalIDs++;
    }

    public void RetriveID(ulong id) 
    {
        unusedIds.Enqueue(id);
    }

    public void ClearIDs() 
    {
        TotalIDs = 0;
        unusedIds.Clear();
    }
}
