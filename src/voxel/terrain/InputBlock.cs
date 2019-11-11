using Math3D;

namespace VoxelOctree{
public struct InputBlock<T>
{
    T data;
    Vector3i position;

    int lod;
    bool CanBeDiscarded;
    float SortHeuristic;
}
}