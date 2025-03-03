namespace Towermap;

public struct Option<T>
where T : unmanaged
{
    public T Value { get; private set; }
    public bool HasValue { get; private set; }

    public Option(T value)
    {
        Value = value;
        HasValue = true;
    }

    public bool TryGetValue(out T value)
    {
        value = Value; // No need for if checks since Value could be default anyway
        return HasValue; // only important here is the HasValue
    }

    public static implicit operator Option<T>(T value)
    {
        return new Option<T>(value);
    }
}