using System;
using System.Collections.Generic;
using System.Xml;

namespace Towermap.TowerFall;

public sealed class TilesetData 
{
    public Dictionary<string, Tileset> Tilesets = new Dictionary<string, Tileset>();

    public static TilesetData Load(string path)
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);

        TilesetData tilesetData = new TilesetData();
        var data = document["TilesetData"];

        foreach (XmlElement tilesetXml in data.GetElementsByTagName("Tileset"))
        {
            string id = tilesetXml.Attr("id");
            string image = tilesetXml.Attr("image");
            Tileset tileset = new Tileset
            {
                ID = id,
                Image = image,

                Center = SplitElementToInt(tilesetXml, "Center"),
                Single = SplitElementToInt(tilesetXml, "Single"),
                SingleHorizontalLeft = SplitElementToInt(tilesetXml, "SingleHorizontalLeft"),
                SingleHorizontalCenter = SplitElementToInt(tilesetXml, "SingleHorizontalCenter"),
                SingleHorizontalRight = SplitElementToInt(tilesetXml, "SingleHorizontalRight"),
                SingleVerticalTop = SplitElementToInt(tilesetXml, "SingleVerticalTop"),
                SingleVerticalCenter = SplitElementToInt(tilesetXml, "SingleVerticalCenter"),
                SingleVerticalBottom = SplitElementToInt(tilesetXml, "SingleVerticalBottom"),
                Top = SplitElementToInt(tilesetXml, "Top"),
                Bottom = SplitElementToInt(tilesetXml, "Bottom"),
                Left = SplitElementToInt(tilesetXml, "Left"),
                Right = SplitElementToInt(tilesetXml, "Right"),
                TopLeft = SplitElementToInt(tilesetXml, "TopLeft"),
                TopRight = SplitElementToInt(tilesetXml, "TopRight"),
                BottomLeft = SplitElementToInt(tilesetXml, "BottomLeft"),
                BottomRight = SplitElementToInt(tilesetXml, "BottomRight"),
                InsideTopLeft = SplitElementToInt(tilesetXml, "InsideTopLeft"),
                InsideTopRight = SplitElementToInt(tilesetXml, "InsideTopRight"),
                InsideBottomLeft = SplitElementToInt(tilesetXml, "InsideBottomLeft"),
                InsideBottomRight = SplitElementToInt(tilesetXml, "InsideBottomRight"),
                Below = SplitElementToInt(tilesetXml, "Below")
            };

            tilesetData.Tilesets[id] = tileset;
        }

        return tilesetData;
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

    private static int[] SplitStringCSVToInt(ReadOnlySpan<char> innerText) 
    {
        char splitters = ',';
        var count = innerText.Count(splitters) + 1;

        var split = innerText.Split(splitters); 
        int[] nums = new int[count];
        int i = 0;
        foreach (Range segment in split)
        {
            var x = innerText[segment];
            nums[i] = int.Parse(x.Trim());
            i += 1;
        }
        
        return nums;
    }

    public sealed class Tileset
    {
        public string ID;
        public string Image;

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
    }
}