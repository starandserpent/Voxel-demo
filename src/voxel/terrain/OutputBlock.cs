using Math3D;

namespace VoxelOctree{
public class OutputBlock<T>
{
    T Data;
    Vector3i Position;
    int Lod = 0;
    bool DropHint;
}
}