using System;
using System.Collections.Generic;
using Godot;
public class WorldGenerator
{
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

        //Round world size to nearest node lenght
        int playerPosX = ((int)marker.pos.x/16)*16;
        int playerPosY = ((int)marker.pos.y/16)*16;
        int playerPosZ = ((int)marker.pos.z/16)*16;

        CreateNode(0, 0, 0, 0, 0, default(OctreeNode), marker);
    }

    //Procedural generation
    void UpdateSector(float x, float z, float range, LoadMarker trigger) {}

    private OctreeNode CreateNode(int posX, int posY, int posZ, int layer, int type, OctreeNode parentNode, LoadMarker marker) {
        if(layer == 0){
            uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
            Chunk chunk = LoadArea(posX * 16, posY * 16, posZ * 16, marker);
            int lolong = (int) Morton3D.encode(posX, posY, posZ);
            OctreeNode childNode = new OctreeNode();
            childNode.chunk = chunk;
            childNode.locCode = lolong;
            octree.nodes[0].Span[lolong] = childNode;

            if(!octree.nodes.ContainsKey(1)){
                CreateNode(posX, posY, posZ, 1, type, childNode, marker);
            }else{
                parentNode.children.Add(type, childNode);
                return parentNode;
            }
            
        }else if(layer < octree.layers){
            OctreeNode node = new OctreeNode();
            int loccode = (int) Morton3D.encode(posX, posY, posZ);
            node.locCode = loccode;
            node.children = new Dictionary<int, OctreeNode>();

          //  if(layer == 1){
            CubeMesh mesh = new CubeMesh();
            MeshInstance instance = new MeshInstance();
            instance.SetMesh(mesh);
            instance.SetScale(new Vector3(16 * (float) Math.Pow(2, layer - 1), 16 * (float) Math.Pow(2, layer - 1), 16 * (float) Math.Pow(2, layer - 1)));

            instance.SetTranslation(new Vector3(posX * 16 *  (float) Math.Pow(2, layer) + 16 *  (float) Math.Pow(2, layer - 1), posY * 16 *  (float) Math.Pow(2, layer)  + 16 *  (float) Math.Pow(2, layer - 1) , posZ *  16 * (float) Math.Pow(2, layer)  + 16 *  (float) Math.Pow(2, layer - 1)));
            meshInstances.Add(instance);
         //   }

            if(!octree.nodes.ContainsKey(layer)){
                uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
                node.children.Add(type, parentNode);
                octree.nodes.Add(layer, new OctreeNode[size]);
                octree.nodes[layer].Span[loccode] = node;
                node = CreateNode(posX, posY, posZ, layer + 1, type, node, marker);
            }else{
                parentNode.children.Add(type, node);
            }

            for(int i = 0; i < 8; i ++){
                if(!node.children.ContainsKey(i)){
                    switch(i){
                        case 0:
                            node = CreateNode((2 * posX) + 1, (2 * posY) + 1, (2 * posZ) + 1, layer - 1, 0, node, marker);
                        break;

                           case 1:
                             node = CreateNode(2 * posX, (2 * posY) + 1, (2 * posZ) + 1, layer - 1, 1, node, marker);
                        break;

                           case 2:
                             node = CreateNode(2 * posX, (2 * posY) + 1, 2 * posZ, layer - 1, 2, node, marker);
                        break;

                           case 3:
                             node = CreateNode((2 * posX) + 1, (2 * posY) + 1, 2 * posZ, layer - 1, 3, node, marker);
                        break;

                           case 4:
                             node = CreateNode((2 * posX) + 1, (2 * posY) + 1, (2 * posZ) + 1, layer - 1, 4, node, marker);
                        break;

                           case 5:
                             node = CreateNode(2 * posX, 2 * posY, (2 * posZ) + 1, layer - 1, 5, node, marker);
                        break;

                           case 6:
                             node = CreateNode(2 * posX, 2 * posY, 2 * posZ, layer - 1, 6, node, marker);
                        break;

                           case 7:
                             node = CreateNode((2 * posX) + 1, 2 * posY, 2 * posZ, layer - 1, 7, node, marker);
                        break;
                    }
                }
            }
            }
            return parentNode;
        }


    //Loads chunks
    private Chunk LoadArea(float x, float y, float z, LoadMarker marker) {
       /* if (x >= 0 && z >= 0
        && marker.getHardRadius() + marker.pos.x > x 
        && marker.getHardRadius() + marker.pos.y > y
        && marker.getHardRadius() + marker.pos.z > z
        && marker.pos.x - marker.getHardRadius() < x 
        && marker.pos.y - marker.getHardRadius() < y
        && marker.pos.z - marker.getHardRadius() < z) {*/
            Chunk chunk = generator.GetChunk(x, y, z);
            marker.sendChunk(chunk);
            mesher.ChunkLoaded(chunk, false);
            return chunk;
      //  }

        return new Chunk();
    }
}