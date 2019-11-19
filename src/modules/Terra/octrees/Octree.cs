using System;
using System.Collections.Generic;
public struct Octree
{
    public uint layers {get; set;}
    public uint posX {get; set;}
    public uint posY {get; set;}
    public uint posZ {get; set;}
    public Dictionary<int, Memory<OctreeNode>> nodes  {get; set;}
}