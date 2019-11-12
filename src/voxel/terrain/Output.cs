using System.Collections.Generic;

namespace VoxelOctree{
public struct Output<T>
{
    public List<OutputBlock<T>> blocks;
    public Stats<T> stats;
}
}