using System.Buffers;

public class Foreman
{
    private int dirtID;
    private int grassID;
    private int grassMeshID;
    private Weltschmerz weltschmerz;
    private Registry registry;
    private ArrayPool<uint> pool;
    public Foreman()
    {
        pool = ArrayPool<uint>.Create();
        weltschmerz = new Weltschmerz();
    }

    public void SetMaterials(Registry registry)
    {
        dirtID = registry.SelectByName("dirt").worldID;
        grassID = registry.SelectByName("grass").worldID;
    }

    public Chunk GetChunk(float posX, float posY, float posZ)
    {
        Chunk chunk = new Chunk();

        chunk.voxels =  pool.Rent(262144/3);

        chunk.x = (uint) posX;
        chunk.y = (uint) posY;
        chunk.z = (uint) posZ;

        chunk.isEmpty = true;

        int posx = (int) (posX * 4);
        int posz = (int) (posZ * 4);
        int posy = (int) (posY * 4);

        int lastPosition = 0;

        bool isDifferent = false;

        for (int z = 0; z < 64; z++)
        {
            for (int x = 0; x < 64; x++)
            {
                int elevation = (int) weltschmerz.getElevation(x + posx, z + posz);

                if (elevation / 64 == (posy / 64))
                {
                    int elev = elevation % 64;

                    uint bitPos = (uint) (elev - 1) << 8;
                    uint bitValue = (uint) dirtID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    bitPos = (uint) 1 << 8;
                    bitValue = (uint) grassID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    bitPos = (uint) (64 - elev) << 8;
                    bitValue = (uint) 0;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    isDifferent = true;
                    chunk.isEmpty = false;
                }
            }
        }

        return chunk;
    }
}