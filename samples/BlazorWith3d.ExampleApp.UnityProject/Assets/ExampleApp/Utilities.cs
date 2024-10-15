using UnityEngine;

namespace ExampleApp
{
    public static class Utilities
    {
        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
    }
}