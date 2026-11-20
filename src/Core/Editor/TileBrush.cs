using System;
using Cosmiria;
using Riateu;
using Riateu.Graphics;
using Towermap.TowerFall;

namespace Towermap;

public sealed class TileBrush(string atlasName)
{
    public TextureQuad Texture = Resource.Atlas[atlasName];
    public Rectangle Selection = new Rectangle(0, 0, 10, 10);

    public void SetTheme(TilesetData.Tileset tileset)
    {
        Texture = Resource.Atlas[tileset.Image];
    }

    public Array2D<int> GetData()
    {
        var data = new Array2D<int>(Selection.Height / 10, Selection.Width / 10);
        for (int x = 0; x < data.Columns; x++)
        {
            for (int y = 0; y < data.Rows; y++)
            {
                int rx = Selection.X + x * 10;
                int ry = Selection.Y + y * 10;
                data[y, x] = (ry / 10) * (Texture.Width / 10) + (rx / 10);
            }
        }
        return data;
    }
}

public struct TilePalette : IComponent
{
    public TextureQuad Texture;
    public Rectangle Selection;
    public Callback<Rectangle> OnSelect;

    private Point start;
    private bool holding;

    public Node Render(UIStack ui, EventStack ev)
    {
        float w = Texture.Width;
        float h = Texture.Height;
        var selection = default(Style).Absolute()
            .Left(Length.Percent(Selection.X / w * 100))
            .Top(Length.Percent(Selection.Y / h * 100))
            .W(Length.Percent(Selection.Width / w * 100))
            .H(Length.Percent(Selection.Height / h * 100))
            .Border(2, Tailwind.Yellow400);

        return ui.View(default(Style).Relative().W(Length.Full).Bg(Styles.Black).Border(1, Styles.Line)).Child([
            ui.AtlasImage(Texture, ImageFit.Fill, default(Style).W(Length.Full))
                .OnDrag(ev.Listener(static (ref TilePalette p, in PointerEvent e) => p.Drag(e))),
            ui.View(selection).IgnorePointer()
        ]);
    }

    private void Drag(in PointerEvent e)
    {
        if (e.Kind == PointerEventKind.DragEnd)
        {
            holding = false;
            return;
        }

        int rx = Math.Clamp((int)Math.Floor(e.LocalPosition.X / e.Size.X * Texture.Width / 10) * 10, 0, Texture.Width - 10);
        int ry = Math.Clamp((int)Math.Floor(e.LocalPosition.Y / e.Size.Y * Texture.Height / 10) * 10, 0, Texture.Height - 10);
        if (!holding)
        {
            holding = true;
            start = new Point(rx, ry);
        }

        int lx = Math.Min(start.X, rx);
        int ly = Math.Min(start.Y, ry);
        OnSelect.Invoke(new Rectangle(lx, ly, Math.Max(start.X, rx) - lx + 10, Math.Max(start.Y, ry) - ly + 10));
    }
}
