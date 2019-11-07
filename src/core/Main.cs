using Godot;
using System;
public class Main : Node
{
    public override void _Ready()
    {
        FastNoise entity = ResourceLoader.Load("res://src/libraries/fastnoise.gdns") as fastnoise;
        GD.Print();
    }
}