using System.Collections.Generic;
public class Octree
{
    public uint layers { get; set; }
    public int sizeX { get; set; }
    public int sizeY { get; set; }
    public int sizeZ { get; set; }
    public OctreeNode currentNode { get; set; }
    public Dictionary<int, OctreeNode[]> nodes { get; set; }
}