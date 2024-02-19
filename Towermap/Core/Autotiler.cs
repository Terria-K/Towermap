using System;
using System.Xml;
using Riateu;
using Riateu.Graphics;

namespace Towermap;


public class Autotiler 
{
    public int[] Center;
    public int[] Single;
    public int[] SingleHorizontalLeft;
    public int[] SingleHorizontalCenter;
    public int[] SingleHorizontalRight;
    public int[] SingleVerticalTop;
    public int[] SingleVerticalCenter;
    public int[] SingleVerticalBottom;
    public int[] Top;
    public int[] Bottom;
    public int[] Left;
    public int[] Right;
    public int[] TopLeft;
    public int[] TopRight;
    public int[] BottomLeft;
    public int[] BottomRight;
    public int[] InsideTopLeft;
    public int[] InsideTopRight;
    public int[] InsideBottomLeft;
    public int[] InsideBottomRight;
    public int[] Below;

    private Spritesheet tileSheet;
    private bool initialized;

    private bool current = false;
    private bool left = false;
    private bool right = false;
    private bool up = false;
    private bool down = false;
    private bool upLeft = false;
    private bool upRight = false;
    private bool downLeft = false;
    private bool downRight = false;
    private Random random = new Random(42342);

    public Autotiler(Spritesheet spritesheet) 
    {
        tileSheet = spritesheet;
    }

    private static bool Check(int x, int y, Array2D<bool> data) 
    {
        if (!(x < data.Rows && y < data.Columns && x >= 0 && y >= 0)) 
            return true;

        return data[x, y];
    }

    public void Init(string xmlPath, int tilesetID) 
    {
        XmlDocument document = new XmlDocument();
        document.Load(xmlPath);
        XmlElement tilesetData = document["TilesetData"];

        XmlElement tileset = (XmlElement)tilesetData.GetElementsByTagName("Tileset")[tilesetID];

        Center = SplitElementToInt(tileset, "Center");
        Single = SplitElementToInt(tileset, "Single");
        SingleHorizontalLeft = SplitElementToInt(tileset, "SingleHorizontalLeft");
        SingleHorizontalCenter = SplitElementToInt(tileset, "SingleHorizontalCenter");
        SingleHorizontalRight = SplitElementToInt(tileset, "SingleHorizontalRight");
        SingleVerticalTop = SplitElementToInt(tileset, "SingleVerticalTop");
        SingleVerticalCenter = SplitElementToInt(tileset, "SingleVerticalCenter");
        SingleVerticalBottom = SplitElementToInt(tileset, "SingleVerticalBottom");
        Top = SplitElementToInt(tileset, "Top");
        Bottom = SplitElementToInt(tileset, "Bottom");
        Left = SplitElementToInt(tileset, "Left");
        Right = SplitElementToInt(tileset, "Right");
        TopLeft = SplitElementToInt(tileset, "TopLeft");
        TopRight = SplitElementToInt(tileset, "TopRight");
        BottomLeft = SplitElementToInt(tileset, "BottomLeft");
        BottomRight = SplitElementToInt(tileset, "BottomRight");
        InsideTopLeft = SplitElementToInt(tileset, "InsideTopLeft");
        InsideTopRight = SplitElementToInt(tileset, "InsideTopRight");
        InsideBottomLeft = SplitElementToInt(tileset, "InsideBottomLeft");
        InsideBottomRight = SplitElementToInt(tileset, "InsideBottomRight");
        Below = SplitElementToInt(tileset, "Below");

        initialized = true;
    }

    public int Tile(Array2D<bool> bits, int x, int y, Array2D<bool> also = null) 
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
                return Center;
            }
            if (!up && !down) 
            {
                if (left && right) 
                {
                    return SingleHorizontalCenter;
                }
                if (!left && !right) 
                {
                    return Single;
                }
                if (left) 
                {
                    return SingleHorizontalRight;
                }
                return SingleHorizontalLeft;
            }
            else if (!left && !right) 
            {
                if (up && down)  
                {
                    return SingleVerticalCenter;
                }
                if (down) 
                {
                    return SingleVerticalTop;
                }
                return SingleVerticalBottom;
            }
            else
            {
                if (up && down && left && !right) 
                    return Right;

                if (up && down && !left && right) 
                    return Left;

                if (up && !down && !left && right) 
                    return BottomLeft;

                if (up && !down && left && !right) 
                    return BottomRight;
                
                if (!up && down && !left && right)
                    return TopLeft;

                if (!up && down && left && !right)
                    return TopRight;

                if (up && down && downLeft && !downRight)
                    return InsideTopLeft;

                if (up && down && !downLeft && downRight)
                    return InsideTopRight;

                if (up && down && upLeft && !upRight)
                    return InsideBottomLeft;

                if (up && down && !upLeft && upRight)
                    return InsideBottomRight;

                if (!down)
                    return Bottom;
                return Top;
            }
        }
        else if (up)
        {
            return Below;
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