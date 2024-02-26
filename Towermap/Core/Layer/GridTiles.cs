using System;
using System.Text;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Components;
using Riateu.Graphics;

namespace Towermap;

public class GridTiles : Entity 
{
    private static char[] Separators = ['\n'];
    private Tilemap tilemap;
    private Spritesheet sheet;
    public Array2D<bool> Bits;

    public GridTiles(Texture texture, Spritesheet sheet) 
    {
        this.sheet = sheet;
        var tiles = new Array2D<Quad?>((int)WorldUtils.WorldWidth / 10, (int)WorldUtils.WorldHeight / 10);
        tiles.Fill(null);
        Bits = new Array2D<bool>((int)WorldUtils.WorldWidth / 10, (int)WorldUtils.WorldHeight / 10);
        tilemap = new Tilemap(texture, tiles, 10, TilemapMode.Cull);
        AddComponent(tilemap);
    }

    public void Clear() 
    {
        tilemap.Clear();
    }

    public bool SetGrid(int x, int y, bool grid) 
    {
        if (grid && Bits[x, y]) 
        {
            return false;
        }
        if (!grid && !Bits[x, y]) 
        {
            return false;
        }

        Bits[x, y] = grid;
        return true;
    }

    public void UpdateTile(int x, int y, int grid) 
    {
        tilemap.SetTile(x, y, grid != -1 ? sheet[grid] : null);
    }

    public void UpdateTiles(Autotiler autotiler, Array2D<bool> also = null) 
    {
        for (int x = 0; x < Bits.Rows; x++) 
        {
            for (int y = 0; y < Bits.Columns; y++) 
            {
                if (Bits[x, y]) 
                {
                    UpdateTile(x, y, autotiler.Tile(Bits, x, y, also)); 
                }
            }
        }
    }

    public void SetGrid(string bitString) 
    {
        ReadOnlySpan<char> bitSpan = bitString;
        Array2D<bool> array = new Array2D<bool>((int)WorldUtils.WorldWidth / 10, (int)WorldUtils.WorldHeight / 10);
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
        Bits = array;
    }

    public string Save() 
    {
        StringBuilder builder = new StringBuilder();
        for (int x = 0; x < Bits.Columns; x ++) 
        {
            for (int y = 0; y < Bits.Rows; y++) 
            {
                char c = Bits[y, x] ? '1' : '0';
                builder.Append(c);
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
}