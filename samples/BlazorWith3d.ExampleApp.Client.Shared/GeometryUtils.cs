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
            // based on https://stackoverflow.com/a/23976134
            float denom = Vector3.Dot(plane.Normal, ray.Direction);
            if (Math.Abs(denom) > 0.0001f) // your favorite epsilon
            {
                float t = Vector3.Dot((plane.Normal * plane.D) - ray.Origin, plane.Normal) / denom;
                if (t >= 0) return ray.Origin + ray.Direction * t; // you might want to allow an epsilon here too
            }

            return null;
        }
    }
}