using System.Xml;
using System.Text;
using Riateu;
using Riateu.Components;
using Riateu.Graphics;

namespace Towermap;


public class Tiles : Entity 
{
    private Tilemap tilemap;
    private Spritesheet spritesheet;
    private Array2D<TextureQuad?> tiles;
    public Array2D<int> Ids;

    public Tiles(Texture texture, Spritesheet spritesheet, int gridWidth, int gridHeight) 
    {
        this.spritesheet = spritesheet;
        tiles = new Array2D<TextureQuad?>(gridWidth, gridHeight);
        tiles.Fill(null);
        Ids = new Array2D<int>(gridWidth, gridHeight);
        Ids.Fill(-1);
        tilemap = new Tilemap(tiles, 10);
        AddComponent(tilemap);
    }

    public void SetTheme(XmlElement tileset) 
    {
        var atlas = tileset.GetAttribute("image");
        spritesheet = new Spritesheet(Resource.TowerFallTexture, Resource.Atlas[atlas], 10, 10);
    }

    public void Clear() 
    {
        tilemap.Clear();
    }

    public void UpdateTiles() 
    {
        SetTiles(Ids);
    }

    public void SetTile(int x, int y, int tileID) 
    {
        if (tileID == -1) 
        {
            tiles[x, y] = null; 
            Ids[x, y] = -1;
            return;
        }
        tiles[x, y] = spritesheet.GetTexture(tileID);
        Ids[x, y] = tileID;
    }

    public void SetTiles(Array2D<int> tileIds) 
    {
        Ids = tileIds.Clone();
        for (int x = 0; x < tileIds.Columns; x++) 
        {
            for (int y = 0; y < tileIds.Rows; y++) 
            {
                var gid = Ids[y, x];
                if (gid == -1) 
                {
                    tiles[y, x] = null;
                    continue;
                }
                tiles[y, x] = spritesheet.GetTexture(gid);
            }
        }
    }

    public void SetTiles(string csv) 
    {
        string newLine = "\n";
        if (csv.Contains("\r\n"))
        {
            // yeah, I hate this
            newLine = "\r\n";
        }
        string[] splitted = csv.Split(newLine);
        for (int i = 0; i < splitted.Length; i++) 
        {
            string[] strTiles = splitted[i].Split(',');

            for (int j = 0; j < strTiles.Length; j++) 
            {
                string tile = strTiles[j];
                if (string.IsNullOrEmpty(tile))
                    continue;
                int num = int.Parse(strTiles[j]);
                Ids[j, i] = num;
                if (num == -1) 
                {
                    continue;
                }
                this.tiles[j, i] = spritesheet.GetTexture(num);
            }
        }
    }

    public string Save() 
    {
        using (ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[4096]))
        {
            for (int x = 0; x < Ids.Columns; x++) 
            {
                for (int y = 0; y < Ids.Rows; y++) 
                {
                    builder.AppendFormattable(Ids[y, x]);
                    if (y != Ids.Rows - 1)
                        builder.Append(',');
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}