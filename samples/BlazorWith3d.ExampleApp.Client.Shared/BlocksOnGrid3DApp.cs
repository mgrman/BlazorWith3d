using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

using BlazorWith3d.Shared;
using MemoryPack;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
#if COMMON_DOTNET || UNITY_EDITOR
    [Blazor3DApp(
#if UNITY_EDITOR
            true
#endif
    )]
    public partial class BlocksOnGrid3DApp
    {
        protected partial void SerializeObject<T>(T obj, IBufferWriter<byte> writer)
        {
            MemoryPackSerializer.Serialize<T, IBufferWriter<byte>>(writer, obj);
        }

        protected partial T? DeserializeObject<T>(ReadOnlySpan<byte> bytes)
        {
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
#endif


//#if COMMON_UNITY
    [Unity3DApp]
    public partial class BlocksOnGridUnityApi
    {
        protected partial void SerializeObject<T>(T obj, IBufferWriter<byte> writer)
        {
            MemoryPackSerializer.Serialize<T, IBufferWriter<byte>>(writer, obj);
        }

        protected partial T? DeserializeObject<T>(ReadOnlySpan<byte> bytes)
        {
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
//#endif

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlazorControllerInitialized : IMessageToUnity
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class PerfCheck : IMessageToUnity, IMessageToBlazor
    {
        public float Aaa;
        public double Bbb;
        public long Ccc;
        public string? Ddd;
        public int Id;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class UnityAppInitialized : IMessageToBlazor
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial struct PackableMatrix4x4
    {
        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;


        public PackableMatrix4x4(Matrix4x4 matrix)
        {
            this.M11 = matrix.M11;
            this.M12 = matrix.M12;
            this.M13 = matrix.M13;
            this.M14 = matrix.M14;
            this.M21 = matrix.M21;
            this.M22 = matrix.M22;
            this.M23 = matrix.M23;
            this.M24 = matrix.M24;
            this.M31 = matrix.M31;
            this.M32 = matrix.M32;
            this.M33 = matrix.M33;
            this.M34 = matrix.M34;
            this.M41 = matrix.M41;
            this.M42 = matrix.M42;
            this.M43 = matrix.M43;
            this.M44 = matrix.M44;
        }

        public Matrix4x4 ToMatrix4x4()
        {
            return new Matrix4x4(
                this.M11,
                this.M12,
                this.M13,
                this.M14,
                this.M21,
                this.M22,
                this.M23,
                this.M24,
                this.M31,
                this.M32,
                this.M33,
                this.M34,
                this.M41,
                this.M42,
                this.M43,
                this.M44
            );
        }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial struct PackableVector3
    {
        public float X;
        public float Y;
        public float Z;

        public PackableVector3(Vector3 vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
            this.Z = vec.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(
                this.X,
                this.Y,
                this.Z
            );
        }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial struct PackableVector2
    {
        public float X;
        public float Y;

        public PackableVector2(Vector2 vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(
                this.X,
                this.Y
            );
        }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial struct PackableRay
    {
        public PackableVector3 Origin;
        public PackableVector3 Direction;
        
        public PackableRay(Ray ray)
        {
            this.Origin = new PackableVector3(ray.Origin);
            this.Direction = new PackableVector3(ray.Direction);
        }

        public Ray ToRay()
        {
            return new Ray(
                this.Origin.ToVector3(),
                this.Direction.ToVector3()
            );
        }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockTemplate : IMessageToUnity
    {
        public PackableVector3 Size;
        public int TemplateId;
        public string? VisualsUri;
    }


    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockInstance : IMessageToUnity
    {
        public int BlockId;
        public PackableVector2 Position;
        public float RotationZ;
        public int TemplateId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockInstance : IMessageToUnity
    {
        public int BlockId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockTemplate : IMessageToUnity
    {
        public int TemplateId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class UpdateBlockInstance : IMessageToUnity
    {
        public int BlockId;
        public PackableVector2 Position;
        public float RotationZ;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestRaycast : IMessageToUnity
    {
        public int RequestId;
        public PackableRay Ray;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RaycastResponse : IMessageToBlazor
    {
        public int RequestId;
        public PackableVector3 HitWorld;
        public int? HitBlockId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestScreenToWorldRay : IMessageToUnity
    {
        public int RequestId;
        public PackableVector2 Screen;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class ScreenToWorldRayResponse : IMessageToBlazor
    {
        public int RequestId;
        public PackableRay Ray;
    }

}


