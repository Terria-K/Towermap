namespace Towermap;

public static class MathExtensions 
{
    public static System.Numerics.Vector2 ToNumericsVec2(this MoonWorks.Math.Float.Vector2 vec2) 
    {
        return new System.Numerics.Vector2(vec2.X, vec2.Y);
    }
}