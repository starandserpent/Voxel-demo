using Godot;
using System;
using System.Memory;

public class Foreman
{
    private int dirtID;
    private int grassID;
    private int grassMeshID;
    private Weltschmerz weltschmerz;
    private Registry registry;
    public Foreman (){
        weltschmerz = new Weltschmerz();
    }

    public void SetMaterials(TerraModule module, Registry registry){
        dirtID = registry.SelectByName(module, "dirt").worldID;
        grassID = registry.SelectByName(module, "grass").worldID;
    }

    public Chunk GetChunk(float posX, float posY, float posZ){
        Chunk chunk = new Chunk();
        chunk.X = posX;
        chunk.Y = posY;
        chunk.Z = posZ;

        Span<int> voxels

        chunk.ids


        return chunk;
    }
}
