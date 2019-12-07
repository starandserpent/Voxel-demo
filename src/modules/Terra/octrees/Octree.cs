using System.Collections.Generic;
public class Octree
{
    public uint layers { get; set; }
    public uint sizeX { get; set; }
    public uint sizeY { get; set; }
    public uint sizeZ { get; set; }
    public Dictionary<int, OctreeNode[]> nodes { get; set; }
}