using System;
using System.Text;
using System.Xml;
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

    public GridTiles(Texture texture, Spritesheet sheet, int gridWidth, int gridHeight) 
    {
        this.sheet = sheet;
        var tiles = new Array2D<TextureQuad?>(gridWidth, gridHeight);
        tiles.Fill(null);
        Bits = new Array2D<bool>(gridWidth, gridHeight);
        tilemap = new Tilemap(tiles, 10, null);
        AddComponent(tilemap);
    }

    public void SetTheme(XmlElement tileset) 
    {
        var atlas = tileset.GetAttribute("image");
        sheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas[atlas], 10, 10);
    }

    public void SetSpriteSheet(Spritesheet spritesheet)
    {
        sheet = spritesheet;
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
        var random = new Random(autotiler.Seed);
        for (int x = 0; x < Bits.Rows; x++) 
        {
            for (int y = 0; y < Bits.Columns; y++) 
            {
                if (Bits[x, y]) 
                {
                    UpdateTile(x, y, autotiler.Tile(random, Bits, x, y, also)); 
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
        bool hasInclude = false;
        for (int i = 0; i < bitSpan.Length; i++) 
        {
            var c = bitSpan[i];
            // can we just have \n instead? please, Windows?
            if (c == '\r' || c == '\n') 
            {
                if (hasInclude)
                {
                    x = 0;
                    y++;
                    hasInclude = false;
                }
                continue;
            }
            hasInclude = true;
            array[x, y] = c != '0';
            x++;
        }
        Bits = array;
    }

    public string Save() 
    {
        const int NewLineCount = 23;
        int len = Bits.Columns * Bits.Rows + NewLineCount;
        using (ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[len]))
        {
            for (int x = 0; x < Bits.Columns; x ++) 
            {
                for (int y = 0; y < Bits.Rows; y++) 
                {
                    char c = Bits[y, x] ? '1' : '0';
                    builder.Append(c);
                }
                if (x != Bits.Columns - 1)
                {
                    builder.AppendLine();
                }
            }
            return builder.ToString();
        }
    }
}