using Godot;
using System;

public class Client
{
    private Server server;
    public Client(Server server, GameControler controler){
        this.server = server;
    }

    public void recieveChunk(Chunk chunk){

    }
}
