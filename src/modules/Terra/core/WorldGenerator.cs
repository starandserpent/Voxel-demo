using System.Diagnostics;
using System;
using System.Collections.Generic;
using Godot;

public class WorldGenerator
{
    //This is recommend max static octree size because it takes 134 MB
    private static readonly int MAX_OCTREE_NODE_SIZE = 256;
    private static readonly int MAX_OCTANT_LAYERS = 4;
    private GameMesher mesher;
    private Octree octree;
    private Node parent;
    private Foreman generator;
    private bool debug;
    private List<long>[] debugMeasures;
    public WorldGenerator(Node parent, Octree octree, GameMesher mesher, Foreman generator, bool debug)
    {
        this.generator = generator;
        this.mesher = mesher;
        this.octree = octree;
        this.parent = parent;
        this.debug = debug;
        debugMeasures = new List<long>[3];
        debugMeasures[0] = new List<long>();
    }

    //Initial generation
    public void SeekSector(LoadMarker marker)
    {
        //Round world size to nearest node lenght
        int playerPosX = (int) ((marker.GetTranslation().x / Constants.CHUNK_LENGHT));
        int playerPosY = (int) ((marker.GetTranslation().y / Constants.CHUNK_LENGHT));
        int playerPosZ = (int) ((marker.GetTranslation().z / Constants.CHUNK_LENGHT));

        Translation translation = new Translation();

        for(int y = 0; y < marker.hardRadius; y++){
            for(int z = 0; z < marker.hardRadius; z++){
                for(int x = 0; x < marker.hardRadius; x++){
                    LoadArea(playerPosX + x, playerPosY + y, playerPosZ + z, marker);
                    LoadArea(playerPosX - x, playerPosY + y, playerPosZ + z, marker);
                    LoadArea(playerPosX + x, playerPosY + y, playerPosZ - z, marker);
                    LoadArea(playerPosX - x, playerPosY - y, playerPosZ - z, marker);
                    LoadArea(playerPosX + x, playerPosY - y, playerPosZ - z, marker);
                    LoadArea(playerPosX - x, playerPosY - y, playerPosZ + z, marker);
             }
        }     
        }
    }

    //Procedural generation
    public void UpdateSector(LoadMarker marker)
    {
        //Round world size to nearest node lenght
        int playerPosX = (int) ((marker.GetTranslation().x / Constants.CHUNK_LENGHT));
        int playerPosY = (int) ((marker.GetTranslation().y / Constants.CHUNK_LENGHT));
        int playerPosZ = (int) ((marker.GetTranslation().z / Constants.CHUNK_LENGHT));

        for(int y = 0; y < marker.hardRadius; y++){
            for(int z = 0; z < marker.hardRadius; z++){
                for(int x = 0; x < marker.hardRadius; x++){
                    LoadArea(playerPosX + x, playerPosY + y, playerPosZ + z, marker);
                    LoadArea(playerPosX - x, playerPosY + y, playerPosZ + z, marker);
                    LoadArea(playerPosX + x, playerPosY + y, playerPosZ - z, marker);
                    LoadArea(playerPosX - x, playerPosY - y, playerPosZ - z, marker);
                    LoadArea(playerPosX + x, playerPosY - y, playerPosZ - z, marker);
                    LoadArea(playerPosX - x, playerPosY - y, playerPosZ + z, marker);
             }
        }     
        }
    }
    private void Connect(int posX, int posY, int posZ, int layer, OctreeNode node){
        int parentNodePosX = (int)(posX/2);
        int parentNodePosY = (int)(posY/2);
        int parentNodePosZ = (int)(posZ/2);

        int lolong = (int) Morton3D.encode(parentNodePosX, parentNodePosY, parentNodePosZ);
        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
        if(lolong < size && layer < octree.layers){
                        MeshInstance instance = DebugMesh();
            instance.Scale = new Vector3(16 * (float) Math.Pow(2, layer - 1), 16 * (float) Math.Pow(2, layer - 1),
                16 * (float) Math.Pow(2, layer - 1));
            instance.Name = posX * 8 * (float) Math.Pow(2, layer) + " " + posY * 8 * (float) Math.Pow(2, layer) +
                            " " + posZ * 8 * (float) Math.Pow(2, layer);
            instance.Translation = new Vector3(posX * 8 * (float) Math.Pow(2, layer),
                posY * 8 * (float) Math.Pow(2, layer), posZ * 8 * (float) Math.Pow(2, layer));
            parent.AddChild(instance);
        
            OctreeNode parentNode;
            if(octree.nodes.ContainsKey(layer)){
                if(octree.nodes[layer][lolong] != default(OctreeNode)){
                  parentNode = octree.nodes[layer][lolong];
                }else{
                    parentNode = new OctreeNode();
                    parentNode.locCode = lolong;
                    parentNode.children = new Dictionary<int, OctreeNode>();
                    octree.nodes[layer][lolong] = parentNode;
                }
            }else{
                octree.nodes[layer] = new OctreeNode[size];
                parentNode = new OctreeNode();
                parentNode.locCode = lolong;
                parentNode.children = new Dictionary<int, OctreeNode>();
                octree.nodes[layer][lolong] = parentNode;
            }

        if(posX % 2 == 0 && posX % 2 == 0 && posX % 2 == 0){
            parentNode.children[0] = node;
        }else if(posX % 2 == 1 && posX % 2 == 0 && posX % 2 == 0){
            parentNode.children[1] = node;
        }else if(posX % 2 == 0 && posX % 2 == 0 && posX % 2 == 1){
            parentNode.children[2] = node;
        }else if(posX % 2 == 1 && posX % 2 == 0 && posX % 2 == 1){
            parentNode.children[3] = node;
        }else if(posX % 2 == 0 && posX % 2 == 1 && posX % 2 == 0){
            parentNode.children[4] = node;
        }else if(posX % 2 == 1 && posX % 2 == 1 && posX % 2 == 0){
            parentNode.children[5] = node;
        }else if(posX % 2 == 0 && posX % 2 == 1 && posX % 2 == 1){
            parentNode.children[6] = node;
        }else if(posX % 2 == 1 && posX % 2 == 1 && posX % 2 == 1){
            parentNode.children[7] = node;           
        }

        Connect(parentNodePosX, parentNodePosY, parentNodePosZ, layer + 1, parentNode);
        }
    }

    //Loads chunks
    private Chunk LoadArea(int x, int y, int z, LoadMarker marker)
    {
         if (x >= 0 && z >= 0 && y >= 0){
        int lolong = (int) Morton3D.encode(x, y, z);
        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;

        if(lolong < size  && octree.nodes[0][lolong] == default(OctreeNode)){
        Stopwatch watch = new Stopwatch();
        watch.Start();
            
        OctreeNode childNode = new OctreeNode();
        childNode.locCode = lolong;
        octree.nodes[0][lolong] = childNode;

        Chunk chunk = generator.GetChunk(x << Constants.CHUNK_EXPONENT, y << Constants.CHUNK_EXPONENT, z << Constants.CHUNK_EXPONENT);
        childNode.chunk = chunk;
        watch.Stop();
        debugMeasures[0].Add(watch.ElapsedMilliseconds);
        
        mesher.MeshChunk(chunk, false);
        Connect(x, y, z, 1, childNode);
        return chunk;
        }

        //  marker.sendChunk(chunk);
        }
        return new Chunk();
    }

    public List<long>[] GetMeasures(){
        debugMeasures[1] = mesher.GetAddingMeasures();
        debugMeasures[2] = mesher.GetMeshingMeasures();
        return debugMeasures;
    }

    private static MeshInstance DebugMesh()
    {
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