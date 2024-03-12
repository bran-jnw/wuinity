using System.Numerics;

public static class Vector2Extensions
{
    public static UnityEngine.Vector2 ToUnityVector2(this Vector2 v0)
    {
        return new UnityEngine.Vector2(v0.X, v0.Y);
    }
}
