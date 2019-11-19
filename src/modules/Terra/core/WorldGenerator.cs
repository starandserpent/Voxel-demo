using System.Linq;
using System;
using System.Collections.Generic;
using Godot;
public class WorldGenerator
{
    //private readonly OffheapOctree octree;
   //This is recommend max static octree size because it takes 134 MB
    private static readonly int MAX_OCTREE_NODE_SIZE = 256;
    private static readonly int MAX_OCTANT_LAYERS = 4;
    private GameMesher mesher;
    private List<MeshInstance> meshInstances;
    private Octree octree;
    private Foreman generator;

    public WorldGenerator(Octree octree, GameMesher mesher, Foreman generator) {
        this.generator = generator;
        this.mesher = mesher;
        this.octree = octree;
    }

    public void SetMeshInstaces(List<MeshInstance> instances){
        this.meshInstances = instances;
    }

    //Initial generation
    public void SeekSector(LoadMarker marker) {

        //Round world size to nearest node length
        octree.posX = 32;
        octree.posY = 32;
        octree.posZ = 32;

        uint size = octree.posX * octree.posY * octree.posZ;
        octree.layers = (uint) Utils.calculateLayers(size);

        octree.nodes = new Dictionary<int, Memory<OctreeNode>>();
        octree.nodes[0]= new OctreeNode[size];

        int playerPosX = ((int)marker.pos.x/16)*16;
        int playerPosY = ((int)marker.pos.y/16)*16;
        int playerPosZ = ((int)marker.pos.z/16)*16;

        CreateNode(playerPosX, playerPosY, playerPosZ, 0, 0, default(OctreeNode), marker);
    }

    //Procedural generation
    void UpdateSector(float x, float z, float range, LoadMarker trigger) {}

    private void CreateNode(int posX, int posY, int posZ, int layer, int type, OctreeNode parentNode, LoadMarker marker) {
        if(layer == 0){
            uint size = octree.posX * octree.posY * octree.posZ;
            Chunk chunk = LoadArea(posX * 16, posY * 16, posZ * 16, marker);
            int lolong = (int) Morton3D.encode(posX, posY, posZ);
            OctreeNode childNode = new OctreeNode();
            childNode.chunk = chunk;
            childNode.locCode = lolong;
            mesher.ChunkLoaded(chunk, false);
            octree.nodes[0].Span[lolong] = childNode;

            if(!octree.nodes.ContainsKey(1)){
                CreateNode(posX, posY, posZ, layer + 1, type, childNode, marker);
            }else{
                parentNode.children[type] = childNode;
            }
        }else if(layer < octree.layers){
            OctreeNode node = new OctreeNode();
            int loccode = (int) Morton3D.encode(posX, posY, posZ);
            node.locCode = loccode;
            node.children = new Dictionary<int, OctreeNode>();

            if(!octree.nodes.ContainsKey(layer)){
                uint size = octree.posX * octree.posY * octree.posZ;
                OctreeNode clone = parentNode;
                node.children.Add(type, clone);
                octree.nodes.Add(layer, new OctreeNode[size]);
                octree.nodes[layer].Span[loccode] = node;
                CreateNode(posX, posY, posZ, layer + 1, type, node, marker);
            }else{
                parentNode.children[type] = node;
            }

            for(int i = 0; i < 8; i ++){
                if(!node.children.ContainsKey(i)){
                    switch(i){
                        case 0:
                            CreateNode(posX, posY, posZ, layer - 1, 0, node, marker);
                        break;
                           case 1:
                            CreateNode(posX + 1, posY, posZ, layer - 1, 1, node, marker);
                        break;
                           case 2:
                            CreateNode(posX + 1, posY, posZ + 1, layer - 1, 2, node, marker);

                        break;
                           case 3:
                            CreateNode(posX, posY, posZ + 1, layer - 1, 3, node, marker);

                        break;
                           case 4:
                            CreateNode(posX, posY + 1, posZ, layer - 1, 4, node, marker);

                        break;
                           case 5:
                            CreateNode(posX + 1, posY + 1, posZ, layer - 1, 5, node, marker);

                        break;
                           case 6:
                            CreateNode(posX + 1, posY + 1, posZ + 1, layer - 1, 6, node, marker);

                        break;
                           case 7:
                            CreateNode(posX, posY + 1, posZ + 1, layer - 1, 7, node, marker);
                        break;
                    }
                }
            }
            }
        }


    //Loads chunks
    private Chunk LoadArea(float x, float y, float z, LoadMarker marker) {
        if (x >= 0 && z >= 0) {
            Chunk chunk = generator.GetChunk(x, y, z);
            marker.sendChunk(chunk);
            return chunk;
        }

        return new Chunk();
    }
}