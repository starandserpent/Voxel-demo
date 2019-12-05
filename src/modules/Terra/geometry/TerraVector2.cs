using Godot;
using System;

public struct TerraVector2
{
    public float x;
    public float y;

    public TerraVector2(float xy)
    {
        this.x = xy;
        this.y = xy;
    }

    public TerraVector2(float px, float py)
    {
        this.x = px;
        this.y = py;
    }

    public TerraVector2(TerraVector2 other)
    {
        this.x = other.x;
        this.y = other.y;
    }

    public static TerraVector2 empty()
    {
        TerraVector2 vector3I;
        vector3I.x = 0;
        vector3I.y = 0;
        return vector3I;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public float Volume()
    {
        return x * y;
    }

    public double Lenght()
    {
        return Math.Sqrt(x * x + y * y);
    }

    public void Add(TerraVector2 vector3i)
    {
        this.x += vector3i.x;
        this.y += vector3i.y;
    }

    public void Deduct(TerraVector2 vector3i)
    {
        this.x -= vector3i.x;
        this.y -= vector3i.y;
    }

    public TerraVector2 Reverse()
    {
        return new TerraVector2(-x, -y);
    }

    public void ClampTo(TerraVector2 min, TerraVector2 max)
    {
        //MIN
        if (x < min.x)
            x = min.x;
        if (y < min.y)
            y = min.y;

        //MAX
        if (x >= max.x)
            x = max.x;
        if (y >= max.y)
            y = max.y;
    }

    public bool IsContainedIn(TerraVector2 min, TerraVector2 max)
    {
        return x >= min.x && y >= min.y && x < max.x && y < max.y;
    }

    public float GetIndex(TerraVector2 areaSize)
    {
        return y + areaSize.y * (x + areaSize.x);
    }

    public bool AreValuesEqual()
    {
        return x == y;
    }

    public static TerraVector2 GetVectorFromIndex(int i, TerraVector2 areaSize)
    {
        TerraVector2 pos;
        pos.y = i % areaSize.y;
        pos.x = (i / areaSize.y) % areaSize.x;
        return pos;
    }

    public override bool Equals(object obj)
    {
        return obj is TerraVector2 i &&
               x == i.x &&
               y == i.y;
    }

    public override int GetHashCode()
    {
        var hashCode = 373119288;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        return hashCode;
    }

    public static TerraVector2 operator +(TerraVector2 a, TerraVector2 b)
    {
        return new TerraVector2(a.x + b.x, a.y + b.y);
    }

    public static TerraVector2 operator +(TerraVector2 a, int b)
    {
        return new TerraVector2(a.x + b, a.y + b);
    }

    public static TerraVector2 operator -(TerraVector2 a, TerraVector2 b)
    {
        return new TerraVector2(a.x - b.x, a.y - b.y);
    }

    public static TerraVector2 operator -(TerraVector2 a, int b)
    {
        return new TerraVector2(a.x - b, a.y - b);
    }

    public static TerraVector2 operator *(TerraVector2 a, TerraVector2 b)
    {
        return new TerraVector2(a.x * b.x, a.y * b.y);
    }

    public static TerraVector2 operator *(TerraVector2 a, int b)
    {
        return new TerraVector2(a.x * b, a.y * b);
    }

    public static TerraVector2 operator /(TerraVector2 a, TerraVector2 b)
    {
        return new TerraVector2(a.x / b.x, a.y / b.y);
    }

    public static TerraVector2 operator /(TerraVector2 a, int b)
    {
        return new TerraVector2(a.x / b, a.y / b);
    }


    public static bool operator ==(TerraVector2 a, TerraVector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(TerraVector2 a, TerraVector2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static TerraVector2 operator %(TerraVector2 a, TerraVector2 b)
    {
        return new TerraVector2(a.x % b.x, a.y % b.y);
    }

    public static bool operator <(TerraVector2 a, TerraVector2 b)
    {
        if (a.x == b.x)
        {
            return a.y < b.y;
        }
        else
        {
            return a.x < b.x;
        }
    }

    public static bool operator >(TerraVector2 a, TerraVector2 b)
    {
        if (a.x == b.x)
        {
            return a.y > b.y;
        }
        else
        {
            return a.x > b.x;
        }
    }

    //[min, max]
    private static void SortMinMax(ref int min, ref int max)
    {
        if (min > max)
        {
            int temp = min;
            min = max;
            max = min;
        }
    }
}