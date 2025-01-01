using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;

using BlazorWith3d.Shared;
using MemoryPack;


namespace BlazorWith3d.ExampleApp.Client.Shared
{
    internal class MemoryPackBinaryApiSerializer:IBinaryApiSerializer
    {
        public void SerializeObject<T>(T obj, IBufferWriter<byte> writer)
        {
            MemoryPackSerializer.Serialize<T, IBufferWriter<byte>>(writer, obj);
        }

        public T? DeserializeObject<T>(ReadOnlySpan<byte> bytes)
        {
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
    }
    
#if COMMON_DOTNET || UNITY_EDITOR
    [Blazor3DApp(typeof(MemoryPackBinaryApiSerializer))]
    public partial interface IBlocksOnGrid3DApp
    {
    }
#endif
    
//#if COMMON_UNITY
    [Unity3DApp(typeof(MemoryPackBinaryApiSerializer))]
    public partial interface IBlocksOnGridUnityApi
    {
    }
//#endif

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlazorControllerInitialized : IMessageToUnity
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class PerfCheck : IMessageToUnity<PerfCheck>
    {
        public float Aaa { get; set; }
        public double Bbb{ get; set; }
        public long Ccc{ get; set; }
        public string? Ddd{ get; set; }
        public int Id{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class UnityAppInitialized : IMessageToBlazor
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockTemplate : IMessageToUnity
    {
        public PackableVector3 Size{ get; set; }
        public int TemplateId{ get; set; }
        public string? VisualsUri{ get; set; }
    }


    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockInstance : IMessageToUnity
    {
        public int BlockId{ get; set; }
        public PackableVector2 Position{ get; set; }
        public float RotationZ{ get; set; }
        public int TemplateId{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockInstance : IMessageToUnity
    {
        public int BlockId{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockTemplate : IMessageToUnity
    {
        public int TemplateId{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class UpdateBlockInstance : IMessageToUnity
    {
        public int BlockId{ get; set; }
        public PackableVector2 Position{ get; set; }
        public float RotationZ{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestRaycast : IMessageToUnity<RaycastResponse>
    {
        public PackableRay Ray{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RaycastResponse 
    {
        public PackableVector3 HitWorld{ get; set; }
        public int? HitBlockId{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestScreenToWorldRay : IMessageToUnity<ScreenToWorldRayResponse>
    {
        public PackableVector2 Screen{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class ScreenToWorldRayResponse 
    {
        public PackableRay Ray{ get; set; }
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class TriggerTestToBlazor: IMessageToUnity
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class TestToBlazor: IMessageToBlazor<TestToBlazor> 
    {
        public int Id { get; set; }
    }


    [MemoryPackable]
    [GenerateTypeScript]
    public partial struct PackableMatrix4x4
    {
        public float M11{ get; set; }
        public float M12{ get; set; }
        public float M13{ get; set; }
        public float M14{ get; set; }
        public float M21{ get; set; }
        public float M22{ get; set; }
        public float M23{ get; set; }
        public float M24{ get; set; }
        public float M31{ get; set; }
        public float M32{ get; set; }
        public float M33{ get; set; }
        public float M34{ get; set; }
        public float M41{ get; set; }
        public float M42{ get; set; }
        public float M43{ get; set; }
        public float M44{ get; set; }


        public static implicit operator PackableMatrix4x4(Matrix4x4 matrix)
        {
            var @this = new PackableMatrix4x4();
            @this.M11 = matrix.M11;
            @this.M12 = matrix.M12;
            @this.M13 = matrix.M13;
            @this.M14 = matrix.M14;
            @this.M21 = matrix.M21;
            @this.M22 = matrix.M22;
            @this.M23 = matrix.M23;
            @this.M24 = matrix.M24;
            @this.M31 = matrix.M31;
            @this.M32 = matrix.M32;
            @this.M33 = matrix.M33;
            @this.M34 = matrix.M34;
            @this.M41 = matrix.M41;
            @this.M42 = matrix.M42;
            @this.M43 = matrix.M43;
            @this.M44 = matrix.M44;
            return @this;
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
        public float X{ get; set; }
        public float Y{ get; set; }
        public float Z{ get; set; }

        public static implicit operator PackableVector3(Vector3 vec)
        {
            var @this = new PackableVector3();
            @this.X = vec.X;
            @this.Y = vec.Y;
            @this.Z = vec.Z;
            return @this;
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
        public float X{ get; set; }
        public float Y{ get; set; }

        public static implicit operator PackableVector2(Vector2 vec)
        {
            var @this = new PackableVector2();
            @this.X = vec.X;
            @this.Y = vec.Y;
            return @this;
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
        public PackableVector3 Origin{ get; set; }
        public PackableVector3 Direction{ get; set; }


        public static implicit operator PackableRay(Ray ray)
        {
            var @this = new PackableRay();
            @this.Origin = ray.Origin;
            @this.Direction = ray.Direction;
            return @this;
        }

        public Ray ToRay()
        {
            return new Ray(
                this.Origin.ToVector3(),
                this.Direction.ToVector3()
            );
        }
    }
}


