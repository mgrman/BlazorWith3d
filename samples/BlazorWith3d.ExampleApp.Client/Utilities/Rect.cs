using System.Numerics;

namespace BlazorWith3d.ExampleApp.Client.Utilities;

public readonly struct Rect : IEquatable<Rect>
{
    public bool Equals(Rect other)
    {
        return Center.Equals(other.Center) && Size.Equals(other.Size);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Center, Size);
    }

    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect left, Rect right)
    {
        return !left.Equals(right);
    }

    public readonly Vector2 Center;
    public readonly Vector2 Size;
    public readonly Vector2 Extents => Size / 2;

    public Vector2 Min => Center - Extents;
    public Vector2 Max => Center + Extents;

    public Vector2 Corner00 => Center - Extents;
    public Vector2 Corner01 => Center - Extents.NegativeY();
    public Vector2 Corner10 => Center - Extents.NegativeX();
    public Vector2 Corner11 => Center + Extents;
    public float MinX => Center.X - Extents.X;
    public float MinY => Center.Y - Extents.Y;
    public float MaxX => Center.X + Extents.X;
    public float MaxY => Center.Y + Extents.Y;

    public Rect(Vector2 center, Vector2 size)
    {
        Center = center;
        Size = size;
    }

    public static Rect FromCenterSizeAnd90DegRotation(Vector2 center, Vector2 size, float degrees)
    {
        var deg=(int)degrees;

        deg = deg % 360;
        if (deg < 0)
        {
            deg = 360 + deg;
        }

        switch (deg)
        {
            case 0: 
            case 180: 
                break;
            case 90: 
            case 270: 
                size=new  Vector2(size.Y,size.X);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(degrees), degrees, "Only multiples of 90 are allowed for degrees");
        }

        return new Rect(center, size);
    }
    
    public bool IntersectsWith(Rect that)
    {
        var minX = Math.Max(this.MinX, that.MinX);
        var maxX = Math.Min(this.MaxX, that.MaxX);
        var minY = Math.Max(this.MinY, that.MinY);
        var maxY = Math.Min(this.MaxY, that.MaxY);

        return minX<maxX && minY<maxY;
    }

    public bool Contains(Vector2 point)
    {
      return  MinX <= point.X && point.X < MaxX && MinY <= point.Y && point.Y < MaxY;
    }

    public IEnumerable<Rect> GetRectanglesClosestToPoint(Rect targetRect)
    {
        var center = targetRect.Center;
        var size = targetRect.Size;
        
        yield return new Rect(new Vector2(center.X,MinY-size.Y/2), size);
        yield return new Rect(new Vector2(center.X,MaxY+size.Y/2), size);
        yield return new Rect(new Vector2(MinX-size.X/2,center.Y), size);
        yield return new Rect(new Vector2(MaxX+size.X/2,center.Y), size);
    }
    
    
    public IEnumerable<Rect> GetRectanglesAroundCorners(Vector2 size)
    {
        var extents = size / 2;
        
        yield return new Rect(Corner00-extents, size);
        yield return new Rect(Corner00-extents.NegativeY(), size);
        yield return new Rect(Corner00-extents.NegativeX(), size);
        
        yield return new Rect(Corner11+extents, size);
        yield return new Rect(Corner11+extents.NegativeY(), size);
        yield return new Rect(Corner11+extents.NegativeX(), size);
        
        yield return new Rect(Corner01+extents, size);
        yield return new Rect(Corner01-extents, size);
        yield return new Rect(Corner01+extents.NegativeX(), size);
        
        yield return new Rect(Corner10+extents, size);
        yield return new Rect(Corner10-extents, size);
        yield return new Rect(Corner10+extents.NegativeY(), size);
    }
}

public static class VectorExtensions
{
    public static Vector2 NegativeX(this Vector2 vector) => new (-vector.X, vector.Y); 
    public static Vector2 NegativeY(this Vector2 vector) => new (vector.X, -vector.Y); 
    public static Vector2 XY(this Vector3 vector) => new (vector.X, vector.Y); 
}