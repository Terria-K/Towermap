using System;
using System.Xml;
using Riateu;
using Towermap.TowerFall;

namespace Towermap;


public class Autotiler 
{
    private bool initialized;
    private int seed = 0;
    public int Seed 
    {
        get => seed;
        set 
        {
            seed = value;
            random = new Random(seed);
        }
    }

    private bool current = false;
    private bool left = false;
    private bool right = false;
    private bool up = false;
    private bool down = false;
    private bool upLeft = false;
    private bool upRight = false;
    private bool downLeft = false;
    private bool downRight = false;
    private TilesetData.Tileset tileset;
    private Random random = new Random();

    private static bool Check(int x, int y, Array2D<bool> data) 
    {
        if (!(x < data.Rows && y < data.Columns && x >= 0 && y >= 0)) 
            return true;

        return data[x, y];
    }

    public void Init(TilesetData.Tileset tileset) 
    {
        this.tileset = tileset;
        initialized = true;
    }

    public int Tile(Array2D<bool> bits, int x, int y, Array2D<bool> also = null) 
    {
        return Tile(random, bits, x, y, also);
    }

    public int Tile(Random random, Array2D<bool> bits, int x, int y, Array2D<bool> also = null) 
    {
        if (!initialized || !ArrayUtils.ArrayCheck(x, y, bits) || !(current = bits[x, y]))
        {
            return -1;
        }

        if (also != null) 
        {
            left = Check(x - 1, y, bits) || Check(x - 1, y, also);
            right = Check(x + 1, y, bits) || Check(x + 1, y, also);
            up = Check(x, y - 1, bits) || Check(x, y - 1, also);
            down = Check(x, y + 1, bits) || Check(x, y + 1, also);

            upLeft = Check(x - 1, y - 1, bits) || Check(x - 1, y - 1, also);
            upRight = Check(x + 1, y - 1, bits) || Check(x + 1, y - 1, also);
            downLeft = Check(x - 1, y + 1, bits) || Check(x - 1, y + 1, also);
            downRight = Check(x + 1, y + 1, bits) || Check(x + 1, y + 1, also);
        }
        else 
        {
            left = Check(x - 1, y, bits);
            right = Check(x + 1, y, bits);
            up = Check(x, y - 1, bits);
            down = Check(x, y + 1, bits);

            upLeft = Check(x - 1, y - 1, bits);
            upRight = Check(x + 1, y - 1, bits);
            downLeft = Check(x - 1, y + 1, bits);
            downRight = Check(x + 1, y + 1, bits);
        }

        int[] tiles = HandleTiles();
        return tiles[random.Next() % tiles.Length];
    }

    private int[] HandleTiles() 
    {
        if (current) 
        {
            if (left && right && up && down && upLeft && upRight && downLeft && downRight)
            {
                return tileset.Center;
            }
            if (!up && !down) 
            {
                if (left && right) 
                {
                    return tileset.SingleHorizontalCenter;
                }
                if (!left && !right) 
                {
                    return tileset.Single;
                }
                if (left) 
                {
                    return tileset.SingleHorizontalRight;
                }
                return tileset.SingleHorizontalLeft;
            }
            else if (!left && !right) 
            {
                if (up && down)  
                {
                    return tileset.SingleVerticalCenter;
                }
                if (down) 
                {
                    return tileset.SingleVerticalTop;
                }
                return tileset.SingleVerticalBottom;
            }
            else
            {
                if (up && down && left && !right) 
                    return tileset.Right;

                if (up && down && !left && right) 
                    return tileset.Left;

                if (up && !down && !left && right) 
                    return tileset.BottomLeft;

                if (up && !down && left && !right) 
                    return tileset.BottomRight;
                
                if (!up && down && !left && right)
                    return tileset.TopLeft;

                if (!up && down && left && !right)
                    return tileset.TopRight;

                if (up && down && downLeft && !downRight)
                    return tileset.InsideTopLeft;

                if (up && down && !downLeft && downRight)
                    return tileset.InsideTopRight;

                if (up && down && upLeft && !upRight)
                    return tileset.InsideBottomLeft;

                if (up && down && !upLeft && upRight)
                    return tileset.InsideBottomRight;

                if (!down)
                    return tileset.Bottom;
                return tileset.Top;
            }
        }
        else if (up)
        {
            return tileset.Below;
        }
        return null;
    }

    private static int[] SplitElementToInt(XmlElement element, string child) 
    {
        var elm = element[child];
        if (elm == null) 
        {
            return null;
        }
        return SplitStringCSVToInt(elm.InnerText);
    }

    private static int[] SplitStringCSVToInt(string innerText) 
    {
        var split = innerText.Split(","); 
        int[] nums = new int[split.Length];
        for (int i = 0; i < split.Length; i++) 
        {
            var sp = split[i];
            nums[i] = int.Parse(sp.Trim());
        }
        return nums;
    }
}