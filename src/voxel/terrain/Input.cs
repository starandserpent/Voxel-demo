using System.Collections.Generic;
using Math3D;
using Godot;

namespace VoxelOctree{
public class Input<T>
{
    public List<InputBlock<T>> blocks;
    public Vector3i priorityPosition;
    public Vector3 priorityDirection;
    public int exlusiveRegionExtent;
    public int exlusiveRegionMaxLOD;
    public bool useExlusiveRegion;
    public int maxLODIndex;

    public bool IsEmpty(){
        return blocks.Count == 0;
    }
}
}