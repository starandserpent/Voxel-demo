using Godot;
using System;

public struct Vector3i
{
    public int x;
    public int y;
    public int z;

    public Vector3i(int xyz){
        this.x = xyz;
        this.y = xyz;
        this.z = xyz;
    }

    public Vector3i(int px, int py, int pz){
        this.x = px;
        this.y = py;
        this.z = pz;
    }

     public Vector3i(Vector3i other){
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public Vector3i(Vector3 vector3f){
        this.x = (int) Math.Floor(vector3f.x);
        this.y = (int) Math.Floor(vector3f.y);
        this.z = (int) Math.Floor(vector3f.z);
    }

    public static Vector3i empty(){
        Vector3i vector3I;
        vector3I.x = 0;
        vector3I.y = 0;
        vector3I.z = 0;
        return vector3I;
    }

    public Vector3 ToVector3(){
        return new Vector3(x, y, z);
    }

    public int Volume(){
        return x * y * z;
    }

    public double Lenght(){
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public void Add(Vector3i vector3i){
        this.x += vector3i.x;
        this.y += vector3i.y;
        this.z += vector3i.z;
    }

    public void Deduct(Vector3i vector3i){
        this.x -= vector3i.x;
        this.y -= vector3i.y;
        this.z -= vector3i.z;
    }
    public Vector3i Reverse(){
        return new Vector3i(-x, -y, -z);
    }

    public void ClampTo(Vector3i min, Vector3i max){
        //MIN
        if(x < min.x)
            x = min.x;
        if(y < min.y)
            y = min.y;
        if(z < min.z)
            z = min.z;

        //MAX
        if(x >= max.x)
            x = max.x;
        if(y >= max.y)
            y = max.y;
        if(z >= max.z)
            z = max.z;
    }

    public bool IsContainedIn(Vector3i min, Vector3i max){
        return x >= min.x && y >= min.y && z >= min.z && x < max.x && y < max.y && z < max.z;
    }

    public int GetIndex(Vector3i areaSize){
        return y + areaSize.y * (x + areaSize.x * z);
    }

    public bool AreValuesEqual(){
        return x == y && y == z;
    }

    public static Vector3i GetVectorFromIndex(int i, Vector3i areaSize){
        Vector3i pos;
        pos.y = i % areaSize.y;
        pos.x = (i / areaSize.y) % areaSize.x;
        pos.z = i / (areaSize.y * areaSize.x);
        return pos;
     }

    public override bool Equals(object obj)
    {
        return obj is Vector3i i &&
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

    public static Vector3i operator +(Vector3i a, Vector3i b){
        return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3i operator +(Vector3i a, int b){
        return new Vector3i(a.x + b, a.y + b, a.z + b);
    }

    public static Vector3i operator -(Vector3i a, Vector3i b){
        return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3i operator -(Vector3i a, int b){
        return new Vector3i(a.x - b, a.y - b, a.z - b);
    }

    public static Vector3i operator *(Vector3i a, Vector3i b){
        return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3i operator *(Vector3i a, int b){
        return new Vector3i(a.x * b, a.y * b, a.z * b);
    }

    public static Vector3i operator /(Vector3i a, Vector3i b){
        return new Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);
    }

     public static Vector3i operator /(Vector3i a, int b){
        return new Vector3i(a.x / b, a.y / b, a.z / b);
    }


    public static bool operator ==(Vector3i a, Vector3i b){
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Vector3i a, Vector3i b){
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static Vector3i operator << (Vector3i a, int b){
        return new Vector3i(a.x << b, a.y << b, a.z << b);
    }

    public static Vector3i operator >>(Vector3i a, int b){
        return new Vector3i(a.x >> b, a.y >> b, a.z >> b);
    }

    public static Vector3i operator %(Vector3i a, Vector3i b){
        return new Vector3i(a.x % b.x, a.y % b.y, a.z % b.z);
    }

    public static bool operator <(Vector3i a, Vector3i b){
        if (a.x == b.x) {
		    if (a.y == b.y) {
		    	return a.z < b.z;
		    } else {
		    	return a.y < b.y;
		    }
	    } else {
		    return a.x < b.x;
	    }  
    }
     public static bool operator >(Vector3i a, Vector3i b){
        if (a.x == b.x) {
		    if (a.y == b.y) {
		    	return a.z > b.z;
		    } else {
		    	return a.y > b.y;
		    }
	    } else {
		    return a.x > b.x;
	    }          
    }

    public static void sortMinMaxVector(ref Vector3i min, ref Vector3i max){
        SortMinMax(ref min.x, ref max.x);
        SortMinMax(ref min.y, ref max.y);
        SortMinMax(ref min.z, ref max.z);
    }
    
    //[min, max]
    private static void SortMinMax(ref int min, ref int max){
        if(min > max){
            int temp = min;
            min = max;
            max = min;
        }
     }

}
