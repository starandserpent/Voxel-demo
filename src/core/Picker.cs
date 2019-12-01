using System;
using Godot;
using System.Collections.Generic;

using Threading = System.Threading.Thread;
using ThreadingStart = System.Threading.ThreadStart;
public class Picker
{
    private Terra terra;
    private GameMesher mesher;
    private volatile List<Chunk> chunksToProccess;
    public Picker(Terra terra, GameMesher mesher){
        this.terra = terra;
        this.mesher = mesher;
        chunksToProccess = new List<Chunk>();

        ThreadingStart start = new ThreadingStart(processChunks); 
        Threading thread = new Threading(start);
        thread.Start();
    }

    public void Pick(Vector3 pos, Vector3 normals){
        GD.Print("x: " + pos.x+ "y: "+ pos.y + "z: "+ pos.z);
        float posX = pos.x;
        float posY = pos.y;
        float posZ = pos.z;
        Chunk chunk = terra.traverseOctree((int) posX/16,(int) posY/16,(int) posZ/16);

        int x = (int)((posX - chunk.x) * 4);
        int y = (int)((posY - chunk.y) * 4);
        int z = (int)((posZ - chunk.z) * 4);
        
        if(normals.y > 0){
            y--;
        }
        if(normals.z > 0){
            z--;
        }

        if(x + (y * 64) + (z * 4096) < chunk.voxels.Length && !chunk.voxels.Span.IsEmpty){
            chunk.voxels.Span[x + (y * 64) + (z * 64 * 64)] = 0;
            terra.replaceChunk(chunk);
            chunksToProccess.Add(chunk);
        }
    }

    private void processChunks(){
        while(Threading.CurrentThread.IsAlive){
            if(chunksToProccess.Count > 0){
                Chunk chunk = chunksToProccess[chunksToProccess.Count - 1];
                mesher.MeshChunk(chunk, false);
            }
        }
    }

    public void Place(){
        
    }
}