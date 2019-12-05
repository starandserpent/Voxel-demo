using Godot;
using System;

public struct TerraVector3
{
    public float x;
    public float y;
    public float z;

    public TerraVector3(float xyz)
    {
        this.x = xyz;
        this.y = xyz;
        this.z = xyz;
    }

    public TerraVector3(float px, float py, float pz)
    {
        this.x = px;
        this.y = py;
        this.z = pz;
    }

    public TerraVector3(TerraVector3 other)
    {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public static TerraVector3 empty()
    {
        TerraVector3 vector3I;
        vector3I.x = 0;
        vector3I.y = 0;
        vector3I.z = 0;
        return vector3I;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public float Volume()
    {
        return x * y * z;
    }

    public double Lenght()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public void Add(TerraVector3 vector3i)
    {
        this.x += vector3i.x;
        this.y += vector3i.y;
        this.z += vector3i.z;
    }

    public void Deduct(TerraVector3 vector3i)
    {
        this.x -= vector3i.x;
        this.y -= vector3i.y;
        this.z -= vector3i.z;
    }

    public TerraVector3 Reverse()
    {
        return new TerraVector3(-x, -y, -z);
    }

    public void ClampTo(TerraVector3 min, TerraVector3 max)
    {
        //MIN
        if (x < min.x)
            x = min.x;
        if (y < min.y)
            y = min.y;
        if (z < min.z)
            z = min.z;

        //MAX
        if (x >= max.x)
            x = max.x;
        if (y >= max.y)
            y = max.y;
        if (z >= max.z)
            z = max.z;
    }

    public bool IsContainedIn(TerraVector3 min, TerraVector3 max)
    {
        return x >= min.x && y >= min.y && z >= min.z && x < max.x && y < max.y && z < max.z;
    }

    public float GetIndex(TerraVector3 areaSize)
    {
        return y + areaSize.y * (x + areaSize.x * z);
    }

    public bool AreValuesEqual()
    {
        return x == y && y == z;
    }

    public static TerraVector3 GetVectorFromIndex(int i, TerraVector3 areaSize)
    {
        TerraVector3 pos;
        pos.y = i % areaSize.y;
        pos.x = (i / areaSize.y) % areaSize.x;
        pos.z = i / (areaSize.y * areaSize.x);
        return pos;
    }

    public override bool Equals(object obj)
    {
        return obj is TerraVector3 i &&
               x == i.x &&
               y == i.y &&
               z == i.z;
    }

    public override int GetHashCode()
    {
        var hashCode = 373119288;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    public static TerraVector3 operator +(TerraVector3 a, TerraVector3 b)
    {
        return new TerraVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static TerraVector3 operator +(TerraVector3 a, int b)
    {
        return new TerraVector3(a.x + b, a.y + b, a.z + b);
    }

    public static TerraVector3 operator -(TerraVector3 a, TerraVector3 b)
    {
        return new TerraVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static TerraVector3 operator -(TerraVector3 a, int b)
    {
        return new TerraVector3(a.x - b, a.y - b, a.z - b);
    }

    public static TerraVector3 operator *(TerraVector3 a, TerraVector3 b)
    {
        return new TerraVector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static TerraVector3 operator *(TerraVector3 a, int b)
    {
        return new TerraVector3(a.x * b, a.y * b, a.z * b);
    }

    public static TerraVector3 operator /(TerraVector3 a, TerraVector3 b)
    {
        return new TerraVector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static TerraVector3 operator /(TerraVector3 a, int b)
    {
        return new TerraVector3(a.x / b, a.y / b, a.z / b);
    }

    public static bool operator ==(TerraVector3 a, TerraVector3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(TerraVector3 a, TerraVector3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static TerraVector3 operator %(TerraVector3 a, TerraVector3 b)
    {
        return new TerraVector3(a.x % b.x, a.y % b.y, a.z % b.z);
    }

    public static TerraVector3 operator >>(TerraVector3 a, int b)
    {
        return new TerraVector3((float) ((int) a.x >> b), (float) ((uint) a.y >> b), (float) ((uint) a.z >> b));
    }

    public static bool operator <(TerraVector3 a, TerraVector3 b)
    {
        if (a.x == b.x)
        {
            if (a.y == b.y)
            {
                return a.z < b.z;
            }
            else
            {
                return a.y < b.y;
            }
        }
        else
        {
            return a.x < b.x;
        }
    }

    public static bool operator >(TerraVector3 a, TerraVector3 b)
    {
        if (a.x == b.x)
        {
            if (a.y == b.y)
            {
                return a.z > b.z;
            }
            else
            {
                return a.y > b.y;
            }
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