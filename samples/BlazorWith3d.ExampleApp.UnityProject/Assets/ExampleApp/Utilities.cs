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

        public static PackableMatrix4x4 ToPackableMatrix4x4(this Matrix4x4 m)
        {
            var pm = new PackableMatrix4x4();
            pm.M11 = m.m00;
            pm.M12 = m.m01;
            pm.M13 = m.m02;
            pm.M14 = m.m03;
            pm.M21 = m.m10;
            pm.M22 = m.m11;
            pm.M23 = m.m12;
            pm.M24 = m.m13;
            pm.M31 = m.m20;
            pm.M32 = m.m21;
            pm.M33 = m.m22;
            pm.M34 = m.m23;
            pm.M41 = m.m30;
            pm.M42 = m.m31;
            pm.M43 = m.m32;
            pm.M44 = m.m33;
            return pm;
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

        public static PackableVector2 ToPackable(this System.Numerics.Vector2 m)
        {
            var pm = new PackableVector2();
            pm.X = m.X;
            pm.Y = m.Y;
            return pm;
        }

        public static PackableVector3 ToPackable(this System.Numerics.Vector3 m)
        {
            var pm = new PackableVector3();
            pm.X = m.X;
            pm.Y = m.Y;
            pm.Z = m.Z;
            return pm;
        }

        public static PackableRay ToPackable(this BlazorWith3d.ExampleApp.Client.Shared.Ray m)
        {
            var pm = new PackableRay(m);
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