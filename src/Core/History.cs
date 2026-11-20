using System.Collections.Generic;
using System.Numerics;
using Riateu.Graphics;

namespace Towermap;

public interface IEditCommand
{
    string Name { get; }
    Layers Layer { get; }
    bool IsEmpty { get; }
    void Undo();
    void Redo();
}

public sealed class History 
{
    private readonly List<IEditCommand> commands = [];

    public IReadOnlyList<IEditCommand> Commands => commands;
    public int Position { get; private set; }

    public bool Push(IEditCommand command)
    {
        if (command.IsEmpty)
        {
            return false;
        }
        commands.RemoveRange(Position, commands.Count - Position);
        commands.Add(command);
        Position = commands.Count;
        return true;
    }

    public bool TryMerge(ActorEdit edit)
    {
        if (Position == commands.Count && Position > 0 && commands[^1] is ActorEdit last && last.CanMerge(edit))
        {
            last.After = edit.After;
            return true;
        }
        return false;
    }

    public IEditCommand Undo()
    {
        if (Position == 0)
        {
            return null;
        }
        var command = commands[--Position];
        command.Undo();
        return command;
    }

    public IEditCommand Redo()
    {
        if (Position == commands.Count)
        {
            return null;
        }
        var command = commands[Position++];
        command.Redo();
        return command;
    }

    public void Clear()
    {
        commands.Clear();
        Position = 0;
    }
}

public sealed class GridEdit(GridTiles grid, Layers layer) : IEditCommand
{
    private readonly Dictionary<Point, (bool Before, bool After)> cells = [];

    public GridTiles Grid => grid;
    public string Name
    {
        get
        {
            int placed = 0;
            foreach (var cell in cells.Values)
            {
                if (cell.After)
                    placed++;
            }
            int erased = cells.Count - placed;
            return (placed, erased) switch
            {
                (> 0, 0) => $"{layer}: place {placed}",
                (0, _) => $"{layer}: erase {erased}",
                _ => $"{layer}: place {placed}, erase {erased}"
            };
        }
    }
    public Layers Layer => layer;
    public bool IsEmpty => cells.Count == 0;

    public void Record(int x, int y, bool before, bool after)
    {
        var point = new Point(x, y);
        before = cells.TryGetValue(point, out var cell) ? cell.Before : before;
        if (before == after)
        {
            cells.Remove(point);
            return;
        }
        cells[point] = (before, after);
    }

    public void Undo()
    {
        foreach (var (point, cell) in cells)
        {
            grid.Bits[point.X, point.Y] = cell.Before;
        }
    }

    public void Redo()
    {
        foreach (var (point, cell) in cells)
        {
            grid.Bits[point.X, point.Y] = cell.After;
        }
    }
}

public sealed class TileEdit(Tiles tiles, Layers layer) : IEditCommand
{
    private readonly Dictionary<Point, (int Before, int After)> cells = [];

    public Tiles Tiles => tiles;
    public string Name => $"{layer}: {cells.Count} tile{(cells.Count == 1 ? "" : "s")}";
    public Layers Layer => layer;
    public bool IsEmpty => cells.Count == 0;

    public void Record(int x, int y, int before, int after)
    {
        var point = new Point(x, y);
        before = cells.TryGetValue(point, out var cell) ? cell.Before : before;
        if (before == after)
        {
            cells.Remove(point);
            return;
        }
        cells[point] = (before, after);
    }

    public void Undo()
    {
        foreach (var (point, cell) in cells)
        {
            tiles.SetTile(point.X, point.Y, cell.Before);
        }
    }

    public void Redo()
    {
        foreach (var (point, cell) in cells)
        {
            tiles.SetTile(point.X, point.Y, cell.After);
        }
    }
}

public sealed class SelectionMove(IEditCommand edit, Layers layer, Rectangle before, Rectangle after) : IEditCommand
{
    public Rectangle Before => before;
    public Rectangle After => after;

    public string Name => $"{layer}: move selection";
    public Layers Layer => layer;
    public bool IsEmpty => edit.IsEmpty;

    public void Undo() => edit.Undo();
    public void Redo() => edit.Redo();
}

public sealed class AddActor(Level level, LevelActor actor) : IEditCommand
{
    public string Name => $"Add {actor.Data.Name}";
    public Layers Layer => Layers.Entities;
    public bool IsEmpty => false;

    public void Undo() => level.Actors.Remove(actor);
    public void Redo() => level.Actors.Add(actor);
}

public sealed class RemoveActor(Level level, LevelActor actor, int index) : IEditCommand
{
    public string Name => $"Remove {actor.Data.Name}";
    public Layers Layer => Layers.Entities;
    public bool IsEmpty => false;

    public void Undo() => level.Actors.Insert(System.Math.Min(index, level.Actors.Count), actor);
    public void Redo() => level.Actors.Remove(actor);
}

public readonly record struct ActorState(Vector2 Position, int Width, int Height, Vector2[] Nodes, Dictionary<string, object> CustomData)
{
    public static ActorState Of(LevelActor actor) => new(
        actor.Position,
        actor.Width,
        actor.Height,
        actor.Nodes?.ToArray(),
        new Dictionary<string, object>(actor.CustomData));

    public void ApplyTo(LevelActor actor)
    {
        actor.Position = Position;
        actor.Width = Width;
        actor.Height = Height;
        if (Nodes != null)
        {
            actor.Nodes = [.. Nodes];
        }
        actor.CustomData = new Dictionary<string, object>(CustomData);
    }
}

public sealed class ActorEdit(LevelActor actor, ActorState before, ActorState after, string mergeKey = null) : IEditCommand
{
    public LevelActor Actor => actor;
    public string MergeKey => mergeKey;
    public ActorState After = after;

    public string Name => $"Edit {actor.Data.Name} ({Describe(before, After)})";
    public Layers Layer => Layers.Entities;
    public bool IsEmpty => false;

    public bool CanMerge(ActorEdit other) => mergeKey != null && other.MergeKey == mergeKey && other.Actor == actor;

    public void Undo() => before.ApplyTo(actor);
    public void Redo() => After.ApplyTo(actor);

    private static string Describe(ActorState a, ActorState b) =>
        a.Position != b.Position ? "move"
        : a.Width != b.Width || a.Height != b.Height ? "resize"
        : a.Nodes?.Length != b.Nodes?.Length ? "nodes"
        : "properties";
}
