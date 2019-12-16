    using System.Numerics;
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