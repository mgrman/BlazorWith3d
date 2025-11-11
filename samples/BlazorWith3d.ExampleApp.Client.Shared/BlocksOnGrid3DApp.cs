using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using BlazorWith3d.Shared;
using MemoryPack;


namespace BlazorWith3d.ExampleApp.Client.Shared
{
    [GenerateBinaryApi(typeof(IBlocksOnGrid3DController))]
    public partial interface IBlocksOnGrid3DRenderer
    {
        ValueTask InitializeRenderer(RendererInitializationInfo msg);
        ValueTask<RaycastResponse> InvokeRequestRaycast(RequestRaycast msg);
        ValueTask InvokeAddBlockTemplate(AddBlockTemplate msg);
        ValueTask InvokeAddBlockInstance(AddBlockInstance msg);
        ValueTask InvokeRemoveBlockInstance(RemoveBlockInstance msg);
        ValueTask InvokeRemoveBlockTemplate(RemoveBlockTemplate msg);
        ValueTask InvokeUpdateBlockInstance(int blockId, PackableVector2 position,float rotationZ);
        ValueTask InvokeTriggerTestToBlazor(TriggerTestToBlazor msg);
        ValueTask<PerfCheck> InvokePerfCheck(PerfCheck msg);
        ValueTask<ScreenToWorldRayResponse> InvokeRequestScreenToWorldRay(RequestScreenToWorldRay msg);
    }


    [GenerateTSTypes(typeof(IBlocksOnGrid3DRenderer), "scripts/BlocksOnGrid")]
    [GenerateTSTypesWithMemoryPack(typeof(IBlocksOnGrid3DRenderer))]
    [GenerateBinaryApi(typeof(IBlocksOnGrid3DRenderer))]
    public partial interface IBlocksOnGrid3DController
    {
        ValueTask OnRendererInitialized(RendererInitialized msg, IBlocksOnGrid3DRenderer renderer);
        ValueTask<TestToBlazor> OnTestToBlazor(TestToBlazor msg);
    }
    
    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RendererInitializationInfo 
    {
        public PackableColor BackgroundColor 
#if !UNITY
        { get; set; }
#else
;
#endif
        
        public PackableVector3 RequestedCameraPosition 
#if !UNITY
        { get; set; }
#else
;
#endif
        
        /// <summary>
        /// Euler angles in degrees
        /// </summary>
        public PackableVector3 RequestedCameraRotation 
#if !UNITY
        { get; set; }
#else
;
#endif
        public float RequestedCameraFoV 
#if !UNITY
        { get; set; }
#else
;
#endif
        
        /// <summary>
        /// Euler angles in degrees
        /// </summary>
        public PackableVector3 RequestedDirectionalLightRotation 
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class PerfCheck 
    {
        public float Aaa 
#if !UNITY
        { get; set; }
#else
;
#endif
        public double Bbb
#if !UNITY
        { get; set; }
#else
;
#endif
        public long Ccc
#if !UNITY
        { get; set; }
#else
;
#endif
        public string? Ddd
#if !UNITY
        { get; set; }
#else
;
#endif
        public int Id
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RendererInitialized 
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockTemplate 
    {
        public PackableVector3 Size
#if !UNITY
        { get; set; }
#else
;
#endif
        public int TemplateId
#if !UNITY
        { get; set; }
#else
;
#endif
        public string? Visuals3dUri
#if !UNITY
        { get; set; }
#else
;
#endif
        public string? Visuals2dUri
#if !UNITY
        { get; set; }
#else
;
#endif
    }


    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockInstance 
    {
        public int BlockId
#if !UNITY
        { get; set; }
#else
;
#endif
        public PackableVector2 Position
#if !UNITY
        { get; set; }
#else
;
#endif
        public float RotationZ
#if !UNITY
        { get; set; }
#else
;
#endif
        public int TemplateId
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockInstance 
    {
        public int BlockId
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockTemplate 
    {
        public int TemplateId
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestRaycast 
    {
        public PackableRay Ray
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [Serializable]
    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RaycastResponse 
    {
        public PackableVector3 HitWorld
#if !UNITY
        { get; set; }
#else
;
#endif
        public bool IsBlockHit
#if !UNITY
        { get; set; }
#else
            ;
#endif
        public int HitBlockId
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RequestScreenToWorldRay
    {
        public PackableVector2 Screen
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class ScreenToWorldRayResponse 
    {
        public PackableRay Ray
#if !UNITY
        { get; set; }
#else
;
#endif
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class TriggerTestToBlazor
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class TestToBlazor
    {
        public int Id 
#if !UNITY
        { get; set; }
#else
;
#endif
    }


    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PackableMatrix4x4
    {
        public float M11
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M12
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M13
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M14
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M21
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M22
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M23
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M24
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M31
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M32
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M33
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M34
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M41
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M42
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M43
#if !UNITY
        { get; set; }
#else
;
#endif
        public float M44
#if !UNITY
        { get; set; }
#else
;
#endif


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
    [System.Serializable]
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PackableVector3
    {
        public float X
#if !UNITY
        { get; set; }
#else
;
#endif
        public float Y
#if !UNITY
        { get; set; }
#else
;
#endif
        public float Z
#if !UNITY
        { get; set; }
#else
;
#endif

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
    
    [System.Serializable]
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PackableVector2
    {
        public float X
#if !UNITY
        { get; set; }
#else
;
#endif
        public float Y
#if !UNITY
        { get; set; }
#else
;
#endif

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
    [System.Serializable]
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PackableColor
    {
        public float R 
#if !UNITY
        { get; set; }
#else
;
#endif
        public float G 
#if !UNITY
        { get; set; }
#else
;
#endif
        public float B 
#if !UNITY
        { get; set; }
#else
;
#endif

        public static implicit operator PackableColor(Color vec)
        {
            var @this = new PackableColor();
            @this.R = vec.R / 255f;
            @this.G = vec.G / 255f;
            @this.B = vec.B / 255f;
            return @this;
        }

        public Color ToColor()
        {
            return Color.FromArgb((int)(this.R * 255), (int)(this.G * 255), (int)(this.B * 255));
        }
    }
    [System.Serializable]
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct PackableRay
    {
        public PackableVector3 Origin
#if !UNITY
        { get; set; }
#else
;
#endif
        public PackableVector3 Direction
#if !UNITY
        { get; set; }
#else
;
#endif


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


