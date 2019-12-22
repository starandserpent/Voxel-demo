using Godot;
using Godot.Collections;

public struct RawChunk{
    public uint x { get; set; }
    public uint y { get; set; }
    public uint z { get; set; }
    public Array[] arrays {get; set;}
    public Texture[] textures {get; set;}
    public Vector3[][] colliderFaces {get; set;}
}