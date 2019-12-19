using Godot;
using System;

public class Client
{
    private Server server;

    public Client(Server server)
    {
        this.server = server;
    }

    public void RecieveChunk(Chunk chunk)
    {
    }
}