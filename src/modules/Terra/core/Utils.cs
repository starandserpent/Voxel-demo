using System.Numerics;
using System;

public class Utils
{
    private const int WORDBITS = 32;

    public static uint FloorLog2(uint x)
    {
        x |= (x >> 1);
        x |= (x >> 2);
        x |= (x >> 4);
        x |= (x >> 8);
        x |= (x >> 16);

        return (uint) (NumBitsSet(x) - 1);
    }

    public static uint CeilingLog2(uint x)
    {
        int y = (int) (x & (x - 1));

        y |= -y;
        y >>= (WORDBITS - 1);
        x |= (x >> 1);
        x |= (x >> 2);
        x |= (x >> 4);
        x |= (x >> 8);
        x |= (x >> 16);

        return (uint) (NumBitsSet(x) - 1 - y);
    }

    public static int NumBitsSet(uint x)
    {
        x -= ((x >> 1) & 0x55555555);
        x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
        x = (((x >> 4) + x) & 0x0f0f0f0f);
        x += (x >> 8);
        x += (x >> 16);

        return (int) (x & 0x0000003f);
    }

    public static double calculateLayers(uint size)
    {
        return FloorLog2(size) / 3;
    }
}
public struct BoundingRect{
        public Vector2 loc;
        public Vector2 extent;

        public float x{
            get {return loc.X;}
            set {loc.X = x;}
        }
        public float y{
            get {return loc.Y;}
            set {loc.Y = y;}
        }
        public float width{
            get {return extent.X;}
            set {extent.X = width;}
        }
        public float height{
            get {return extent.Y;}
            set {extent.Y = height;}
        }
        public BoundingRect(Vector2 loc,Vector2 extent){
            this.loc = loc;
            this.extent =  Vector2.Abs(loc - extent);
        }
        public BoundingRect(float x, float y, float width, float height){
            this.loc.X = x; this.loc.Y = y;
            this.extent.X = width; this.extent.Y = height;
        }
        public static bool IntersectsRect(BoundingRect rect1, BoundingRect rect2){
            return (rect1.x < rect2.x + rect2.width &&
                    rect1.x + rect1.width > rect2.x &&
                    rect1.y < rect2.y + rect2.height &&
                    rect1.y + rect1.height > rect2.y);
        }
        public override string ToString(){
            return "X: "+x+", Y: "+y+", Width: "+extent.X+", Height: "+extent.Y;
        }
    }
    public struct BoundSphere{
        public Vector3 position;
        public float radius;
        public BoundSphere(Vector3 position, float radius){
            this.position = position;
            this.radius = radius;
        }
        
        public bool intersectsSphere(BoundSphere other){
            float distance = Vector3.Distance(this.position,other.position);
            return distance < (this.radius+other.radius);
        }
        public bool intersectsAABB(AABB box){
            // get box closest point to sphere center by clamping
            Vector3 closPoint = Vector3.Max(box.min,Vector3.Min(this.position,box.max));
            
            // this is the same as isPointInsideSphere
            float distance = Vector3.Distance(closPoint,position);
            return distance < radius;
        }
        
    }
    public struct AABB
    {
        public Vector3 min;
        public Vector3 max;
        public Vector3 size;
        public Vector3 center;

        public AABB(Vector3 min, Vector3 max){
            this.min = min;
            this.max = max;
            this.size = max - min;
            this.size.X = Math.Abs(this.size.X);
            this.size.Y = Math.Abs(this.size.Y);
            this.size.Z = Math.Abs(this.size.Z);
            this.center = min+(size/2.0f);
        }
        public bool isPointInsideAABB(Vector3 point)
        {
            return (point.X >= min.X && point.X <= max.X) &&
            (point.Y >= min.Y && point.Y <= max.Y) &&
            (point.Z >= min.Z && point.Z <= max.Z);
        }
        public bool intersectAABB(AABB a)
        {
            return (min.X <= a.max.X && max.X >= a.min.X) &&
            (min.Y <= a.max.Y && max.Y >= a.min.Y) &&
            (min.Z <= a.max.Z && max.Z >= a.min.Z);
        }
        public static unsafe BoundingRect AABBtoScreenRect(AABB box, Godot.Camera cam){
            Vector2 origin = cam.UnprojectPosition(box.min.toGDVector3()).toNumericVector2();
            Vector2 extent = cam.UnprojectPosition(box.max.toGDVector3()).toNumericVector2();
            
            return new BoundingRect(origin,extent);
        }
        

    }
    public static class GDExtension{
        public const float DEG2RAD = 3.141593f / 180;
        public static Vector2 toNumericVector2(this Godot.Vector2 vec){
            return new Vector2(vec.x,vec.y);
        }
        public static Godot.Vector3 toGDVector3(this Vector3 vec3){
            return new Godot.Vector3(vec3.X,vec3.Y,vec3.Z);
        }
        public static Vector3 toNumericVector3(Godot.Vector3 vec){
            Vector3 val = new Vector3(vec.x,vec.y,vec.z);
            return val;
        }

        public static Godot.Plane xFromPlane(this Godot.Transform transform, ref Godot.Plane plane){
            Godot.Vector3 point = plane.Normal * plane.D;
            Godot.Vector3 point_dir = point + plane.Normal;
            point = transform.Xform(point);
            point_dir = transform.Xform(point_dir);

            Godot.Vector3 normal = point_dir - point;
            normal.Normalized();
            float d =normal.Dot(point);
            plane.Normal = normal;
            plane.D = d;
            return new Godot.Plane(normal,d);
        }
        public static Matrix4x4 multiplyColMaj(this Matrix4x4 value1, Matrix4x4 value2){
            Matrix4x4 m;
            // First Column
            m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
            m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
            m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
            m.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;

            // Second Column
            m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
            m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
            m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
            m.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;

            // Third Column
            m.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
            m.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
            m.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
            m.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;

            // Fourth Column
            m.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;
            m.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;
            m.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;
            m.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

            return m;
        }
    }
