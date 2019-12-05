using System;
using Godot;

public struct Box
{
    public TerraVector3[][] vertice { get; set; }
    public int[] deleteIndex { get; set; }
}