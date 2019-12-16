using System.Numerics;
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