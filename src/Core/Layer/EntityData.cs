using System;
using System.Collections.Generic;
using Cosmiria;

namespace Towermap;

public readonly record struct ActorChange(LevelActor Actor, ActorState Before, string MergeKey);

public struct EntityInspector() : IComponent
{
    public LevelActor Actor;
    public Callback<ActorChange> OnChange;

    private readonly NumberBuffer posX = new(integer: true);
    private readonly NumberBuffer posY = new(integer: true);
    private readonly NumberBuffer width = new(10, 10, 10, integer: true);
    private readonly NumberBuffer height = new(10, 10, 10, integer: true);
    private readonly Dictionary<string, object> fields = [];

    public static EntityInspector For(LevelActor actor)
    {
        var inspector = new EntityInspector { Actor = actor };
        foreach (var (key, value) in actor.CustomData)
        {
            inspector.fields[key] = value switch
            {
                int i => new NumberBuffer(i, integer: true),
                float f => new NumberBuffer(f, 0),
                string s => new TextBuffer(s),
                _ => null
            };
        }
        return inspector;
    }

    public Node Render(UIStack ui, EventStack ev)
    {
        Sync();
        EventHandle apply = ev.Listener(static (ref EntityInspector i) => i.ApplyTransform());

        Node panel = ui.View(Styles.Column).Child([
            ui.Separator(Actor.Data.Name),
            ui.Labeled("ID", ui.Text($"{Actor.ID}")),
            ui.Labeled("X", ui.NumberInput(posX, Styles.Field).OnChange(apply)),
            ui.Labeled("Y", ui.NumberInput(posY, Styles.Field).OnChange(apply)),
            Actor.Data.ResizeableX ? ui.Labeled("Width", ui.NumberInput(width, Styles.Field).OnChange(apply)) : Node.None,
            Actor.Data.ResizeableY ? ui.Labeled("Height", ui.NumberInput(height, Styles.Field).OnChange(apply)) : Node.None,
        ]);

        foreach (var (key, field) in fields)
        {
            EventHandle change = ev.Listener(static (ref EntityInspector i, string k) => i.ApplyField(k), key);
            panel.Child(field switch
            {
                NumberBuffer number => ui.Labeled(key, ui.NumberInput(number, Styles.Field).OnChange(change)),
                TextBuffer text => ui.Labeled(key, ui.TextInput(text, null, Styles.Field).OnChange(change)),
                _ when Actor.CustomData[key] is bool on => ui.Checkbox(on, key).OnClick(change),
                _ => Node.None
            });
        }
        return panel;
    }

    private readonly void Sync()
    {
        posX.Value = Actor.PosX;
        posY.Value = Actor.PosY;
        width.Value = Actor.Width;
        height.Value = Actor.Height;
        foreach (var (key, field) in fields)
        {
            switch (field, Actor.CustomData[key])
            {
            case (NumberBuffer number, int i):
                number.Value = i;
                break;
            case (NumberBuffer number, float f):
                number.Value = f;
                break;
            case (TextBuffer text, string s) when !text.Text.SequenceEqual(s):
                text.Set(s);
                break;
            }
        }
    }

    private void ApplyTransform()
    {
        var before = ActorState.Of(Actor);
        Actor.Transform.PosX = posX.IntValue;
        Actor.Transform.PosY = posY.IntValue;
        if (Actor.Data.ResizeableX)
        {
            Actor.Width = width.IntValue;
        }
        if (Actor.Data.ResizeableY)
        {
            Actor.Height = height.IntValue;
        }
        OnChange.Invoke(new ActorChange(Actor, before, "transform"));
    }

    private void ApplyField(string key)
    {
        var before = ActorState.Of(Actor);
        var data = Actor.CustomData;
        data[key] = (data[key], fields[key]) switch
        {
            (bool on, _) => !on,
            (int, NumberBuffer number) => number.IntValue,
            (float, NumberBuffer number) => number.FloatValue,
            (string, TextBuffer text) => text.ToString(),
            (var value, _) => value
        };
        OnChange.Invoke(new ActorChange(Actor, before, data[key] is bool ? null : key));
    }
}

public struct ConflictList : IComponent
{
    public List<LevelActor> Actors;
    public Callback<LevelActor> OnPick;

    public Node Render(UIStack ui, EventStack ev)
    {
        Node list = ui.View(Styles.Column.Gap(2)).Child(ui.Separator($"Multiple Entities ({Actors.Count})"));
        foreach (var actor in Actors)
        {
            list.Child(ui.Selectable(false, $"{actor.ID} {actor.Data.Name}")
                .OnClick(ev.Listener(static (ref ConflictList c, LevelActor a) => c.OnPick.Invoke(a), actor)));
        }
        return list;
    }
}
