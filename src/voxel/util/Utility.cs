using System.Collections.Generic;
using Godot;

namespace Util{
public class Utility
{
    Mesh debugBoxMesh;
    public static void shiftUp<T>(List<T> l, int pos){
        int j = 0;
        for (int i = pos; i < l.Count; ++i, ++j){
            l[j] = l[i];
        }

        l.RemoveRange(pos, l.Count);
    }

    public static void UnorderedRemove<T>(List<T> list, int pos){
        int last = list.Count - 1;
        list[pos] = list[last];
        
        list.RemoveAt(last);
    }

    public static float interpolate(float v0, float v1, float v2,
     float v3, float v4, float v5, float v6, float v7, Vector3 pos){
        float minX = 1f - pos.x;
        float minY = 1f - pos.y;
        float minZ = 1f - pos.z;

        float OXOY = minX * minY;
        float XOY = pos.x * minY;

        float res = minZ * (v0 * OXOY + v1 * XOY + v4 * minX * pos.y);
	    res += pos.z * (v3 * OXOY + v2 * XOY + v7 * minX * pos.y);
	    res += pos.x * pos.y * (v5 * minZ + v6 * pos.z);

	    return res;
    }

    public static float min(float a, float b) {
	    return a < b ? a : b;
    }

    public static float max(float a, float b) {
	    return a > b ? a : b;
    }

   /* public static bool IsSurfaceTriangulated(Array surface) {
	    PoolVector3Array positions = surface[Mesh::ARRAY_VERTEX];
	    PoolIntArray indices = surface[Mesh::ARRAY_INDEX];
	    return positions.size() >= 3 && indices.size() >= 3;
    }
    */

/*public static bool IsMeshEmpty(ref Mesh mesh) {
	if (mesh == null)
		return true;
	Mesh mesh = **mesh_ref;
	if (mesh.GetSurfaceCount() == 0)
		return true;
	if (mesh. == 0)
		return true;
	return false;
}
*/

public static int udiv(int x, int d) {
	if (x < 0) {
		return (x - d + 1) / d;
	} else {
		return x / d;
	}
}

// `Math::wrapi` with zero min
    public static int wrap(int x, int d) {
	    return ((x % d) + d) % d;
    }
}
}