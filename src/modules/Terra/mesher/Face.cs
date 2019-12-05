using System;
using Godot;
public struct Face{
    public TerraVector3[] vertice{get; set;}
    public TerraVector2[] uvs{get; set;}
    public int side{get; set;}
    public int deleteIndex{get; set;}
    public int CompareTo(Face y){
        return side.CompareTo(y.side);
    }
}