using Godot;
using Godot.Collections;

public struct RawChunk
{
    public Array[] arrays { get; set; }
    public Texture[] textures { get; set; }
    public Vector3[][] colliderFaces { get; set; }
}