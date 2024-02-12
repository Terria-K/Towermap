using System;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Components;
using Riateu.Graphics;

namespace Towermap;

public class SolidTiles : Entity 
{
    private static char[] Separators = ['\n'];
    private Tilemap tilemap;
    private Spritesheet sheet;

    public SolidTiles(Texture texture, Spritesheet sheet) 
    {
        this.sheet = sheet;
        // TODO dynamic width and height for sets
        var tiles = new Array2D<SpriteTexture?>(32, 24);
        tiles.Fill(null);
        tilemap = new Tilemap(texture, tiles, 10, TilemapMode.Cull);
        AddComponent(tilemap);
    }

    public void Clear() 
    {
        tilemap.Clear();
    }

    public void SetTile(int x, int y, int grid) 
    {
        tilemap.SetTile(x, y, grid != -1 ? sheet[grid] : null);
    }

    public void SetTile(Array2D<bool> tiles, Autotiler autotiler) 
    {
        for (int x = 0; x < tiles.Rows; x++) 
        {
            for (int y = 0; y < tiles.Columns; y++) 
            {
                if (tiles[x, y]) 
                {
                    var tile = autotiler.Tile(tiles, x, y);
                    SetTile(x, y, tile); 
                }
            }
        }
    }

    public Array2D<bool> GetTiles(string bitString) 
    {
        ReadOnlySpan<char> bitSpan = bitString;
        Array2D<bool> array = new Array2D<bool>(32, 24);
        int x = 0;
        int y = 0;
        for (int i = 0; i < bitSpan.Length; i++) 
        {
            var c = bitSpan[i];
            if (c == '\n') 
            {
                x = 0;
                y++;
                continue;
            }
            if (c == '0') 
            {
                array[x, y] = false;
            }
            else 
            {
                array[x, y] = true;
            }
            x++;
        }
        return array;
    }
}