using System;
using System.Collections.Generic;
public struct Octree
{
    public uint layers {get; set;}
    public uint sizeX {get; set;}
    public uint sizeY {get; set;}
    public uint sizeZ {get; set;}
    public Dictionary<int, Memory<OctreeNode>> nodes  {get; set;}
}