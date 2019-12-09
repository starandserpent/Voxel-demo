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

        chunk.voxels =  pool.Rent(32768/3);

        chunk.x = (uint) posX;
        chunk.y = (uint) posY;
        chunk.z = (uint) posZ;

        chunk.isEmpty = true;

        int posx = (int) (posX * 2);
        int posz = (int) (posZ * 2);
        int posy = (int) (posY * 2);

        int lastPosition = 0;

        bool isDifferent = false;

        for (int z = 0; z < 32; z++)
        {
            for (int x = 0; x < 32; x++)
            {
                int elevation = (int) weltschmerz.getElevation(x + posx, z + posz);

                if (elevation / 32 == (posy / 32))
                {
                    int elev = elevation % 32;

                    uint bitPos = (uint) (elev - 1) << 8;
                    uint bitValue = (uint) dirtID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    bitPos = (uint) 1 << 8;
                    bitValue = (uint) grassID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    bitPos = (uint) (32 - elev) << 8;
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