using System.Collections.Generic;
using Godot;
using System;

public class OctreeNode
{
    public long locCode { get; set; }
    public Dictionary<int, OctreeNode> children { get; set; }
    public Chunk chunk { get; set; }
}