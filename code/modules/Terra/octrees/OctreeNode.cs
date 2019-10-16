using Godot;
using System;

public struct OctreeNode
{
    private uint x {get; set;}
    private uint y {get; set;}
    private uint z {get; set;}
    private OctreeNode[] children{get; set;}
}
