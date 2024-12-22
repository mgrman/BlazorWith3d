using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BlazorWith3d.ExampleApp.Client.Shared
{

    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public static Ray Transform(Ray ray, Matrix4x4 matrix)
        {
            var rayOrigin = Vector3.Transform(ray.Origin, matrix);
            var rayDirection = Vector3.TransformNormal(ray.Direction, matrix);
            return new Ray(rayOrigin, rayDirection);
        }

    }

    public static class GeometryUtils
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3? Raycast(this Plane plane, Ray ray)
        {
            float directionDot = Vector3.Dot(ray.Direction, plane.Normal);
            float normalDot = -Vector3.Dot(ray.Origin, plane.Normal) - plane.D;

            if (Math.Abs(directionDot) < 0.000001f)
            {
                return null;
            }

            var enter = normalDot / directionDot;

            return ray.Origin + ray.Direction * enter;
        }
    }
}