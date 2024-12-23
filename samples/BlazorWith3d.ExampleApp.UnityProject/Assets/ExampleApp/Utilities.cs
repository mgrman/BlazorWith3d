using BlazorWith3d.ExampleApp.Client.Shared;

using UnityEngine;

using Ray = UnityEngine.Ray;

namespace ExampleApp
{
    public static class Utilities
    {
        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ToUnity(this PackableVector2 m)
        {
            var pm = new Vector2();
            pm.x = m.X;
            pm.y = m.Y;
            return pm;
        }

        public static Vector3 ToUnity(this PackableVector3 m)
        {
            var pm = new Vector3();
            pm.x = m.X;
            pm.y = m.Y;
            pm.z = m.Z;
            return pm;
        }

        public static Ray ToUnity(this PackableRay m)
        {
            var pm = new Ray(m.Origin.ToUnity(), m.Direction.ToUnity());
            return pm;
        }

        public static System.Numerics.Vector2 ToNumerics(this Vector2 m)
        {
            var pm = new System.Numerics.Vector2();
            pm.X = m.x;
            pm.Y = m.y;
            return pm;
        }

        public static System.Numerics.Vector3 ToNumerics(this Vector3 m)
        {
            var pm = new System.Numerics.Vector3();
            pm.X = m.x;
            pm.Y = m.y;
            pm.Z = m.z;
            return pm;
        }

        public static BlazorWith3d.ExampleApp.Client.Shared.Ray ToNumerics(this Ray m)
        {
            var pm = new BlazorWith3d.ExampleApp.Client.Shared.Ray(m.origin.ToNumerics(), m.direction.ToNumerics());
            return pm;
        }
    }
}