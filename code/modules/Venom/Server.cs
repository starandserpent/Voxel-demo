using Godot;
using System;

public class Server
{
    private Registry registry;
    public Server(){
        TerraModule module = new TerraModule("testgame");
        registry = new Registry();
        PrimitiveResources resources = new PrimitiveResources();
    }
}