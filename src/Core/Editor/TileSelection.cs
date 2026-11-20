using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public sealed class TileSelection
{
    public Rectangle Rect;
    public Layers Layer;
    public bool Floating { get; private set; }
    public bool Dragging { get; private set; }

    private Rectangle lifted;
    private Point dragStart;
    private Rectangle dragRect;
    private bool[] bits;
    private int[] ids;
    private TextureQuad?[] quads;
    private IEditCommand edit;

    public bool HasSelection => Rect.Width > 0 && Rect.Height > 0;
    private int Columns => Rect.Width / 10;
    private int Rows => Rect.Height / 10;

    public bool Contains(float x, float y) =>
        HasSelection && x >= Rect.X && y >= Rect.Y && x < Rect.X + Rect.Width && y < Rect.Y + Rect.Height;

    public void Set(Rectangle rect, Layers layer)
    {
        Rect = rect;
        Layer = layer;
        Floating = false;
        Dragging = false;
    }

    public void Clear() => Set(Rectangle.Zero, Layer);

    public void BeginDrag(int x, int y)
    {
        Dragging = true;
        dragStart = new Point(x, y);
        dragRect = Rect;
    }

    public void Drag(int x, int y)
    {
        Rect.X = dragRect.X + x - dragStart.X;
        Rect.Y = dragRect.Y + y - dragStart.Y;
    }

    public void EndDrag() => Dragging = false;

    public void Lift(GridTiles grid)
    {
        var gridEdit = new GridEdit(grid, Layer);
        bits = new bool[Columns * Rows];
        quads = new TextureQuad?[bits.Length];
        ForEachCell((i, x, y) =>
        {
            bits[i] = grid.Bits[x, y];
            quads[i] = grid.Tiles.GetTile(new Point(x, y));
            if (grid.SetGrid(x, y, false))
            {
                gridEdit.Record(x, y, true, false);
            }
        });
        BeginFloat(gridEdit);
    }

    public void Lift(Tiles tiles)
    {
        var tileEdit = new TileEdit(tiles, Layer);
        ids = new int[Columns * Rows];
        quads = new TextureQuad?[ids.Length];
        ForEachCell((i, x, y) =>
        {
            ids[i] = tiles.Ids[x, y];
            quads[i] = tiles.Tilemap.GetTile(new Point(x, y));
            if (ids[i] != -1)
            {
                tiles.SetTile(x, y, -1);
                tileEdit.Record(x, y, ids[i], -1);
            }
        });
        BeginFloat(tileEdit);
    }

    public SelectionMove Drop()
    {
        switch (edit)
        {
        case GridEdit gridEdit:
            ForEachCell((i, x, y) =>
            {
                if (bits[i] && gridEdit.Grid.SetGrid(x, y, true))
                {
                    gridEdit.Record(x, y, false, true);
                }
            });
            break;
        case TileEdit tileEdit:
            ForEachCell((i, x, y) =>
            {
                int before = tileEdit.Tiles.Ids[x, y];
                if (ids[i] != -1 && before != ids[i])
                {
                    tileEdit.Tiles.SetTile(x, y, ids[i]);
                    tileEdit.Record(x, y, before, ids[i]);
                }
            });
            break;
        }

        var move = new SelectionMove(edit, Layer, lifted, Rect);
        Floating = false;
        Dragging = false;
        edit = null;
        bits = null;
        ids = null;
        quads = null;
        return move;
    }

    public void Draw(Batch batch)
    {
        if (!HasSelection)
        {
            return;
        }

        if (Floating)
        {
            ForEachCell((i, x, y) =>
            {
                if (quads[i] is TextureQuad quad)
                {
                    batch.Draw(quad, new Vector2(x * 10, y * 10), Color.White);
                }
            }, clip: false);
        }

        var position = new Vector2(Rect.X, Rect.Y);
        var size = new Vector2(Rect.Width, Rect.Height);
        Color color = Floating ? Color.Green : Color.Yellow;
        DrawUtils.Rect(batch, position, color * 0.15f, size, Vector2.Zero);
        DrawUtils.Rect(batch, position, color, new Vector2(size.X, 1), Vector2.Zero);
        DrawUtils.Rect(batch, position + new Vector2(0, size.Y - 1), color, new Vector2(size.X, 1), Vector2.Zero);
        DrawUtils.Rect(batch, position, color, new Vector2(1, size.Y), Vector2.Zero);
        DrawUtils.Rect(batch, position + new Vector2(size.X - 1, 0), color, new Vector2(1, size.Y), Vector2.Zero);
    }

    private void BeginFloat(IEditCommand liftEdit)
    {
        edit = liftEdit;
        lifted = Rect;
        Floating = true;
    }

    private void ForEachCell(System.Action<int, int, int> action, bool clip = true)
    {
        int originX = WorldUtils.ToGrid(Rect.X);
        int originY = WorldUtils.ToGrid(Rect.Y);
        int rows = Rows;
        for (int dx = 0; dx < Columns; dx++)
        {
            for (int dy = 0; dy < rows; dy++)
            {
                int x = originX + dx;
                int y = originY + dy;
                if (!clip || WorldUtils.InBounds(x, y))
                {
                    action(dx * rows + dy, x, y);
                }
            }
        }
    }
}
