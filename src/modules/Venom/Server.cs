using Godot;
using System;

public class Server
{
    private Registry registry;
    public Server(Client client){
        TerraModule module = new TerraModule("testgame");
        registry = new Registry();
        module.RegisterObjects(registry);
    }
}
