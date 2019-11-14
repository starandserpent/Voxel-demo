using System;

public struct Chunk{
    public int x{get; set;}
    public int y{get; set;}
    public int z{get; set;}

    public Memory<byte> voxels{get; set;}
}
