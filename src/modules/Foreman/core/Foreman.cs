using Godot;
using System;

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

    public void SetMaterials(Registry registry){
        dirtID = registry.SelectByName("dirt").worldID;
        grassID = registry.SelectByName("grass").worldID;
    }

    public Chunk GetChunk(float posX, float posY, float posZ){
        Chunk chunk = new Chunk();
        chunk.x = (uint) posX;
        chunk.y = (uint) posY;
        chunk.z = (uint) posZ;
        
        chunk.voxels = new Memory<byte>(new byte[262144]);

        int posx = (int)(posX / 16);
        int posz = (int)(posZ / 16);
        int posy = (int)(posY * 4);

        chunk.voxels.Span.Fill((byte) 0);

        bool isDifferent = false;

        for (int z = 0; z < 64; z++) {
            for (int x = 0; x < 64; x++) {
                int elevation = (int) Math.Round(weltschmerz.getElevation(x + posx * 64, z + posz * 64));
                for (int y = 0; y < 64; y++) {
                    if ((elevation / 64) > (posy / 64)) {
                         chunk.voxels.Span[x + (y * 64) + (z * 4096)] = (byte) dirtID;
                    } else if (elevation / 64 == (posy / 64)) {
                        if (Math.Abs((elevation % 64)) >= y) {
                            chunk.voxels.Span[x + (y * 64) + (z * 4096)] = (byte) dirtID;
                            isDifferent = true;
                        }
                    }
                }
                if (isDifferent) {
                    chunk.voxels.Span[x + Math.Abs((elevation % 64) * 64) + (z * 4096)] = (byte) grassID;
                   /* if (random.getBoolean()) {
                        blockBuffer.put(x + Math.Abs(((elevation + 1) % 64) * 64) + (z * 4096), (byte) grassMeshID);
                    }*/
                }           
            }
        }
        return chunk;
    }
}
