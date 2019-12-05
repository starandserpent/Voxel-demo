using System;
using Godot;
public class Face : IComparable<Face>{
    public Vector3[] vertice{get; set;}
    public Vector2[] uvs{get; set;}
    public int side{get; set;}
    public bool isDeleted{get; set;}
    public int CompareTo(Face y){
        return side.CompareTo(y.side);
    }
}