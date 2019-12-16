using System.Numerics;
using System;
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
        public static BoundingRect AABBtoScreenRect(AABB box, Godot.Camera cam){
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
        public static bool insideFrustum(Godot.Collections.Array planes, AABB box)
        {
            Godot.Vector3 half_extents = box.size.toGDVector3() * 0.5f;
            Godot.Vector3 ofs = box.center.toGDVector3();

            for (int i = 0; i < planes.Count; i++)
            {
                Godot.Plane p = (Godot.Plane)planes[i];
                Godot.Vector3 point = new Godot.Vector3(
                (p.Normal.x <= 0) ? -half_extents.x : half_extents.x,
                (p.Normal.y <= 0) ? -half_extents.y : half_extents.y,
                (p.Normal.z <= 0) ? -half_extents.z : half_extents.z);
                point += ofs;
                if (p.Normal.Dot(point) > p.D)
                {
                    return false;
                }
            }
            return true;
        }
    }
