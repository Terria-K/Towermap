using System.Collections.Generic;
using MoonWorks.Math.Float;
using Riateu.Graphics;

namespace Towermap;

public delegate void ActorRender(LevelActor actor, Vector2 position, IBatch spriteBatch);

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
            ResizeableX = info.ResizeableX,
            ResizeableY = info.ResizeableY,
            OnRender = onRender,
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

public class Actor 
{
    public string Name;
    public Quad Texture;
    public int Width;
    public int Height;
    public Vector2 Origin;
    public ActorRender OnRender;
    public bool ResizeableX;
    public bool ResizeableY;
    public Dictionary<string, object> CustomValues;
}


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
    public Dictionary<string, object> CustomValues;

    public ActorInfo(string name, string texture, int width = 20, int height = 20, int originX = 0, int originY = 0, bool resizeableX = false, bool resizeableY = false, Dictionary<string, object> customValues = null) 
    {
        Name = name;
        Texture = texture;
        Width = width;
        Height = height;
        OriginX = originX;
        OriginY = originY;
        ResizeableX = resizeableX;
        ResizeableY = resizeableY;
        CustomValues = customValues;
    }
}