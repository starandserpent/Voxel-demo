using System;
public class Rect3i
{
     Vector3i pos;
     Vector3i size;

    public Rect3i(){}

    public Rect3i(Vector3i pos, Vector3i size){
        this.pos = pos;
        this.size = size;
    }

    //ox = origin x
    public Rect3i(int ox, int oy, int oz, int sx, int sy, int sz){
        pos = new Vector3i(ox, oy, oz);
        size = new Vector3i(sz, sy, sz);
    }

    public Rect3i (Rect3i other){
        pos = new Vector3i(other.pos);
        size = new Vector3i(other.size);
    }

    public static Rect3i FromCenterExtents(Vector3i center, Vector3i extents){
        return new Rect3i(center - extents, extents * 2);
    }

    public static Rect3i FromMinMax(Vector3i min, Vector3i max){
        return new Rect3i(min, max - min);
    }

    public static Rect3i GetBoundingBox(Rect3i a, Rect3i b){
        Rect3i box = new Rect3i();

		box.pos.x = Math.Min(a.pos.x, b.pos.x);
		box.pos.y = Math.Min(a.pos.y, b.pos.y);
		box.pos.z = Math.Min(a.pos.z, b.pos.z);

		Vector3i max_a = a.pos + a.size;
		Vector3i max_b = b.pos + b.size;

		box.size.x = Math.Max(max_a.x, max_b.x) - box.pos.x;
		box.size.y = Math.Max(max_a.y, max_b.y) - box.pos.y;
		box.size.z = Math.Max(max_a.z, max_b.z) - box.pos.z;

		return box;  
    }

    public bool Contains(Vector3i pos){
        Vector3i end = this.pos + size;
		return this.pos.x >= pos.x &&
			   this.pos.y >= pos.y &&
			   this.pos.z >= pos.z &&
			   pos.x < end.x &&
			   pos.y < end.y &&
			   pos.z < end.z; 
    }

    public override String ToString(){
		return "(o:" + pos.ToVector3() + ", s:" + size.ToVector3() + " )";
	}

    public bool Intersects(Rect3i other) {
		if (pos.x > other.pos.x + other.size.x) {
			return false;
		}
		if (pos.y > other.pos.y + other.size.y) {
			return false;
		}
		if (pos.z > other.pos.z + other.size.z) {
			return false;
		}
		if (other.pos.x > pos.x + size.x) {
			return false;
		}
		if (other.pos.y > pos.y + size.y) {
			return false;
		}
		if (other.pos.z > pos.z + size.z) {
			return false;
		}
		return true;
	}

    public void ForEachCell(Func<Vector3i, Vector3i> a) {
		Vector3i max = pos + size;
		Vector3i p;
		for (p.z = pos.z; p.z < max.z; ++p.z) {
			for (p.y = pos.y; p.y < max.y; ++p.y) {
				for (p.x = pos.x; p.x < max.x; ++p.x) {
				    a(p);
				}
			}
		}
	}

	public bool AllCellsMatch(Func<Vector3i, bool> a) {
		Vector3i max = pos + size;
		Vector3i p;
		for (p.z = pos.z; p.z < max.z; ++p.z) {
			for (p.y = pos.y; p.y < max.y; ++p.y) {
				for (p.x = pos.x; p.x < max.x; ++p.x) {
					if (!a(p)) {
						return false;
					}
				}
			}
		}
		return true;
	}

	public void Difference(Rect3i b, Func<Rect3i, bool> action){

		if (!Intersects(b)) {
			action(this);
			return;
		}

		Rect3i a = this;

		Vector3i a_min = a.pos;
		Vector3i b_min = b.pos;
		Vector3i a_max = a.pos + a.size;
		Vector3i b_max = b.pos + b.size;

		if (a_min.x < b_min.x) {
			Vector3i a_rect_size = new Vector3i(b_min.x - a_min.x, a.size.y, a.size.z);
			action(new Rect3i(a_min, a_rect_size));
			a_min.x = b_min.x;
			a.pos.x = b.pos.x;
			a.size.x = a_max.x - a_min.x;
		}
		if (a_min.y < b_min.y) {
			Vector3i a_rect_size = new Vector3i(a.size.x, b_min.y - a_min.y, a.size.z);
			action(new Rect3i(a_min, a_rect_size));
			a_min.y = b_min.y;
			a.pos.y = b.pos.y;
			a.size.y = a_max.y - a_min.y;
		}
		if (a_min.z < b_min.z) {
			Vector3i a_rect_size = new Vector3i(a.size.x, a.size.y, b_min.z - a_min.z);
			action(new Rect3i(a_min, a_rect_size));
			a_min.z = b_min.z;
			a.pos.z = b.pos.z;
			a.size.z = a_max.z - a_min.z;
		}

		if (a_max.x > b_max.x) {
			Vector3i a_rect_pos = new Vector3i(b_max.x, a_min.y, a_min.z);
			Vector3i a_rect_size= new Vector3i(a_max.x - b_max.x, a.size.y, a.size.z);
			action(new Rect3i(a_rect_pos, a_rect_size));
			a_max.x = b_max.x;
			a.size.x = a_max.x - a_min.x;
		}
		if (a_max.y > b_max.y) {
			Vector3i a_rect_pos = new Vector3i(a_min.x, b_max.y, a_min.z);
			Vector3i a_rect_size = new Vector3i(a.size.x, a_max.y - b_max.y, a.size.z);
			action(new Rect3i(a_rect_pos, a_rect_size));
			a_max.y = b_max.y;
			a.size.y = a_max.y - a_min.y;
		}
		if (a_max.z > b_max.z) {
			Vector3i a_rect_pos = new Vector3i(a_min.x, a_min.y, b_max.z);
			Vector3i a_rect_size = new Vector3i(a.size.x, a.size.y, a_max.z - b_max.z);
			action(new Rect3i(a_rect_pos, a_rect_size));
		}
	}
     Rect3i Padded(int m){
		return new Rect3i(
				pos.x - m,
				pos.y - m,
				pos.z - m,
				size.x + 2 * m,
				size.y + 2 * m,
				size.z + 2 * m);
	}

	 public Rect3i Downscaled(int step_size) {
		Rect3i o = new Rect3i;
		o.pos = pos.udiv(step_size);
		Vector3i max_pos = (pos + size - new Vector3i(1)).udiv(step_size);
		o.size = max_pos - o.pos + new Vector3i(1);
		return o;
	}

	public void Clip(Rect3i lim) {
	    Vector3i max_pos = lim.pos + lim.size;
		pos.ClampTo(lim.pos, max_pos);
		size = Vector3i::min(size, max_pos - pos);
	}

    public static bool operator !=(Rect3i a, Rect3i b) {
	    return a.pos != b.pos || a.size != b.size;
    }

     public static bool operator ==(Rect3i a, Rect3i b) {
	    return a.pos == b.pos && a.size == b.size;
    }
}
