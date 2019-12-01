using System.ComponentModel;
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
        int playerPosX = ((int)marker.GetTranslation().x/16)*16;
        int playerPosY = ((int)marker.GetTranslation().y/16)*16;
        int playerPosZ = ((int)marker.GetTranslation().z/16)*16;

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
            bool newLayer = false;
            OctreeNode node = new OctreeNode();
            int loccode = (int) Morton3D.encode(posX, posY, posZ);
            node.locCode = loccode;
            node.children = new Dictionary<int, OctreeNode>();

          //  if(layer == 1){
            MeshInstance instance = DebugMesh();
            instance.SetScale(new Vector3(32 * (float) Math.Pow(2, layer - 1), 32 * (float) Math.Pow(2, layer - 1), 32 * (float) Math.Pow(2, layer - 1)));
            instance.Name = posX * 16 *  (float) Math.Pow(2, layer) +" " + posY * 16 *  (float) Math.Pow(2, layer) +" " + posZ *  16 * (float) Math.Pow(2, layer);
            instance.SetTranslation(new Vector3(posX * 16 *  (float) Math.Pow(2, layer), posY * 16 *  (float) Math.Pow(2, layer), posZ *  16 * (float) Math.Pow(2, layer)));
            meshInstances.Add(instance);
         //   }

            if(!octree.nodes.ContainsKey(layer)){
                uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
                node.children.Add(type, parentNode);
                octree.nodes.Add(layer, new OctreeNode[size]);
                octree.nodes[layer].Span[loccode] = node;
                newLayer = true;
            }else{
                parentNode.children.Add(type, node);
            }

            for(int i = 0; i < 8; i ++){
                if(!node.children.ContainsKey(i)){
                    switch(i){
                        case 0:
                            node = CreateNode(2 * posX, 2 * posY, 2 * posZ, layer - 1, 0, node, marker);
                        break;

                           case 1:
                             node = CreateNode((2 * posX) + 1, 2 * posY, 2 * posZ, layer - 1, 1, node, marker);
                        break;

                           case 2:
                             node = CreateNode(2 * posX, 2 * posY, (2 * posZ) + 1, layer - 1, 2, node, marker);
                        break;

                           case 3:
                             node = CreateNode((2 * posX) + 1, 2 * posY, (2 * posZ) + 1, layer - 1, 3, node, marker);
                        break;

                           case 4:
                             node = CreateNode(2 * posX, (2 * posY) + 1, 2 * posZ, layer - 1, 4, node, marker);
                        break;

                           case 5:
                             node = CreateNode((2 * posX) + 1, (2 * posY) + 1, 2 * posZ, layer - 1, 5, node, marker);
                        break;

                           case 6:
                             node = CreateNode(2 * posX, (2 * posY) + 1, (2 * posZ) + 1, layer - 1, 6, node, marker);
                        break;

                           case 7:
                             node = CreateNode((2 * posX) + 1, (2 * posY) + 1, (2 * posZ) + 1, layer - 1, 7, node, marker);
                        break;
                    }
                }
            }
            if(newLayer){
                node = CreateNode(posX, posY, posZ, layer + 1, type, node, marker);
            }
            }
            return parentNode;
        }


    //Loads chunks
    private Chunk LoadArea(float x, float y, float z, LoadMarker marker) {
        if (x >= 0 && z >= 0
        && marker.GetHardRadius() + marker.GetTranslation().x > x 
        && marker.GetHardRadius() + marker.GetTranslation().x > y
        && marker.GetHardRadius() + marker.GetTranslation().x > z
        && marker.GetTranslation().x - marker.GetHardRadius() < x 
        && marker.GetTranslation().y - marker.GetHardRadius() < y
        && marker.GetTranslation().z - marker.GetHardRadius() < z) {
            Chunk chunk = generator.GetChunk(x, y, z);
          //  marker.sendChunk(chunk);
            mesher.MeshChunk(chunk, false);
            return chunk;
        }
        return new Chunk();
    }

      private static MeshInstance DebugMesh(){
   SurfaceTool tool = new SurfaceTool();
            tool.Begin(PrimitiveMesh.PrimitiveType.Lines);

            //Front
            tool.AddVertex(new Vector3(0, 0, 0));
            tool.AddVertex(new Vector3(1, 0, 0));
            tool.AddVertex(new Vector3(1, 0, 0));
            tool.AddVertex(new Vector3(1, 1, 0));
            tool.AddVertex(new Vector3(1, 1, 0));
            tool.AddVertex(new Vector3(0, 1, 0));
            tool.AddVertex(new Vector3(0, 1, 0));
            tool.AddVertex(new Vector3(0, 0, 0));

            //Back
            tool.AddVertex(new Vector3(0, 0, 1));
            tool.AddVertex(new Vector3(1, 0, 1));
            tool.AddVertex(new Vector3(1, 0, 1));
            tool.AddVertex(new Vector3(1, 1, 1));
            tool.AddVertex(new Vector3(1, 1, 1));
            tool.AddVertex(new Vector3(0, 1, 1));
            tool.AddVertex(new Vector3(0, 1, 1));
            tool.AddVertex(new Vector3(0, 0, 1));

            //BOTTOM
            tool.AddVertex(new Vector3(0, 0, 0));
            tool.AddVertex(new Vector3(0, 0, 1));
            tool.AddVertex(new Vector3(0, 0, 1));
            tool.AddVertex(new Vector3(1, 0, 1));
            tool.AddVertex(new Vector3(1, 0, 1));
            tool.AddVertex(new Vector3(1, 0, 0));
            tool.AddVertex(new Vector3(1, 0, 0));
            tool.AddVertex(new Vector3(0, 0, 0));

            //TOP
            tool.AddVertex(new Vector3(0, 1, 0));
            tool.AddVertex(new Vector3(0, 1, 1));
            tool.AddVertex(new Vector3(0, 1, 1));
            tool.AddVertex(new Vector3(1, 1, 1));
            tool.AddVertex(new Vector3(1, 1, 1));
            tool.AddVertex(new Vector3(1, 1, 0));
            tool.AddVertex(new Vector3(1, 1, 0));
            tool.AddVertex(new Vector3(0, 1, 0));

            MeshInstance instance = new MeshInstance();
            instance.SetMesh(tool.Commit());
            instance.AddChild(new OmniLight());
            return instance;
      }
}