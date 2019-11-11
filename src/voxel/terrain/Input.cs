using System.Numerics;
using System.Collections.Generic;
using Math3D;
using Godot;

namespace VoxelOctree{
public class Input<T>
{
    List<InputBlock<T>> Blocks;
    Vector3i PriorityPosition;
    Vector3 PriorityDirection;
    int ExlusiveRegionExtent;
    int ExlusiveRegionMaxLOD;
    bool UseExlusiveRegion;
    int MaxLODIndex;

    public bool IsEmpty(){
        return Blocks.Count == 0;
    }
}
}