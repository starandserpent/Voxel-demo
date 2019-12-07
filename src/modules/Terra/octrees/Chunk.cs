public struct Chunk
{
    public uint x { get; set; }
    public uint y { get; set; }
    public uint z { get; set; }

    public uint[] voxels { get; set; }

    public bool isEmpty { get; set; }
}