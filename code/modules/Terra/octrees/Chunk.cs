using Godot;
using System;
using System.Memory;

public struct Chunk
{
    public int X{get; set;}
    public int Y{get; set;}
    public int Z{get; set;}

    public Span<int> voxels{get; set;}
}
