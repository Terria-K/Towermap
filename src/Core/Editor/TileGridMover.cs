using System;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public sealed class TileGridMover
{
    private Rectangle gridRectangle;
    public bool Started;
    public Vector2 StartPos;
    public Vector2 EndPos;
    public Rectangle ResultRect;

    public void Start(Vector2 startPos)
    {
        Started = true;
        StartPos = startPos;
    }

    public void Reset()
    {
        gridRectangle = Rectangle.Zero;
    }

    public void Update(int x, int y)
    {
        int rx = (int)(Math.Floor((((x - WorldUtils.WorldX) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);
        int ry = (int)(Math.Floor((((y - WorldUtils.WorldY) / WorldUtils.WorldSize)) / 10.0f) * 10.0f);

        EndPos = new Vector2(rx, ry);

        ResultRect = CalculateResult();
    }

    public Rectangle CalculateResult()
    {
        return new Rectangle(
            (int)Math.Abs(StartPos.X - EndPos.X - gridRectangle.X),
            (int)Math.Abs(StartPos.Y - EndPos.Y - gridRectangle.Y),
            gridRectangle.Width,
            gridRectangle.Height
        );
    }

    public void SetRect(Rectangle rectangle)
    {
        gridRectangle = rectangle;
    }

    public bool IsHovered(int x, int y)
    {
        return gridRectangle.Width != 0 && gridRectangle.Height != 0 && gridRectangle.Contains(new Point(x, y));
    }

    public void Draw(Batch levelBatch, Level level, Layers currentLayer)
    {
        DrawUtils.Rect(
            levelBatch, 
            new Vector2(gridRectangle.X, gridRectangle.Y), 
            Color.Yellow * 0.7f, 
            new Vector2(gridRectangle.Width, gridRectangle.Height), 
            Vector2.Zero
        );

        if (level != null && Started)
        {
            ref var rectangle = ref gridRectangle;
            for (int dx = 0; dx < rectangle.Width / 10; dx += 1) 
            {
                for (int dy = 0; dy < rectangle.Height / 10; dy += 1)
                {
                    int px = WorldUtils.ToGrid(ResultRect.X) + dx;
                    int py = WorldUtils.ToGrid(ResultRect.Y) + dy;
                    int gx = WorldUtils.ToGrid(gridRectangle.X) + dx;
                    int gy = WorldUtils.ToGrid(gridRectangle.Y) + dy;

                    GridTiles gridTiles = currentLayer switch 
                    {
                        Layers.Solids => level.Solids,
                        Layers.BG => level.BGs,
                        _ => throw new NotImplementedException()
                    };

                    var quad = gridTiles.Tiles.GetTile(new Point(gx, gy));
                    if (quad.HasValue)
                    {
                        levelBatch.Draw(quad.Value, new Vector2(px * 10, py * 10), Color.White);
                    }
                }
            }

            DrawUtils.Rect(
                levelBatch, 
                new Vector2(ResultRect.X, ResultRect.Y), 
                Color.Green * 0.7f, 
                new Vector2(ResultRect.Width, ResultRect.Height), 
                Vector2.Zero
            );
        }
    }
}