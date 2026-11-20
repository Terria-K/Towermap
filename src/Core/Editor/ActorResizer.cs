using System;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public sealed class ActorResizer
{
    [Flags]
    private enum Axis { None = 0, X = 1, Y = 2 }

    private const float Grab = 3;
    private const float HandleSize = 4;

    private LevelActor actor;
    private Axis axis;
    private ActorState before;

    public bool Active => actor != null;

    public bool TryStart(LevelActor selected, Vector2 mouse)
    {
        axis = HitTest(selected, mouse);
        if (axis == Axis.None)
        {
            return false;
        }
        actor = selected;
        before = ActorState.Of(selected);
        return true;
    }

    public void Update(Vector2 mouse)
    {
        Vector2 topLeft = actor.Position - actor.Data.Origin;
        if (axis.HasFlag(Axis.X))
        {
            actor.Width = Math.Max(10, (int)MathF.Round((mouse.X - topLeft.X) / 10) * 10);
        }
        if (axis.HasFlag(Axis.Y))
        {
            actor.Height = Math.Max(10, (int)MathF.Round((mouse.Y - topLeft.Y) / 10) * 10);
        }
    }

    public void End(EditorScene scene)
    {
        if (actor.Width != before.Width || actor.Height != before.Height)
        {
            scene.RecordActor(actor, before);
        }
        actor = null;
    }

    public static void Draw(Batch batch, LevelActor selected)
    {
        Vector2 topLeft = selected.Position - selected.Data.Origin;
        Vector2 size = selected.Size;
        Vector2 half = new Vector2(HandleSize * 0.5f);
        if (selected.Data.ResizeableX)
        {
            DrawHandle(batch, topLeft + new Vector2(size.X, size.Y * 0.5f) - half);
        }
        if (selected.Data.ResizeableY)
        {
            DrawHandle(batch, topLeft + new Vector2(size.X * 0.5f, size.Y) - half);
        }
        if (selected.Data.ResizeableX && selected.Data.ResizeableY)
        {
            DrawHandle(batch, topLeft + size - half);
        }
    }

    private static void DrawHandle(Batch batch, Vector2 position)
    {
        DrawUtils.Rect(batch, position - Vector2.One, Color.Black, new Vector2(HandleSize + 2), Vector2.Zero);
        DrawUtils.Rect(batch, position, Color.White, new Vector2(HandleSize), Vector2.Zero);
    }

    private static Axis HitTest(LevelActor selected, Vector2 mouse)
    {
        Vector2 topLeft = selected.Position - selected.Data.Origin;
        Vector2 bottomRight = topLeft + selected.Size;
        bool withinX = mouse.X >= topLeft.X - Grab && mouse.X <= bottomRight.X + Grab;
        bool withinY = mouse.Y >= topLeft.Y - Grab && mouse.Y <= bottomRight.Y + Grab;

        Axis axis = Axis.None;
        if (selected.Data.ResizeableX && withinY && MathF.Abs(mouse.X - bottomRight.X) <= Grab)
        {
            axis |= Axis.X;
        }
        if (selected.Data.ResizeableY && withinX && MathF.Abs(mouse.Y - bottomRight.Y) <= Grab)
        {
            axis |= Axis.Y;
        }
        return axis;
    }
}
