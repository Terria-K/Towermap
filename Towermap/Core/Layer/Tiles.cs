using System.Text;
using MoonWorks.Graphics;
using Riateu;
using Riateu.Components;
using Riateu.Graphics;

namespace Towermap;


public class Tiles : Entity 
{
    private Tilemap tilemap;
    private Spritesheet spritesheet;
    private Array2D<SpriteTexture?> tiles;
    private Array2D<int> ids;

    public Tiles(Texture texture, Spritesheet spritesheet) 
    {
        this.spritesheet = spritesheet;
        tiles = new Array2D<SpriteTexture?>(32, 24);
        tiles.Fill(null);
        ids = new Array2D<int>(32, 24);
        ids.Fill(-1);
        tilemap = new Tilemap(texture, tiles, 10, TilemapMode.Cull);
        AddComponent(tilemap);
    }

    public void Clear() 
    {
        tilemap.Clear();
    }

    public void SetTile(int x, int y, int tileID) 
    {
        if (tileID == -1) 
        {
            tiles[x, y] = null; 
            return;
        }
        tiles[x, y] = spritesheet.GetTexture(tileID);
        ids[x, y] = tileID;
    }

    public void SetTiles(string csv) 
    {
        string[] splitted = csv.Split('\n');
        for (int i = 0; i < splitted.Length; i++) 
        {
            string[] strTiles = splitted[i].Split(',');

            for (int j = 0; j < strTiles.Length; j++) 
            {
                string tile = strTiles[j];
                if (string.IsNullOrEmpty(tile))
                    continue;
                int num = int.Parse(strTiles[j]);
                ids[j, i] = num;
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
        StringBuilder builder = new StringBuilder();
        for (int x = 0; x < ids.Columns; x++) 
        {
            for (int y = 0; y < ids.Rows; y++) 
            {
                builder.Append(ids[y, x]);
                if (y != ids.Rows - 1)
                    builder.Append(',');
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
}