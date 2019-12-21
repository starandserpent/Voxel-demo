using System;
using Godot;
using System.Collections.Generic;

public class Picker
{
    private Terra terra;
    private GameMesher mesher;
    private volatile List<Chunk> chunksToProccess;

    public Picker(Terra terra, GameMesher mesher)
    {
        this.terra = terra;
        this.mesher = mesher;
        chunksToProccess = new List<Chunk>();

        /* ThreadingStart start = new ThreadingStart(ProcessChunks); 
         Threading thread = new Threading(start);
         thread.Start();*/
    }

    public void Pick(Vector3 pos, Vector3 normals)
    {
        float posX = pos.x;
        float posY = pos.y;
        float posZ = pos.z;
        // Chunk chunk = terra.TraverseOctree((int) posX / 16, (int) posY / 16, (int) posZ / 16);

        /* int x = (int) ((posX - chunk.x) * 4);
         int y = (int) ((posY - chunk.y) * 4);
         int z = (int) ((posZ - chunk.z) * 4);
 
         if (normals.y > 0)
         {
             y--;
         }
 
         if (normals.z > 0)
         {
             z--;
         }
 
         //      if(x + (y * 64) + (z * 4096) < chunk.voxels.Length && !chunk.voxels.Span.IsEmpty){
         //        chunk.voxels.Span[x + (y * 64) + (z * 64 * 64)] = 0;
         //      terra.ReplaceChunk((int) posX/16,(int) posY/16,(int) posZ/16,chunk);
         //    ProcessChunks(chunk);
         //}
         */
    }

    private void ProcessChunks(Chunk chunk)
    {
        /*DateTime timeA = DateTime.Now;
        mesher.MeshChunk(chunk, false);
        DateTime timeB = DateTime.Now;
        GD.Print("Picking meshing finished in: " + timeB.Subtract(timeA).Milliseconds + " ms");
        */
    }

    public void Place()
    {
    }
}