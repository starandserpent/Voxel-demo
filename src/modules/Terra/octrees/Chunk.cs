using System;

public ref struct Chunk{
    public int x{get; set;}
    public int y{get; set;}
    public int z{get; set;}

    public Span<byte> voxels{get; set;}
}
