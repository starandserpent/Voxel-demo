using Math3D;

namespace VoxelOctree{
public struct InputBlock<T>
{
    public T data;
    public Vector3i position;

    public int lod;
    public bool canBeDiscarded;
    public float sortHeuristic;
}
}