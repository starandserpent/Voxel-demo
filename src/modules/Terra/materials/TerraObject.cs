using Godot;
using System;

public class TerraObject
{
    public int worldID { get; set; }
    public string fullName { get; set; }
    public string name { get; }
    public Texture texture { get; }
    public TerraMesh mesh { get; set; }

    public TerraObject(string name, Texture texture)
    {
        this.name = name;
        this.texture = texture;
    }
}