using Godot;
using System;

public class WorldControler : Node
{
    private Terra terra;
    private PrimitiveResources resources;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Server server = new Server();
        Client client = new Client(server);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
