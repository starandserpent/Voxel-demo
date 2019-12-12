
using System.Collections.Generic;

public class Terra
{
    // Declare member variables here. Examples:
    private volatile Octree octree;
    public Terra(uint sizeX, uint sizeY, uint sizeZ)
    {
        octree = new Octree();
        octree.sizeX = sizeX;
        octree.sizeY = sizeY;
        octree.sizeZ = sizeZ;

        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
        octree.layers = (uint) Utils.calculateLayers(size);

        octree.nodes = new Dictionary<int, OctreeNode[]>();
        octree.nodes[0] = new OctreeNode[size];
    }

    public Octree GetOctree(){
        return octree;
    }

    public Chunk TraverseOctree(int posX, int posY, int posZ)
    {
      /*  if (posX >= 0 && posY >= 0 && posZ >= 0)
        {
            int lolong = (int) Morton3D.encode(posX, posY, posZ);
            OctreeNode node = octree.nodes[0][lolong];
            return node.chunk;
        }*/

        return default(Chunk);
    }

    public void ReplaceChunk(int posX, int posY, int posZ, Chunk chunk)
    {
        /*int lolong = (int) Morton3D.encode(posX, posY, posZ);
        OctreeNode node = octree.nodes[0][lolong];
        node.chunk = chunk;
        octree.nodes[0][lolong] = node;*/
    }
}