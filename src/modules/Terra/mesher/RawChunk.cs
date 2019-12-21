using Godot;

public struct RawChunk{
    public uint x { get; set; }
    public uint y { get; set; }
    public uint z { get; set; }
    public Vector3[][] primitives {get; set;}
    public int[] indice {get; set;}
    public int[] arraySize {get; set;}
    public int materials {get; set;}
}