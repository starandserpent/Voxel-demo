using System.Linq;
using System.Collections.Generic;
using System;
using Godot;

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
      
        DateTime timeA = DateTime.Now;

        Dictionary<int, uint> surface = new Dictionary<int, uint>();

        Chunk chunk = new Chunk();
        chunk.x = (uint) posX;
        chunk.y = (uint) posY;
        chunk.z = (uint) posZ;
        
        chunk.isEmpty = true;

        int posx = (int)(posX / 16);
        int posz = (int)(posZ / 16);
        int posy = (int)(posY * 4);

        bool isDifferent = false;

        for (int z = 0; z < 64; z++) {
            for (int x = 0; x < 64; x++) {

                int elevation = (int) weltschmerz.getElevation((int)(x + posx * 64), (int)(z + posz * 64));
                
                   if (elevation / 64 == (posy / 64)) {

                            int pos = (elevation % 64) + x * 64 + (z * 4096);

                            uint bitPos = (uint) 1 << 8;
                            uint bitValue = (uint)grassID;

                            surface.Add(pos, bitPos | bitValue);
                            isDifferent = true;
                            chunk.isEmpty = false;

                            if(surface.ContainsKey(pos - 1)){
                                long b = 255;
                                long value = surface[pos - 1] & b;
                                if(value == grassID){                              
                                    long a = 16777215 << 8;
                                    uint index = (uint)(surface[pos - 1] & a) >> 8;
                                    surface[pos] = surface[pos] + (index << 8);
                                    surface.Remove(pos - 1);
                                }
                            }else {
                                bitPos = (uint) (((pos/64)*64) - 1) << 8;
                                bitValue = (uint)dirtID;

                                surface.Add(pos - 1, bitPos | bitValue);
                            }
                    }
                }
            }


        DateTime timeB = DateTime.Now;
        GD.Print("Meshing finished in: " + timeB.Subtract(timeA).Milliseconds +" ms");
            chunk.voxels = new Memory<uint>(surface.Values.ToArray());

            surface.Clear();
        return chunk;
    }
}
