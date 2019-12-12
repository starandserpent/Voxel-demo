using System.Linq;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Foreman
{
     //This is recommend max static octree size because it takes 134 MB
    private static readonly int MAX_OCTREE_NODE_SIZE = 256;
    private static readonly int MAX_OCTANT_LAYERS = 4;
    private volatile GameMesher mesher;
    private volatile Octree octree;
    private volatile Node parent;
    private List<long>[] debugMeasures;
    private int dirtID;
    private int grassID;
    private int grassMeshID;
    private Weltschmerz weltschmerz;
    private Registry registry;
    private bool profiling;
    private volatile Dictionary<int, Chunk>[][] surfaceChunks;
    public Foreman(Weltschmerz weltschmerz, Node parent, Terra terra, GameMesher mesher)
    {
        this.weltschmerz = weltschmerz;
        this.mesher = mesher;
        this.octree = terra.GetOctree();
        this.parent = parent;
        debugMeasures = new List<long>[3];
        surfaceChunks = new Dictionary<int, Chunk>[octree.sizeZ][];
        for(int i = 0; i < octree.sizeZ; i ++){
            surfaceChunks[i] = new Dictionary<int, Chunk>[octree.sizeZ];
        }
        debugMeasures[0] = new List<long>();
    }

    //Initial generation
    public void GenerateTerrain(LoadMarker loadMarker)
    {
        //Round world size to nearest node lenght
        int playerPosX = (int) ((loadMarker.GetTranslation().x / Constants.CHUNK_LENGHT));
        int playerPosY = (int) ((loadMarker.GetTranslation().y / Constants.CHUNK_LENGHT));
        int playerPosZ = (int) ((loadMarker.GetTranslation().z / Constants.CHUNK_LENGHT));

        Translation translation = new Translation();

               for(int y = 0; y < loadMarker.loadRadiusY/2; y++){
            for(int z = 0; z < loadMarker.loadRadiusZ/2; z++){
                for(int x = 0; x < loadMarker.loadRadiusX/2; x++){
                    LoadArea(playerPosX + x, playerPosY + y, playerPosZ + z, loadMarker);
                    LoadArea(playerPosX - x, playerPosY - y, playerPosZ - z, loadMarker);
             }
        }     
        }

      /*          if(profiling){
        GD.Print("Profiling started at " + DateTime.Now);
        Stopwatch watch = new Stopwatch();
        List<long>[] measures = GetMeasures();
        GD.Print("Profiling finished after " + watch.Elapsed.Seconds +" seconds");

        GD.Print("Average filling " + measures[0].Average()+" ms");
        GD.Print("Min filling " + measures[0].Min()+" ms");
        GD.Print("Max filling " + measures[0].Max()+" ms");

          GD.Print("Average meshing rle " + measures[2].Average()+" ms");
        GD.Print("Min Meshing " + measures[2].Min()+" ms");
        GD.Print("Max Meshing " + measures[2].Max()+" ms");

          GD.Print("Average adding to godot  " + measures[1].Average()+" ms");
        GD.Print("Min Mesh generation  " + measures[1].Min()+" ms");
        GD.Print("Max Mesh generation  " + measures[1].Max()+" ms");

        GD.Print("Total time taken for one chunk  " + (measures[0].Average() + measures[1].Average() + measures[2].Average() )+" ms");
        }
    */
    }

    private void Connect(int posX, int posY, int posZ, int layer, OctreeNode node){
        int parentNodePosX = (int)(posX/2);
        int parentNodePosY = (int)(posY/2);
        int parentNodePosZ = (int)(posZ/2);

        int lolong = (int) Morton3D.encode(parentNodePosX, parentNodePosY, parentNodePosZ);
        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
                   MeshInstance instance = DebugMesh();
            instance.Scale = new Vector3(16 * (float) Math.Pow(2, layer - 1), 16 * (float) Math.Pow(2, layer - 1),
                16 * (float) Math.Pow(2, layer - 1));
            instance.Name = posX * 8 * (float) Math.Pow(2, layer) + " " + posY * 8 * (float) Math.Pow(2, layer) +
                            " " + posZ * 8 * (float) Math.Pow(2, layer);
            instance.Translation = new Vector3(posX * 8 * (float) Math.Pow(2, layer),
                posY * 8 * (float) Math.Pow(2, layer), posZ * 8 * (float) Math.Pow(2, layer));
            parent.CallDeferred("add_child", instance);
        if(lolong < size && layer < octree.layers){
        
            OctreeNode parentNode;
            if(octree.nodes.ContainsKey(layer)){
                if(octree.nodes[layer][lolong] != default(OctreeNode)){
                  parentNode = octree.nodes[layer][lolong];
                }else{
                    parentNode = new OctreeNode();
                    parentNode.locCode = lolong;
                    parentNode.children = new OctreeNode[8];
                    octree.nodes[layer][lolong] = parentNode;
                }
            }else{
                octree.nodes[layer] = new OctreeNode[size];
                parentNode = new OctreeNode();
                parentNode.locCode = lolong;
                parentNode.children = new OctreeNode[8];
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

        if(surfaceChunks[x][z] == null){
            surfaceChunks[x][z] = GenerateSurfaceChunks(x  << Constants.CHUNK_EXPONENT, z << Constants.CHUNK_EXPONENT);
        }

        Chunk chunk;

        if(surfaceChunks[x][z].ContainsKey(y)){
            chunk = surfaceChunks[x][z][y];    
        }else{
            chunk = new Chunk();
            chunk.materials = 1;

            chunk.x = (uint) x  << Constants.CHUNK_EXPONENT;
            chunk.y = (uint) y  << Constants.CHUNK_EXPONENT;
            chunk.z = (uint) z  << Constants.CHUNK_EXPONENT;

            chunk.voxels = new uint[1];
            chunk.voxels[0] = (uint) dirtID;

            if(surfaceChunks[x][z].Keys.Max() < y){
                chunk.isEmpty = true;
            }
        }
         
        watch.Stop();
        debugMeasures[0].Add(watch.ElapsedMilliseconds);
           // mesher.MeshChunk(chunk, false);
            Connect(x, y, z, 1, childNode);
        return chunk;
        }
        }
        return new Chunk();
    }

    public List<long>[] GetMeasures(){
        debugMeasures[1] = mesher.GetAddingMeasures();
        debugMeasures[2] = mesher.GetMeshingMeasures();
        return debugMeasures;
    }

    public void SetMaterials(Registry registry)
    {
        dirtID = registry.SelectByName("dirt").worldID;
        grassID = registry.SelectByName("grass").worldID;
    }

    private Dictionary<int, Chunk> GenerateSurfaceChunks(float posX, float posZ)
    {
        Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();

        int posx = (int) (posX * 4);
        int posz = (int) (posZ * 4);

        int lastPosition = 0;

        for (int z = 0; z < Constants.CHUNK_SIZE1D; z++)
        {
            for (int x = 0; x < Constants.CHUNK_SIZE1D; x++)
            {
                int elevation = (int) weltschmerz.GetElevation(x + posx, z + posz);

                Chunk chunk;
                if(!chunks.ContainsKey(elevation/ Constants.CHUNK_SIZE1D)){
                    chunk = InitChunk((int)posX, elevation/ Constants.CHUNK_SIZE1D, (int)posZ);
                    chunks.Add(elevation/ Constants.CHUNK_SIZE1D, chunk);
                }else{
                    chunk = chunks[elevation/ Constants.CHUNK_SIZE1D];
                }

                    int elev = elevation % Constants.CHUNK_SIZE1D;
                    uint bitPos;
                    uint bitValue;
                    
                    bitPos = (uint) elev << 8;
                    bitValue = (uint) dirtID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    if(elev + 1 < Constants.CHUNK_SIZE1D){
                    bitPos = (uint) 1 << 8;
                    bitValue = (uint) grassID;
                    chunk.voxels[lastPosition] = (bitPos | bitValue);
                     lastPosition++;
                    }

                    bitPos = (uint) (Constants.CHUNK_SIZE1D - elev - 1) << 8;
                    bitValue = (uint) 0;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;
            }
        }
        return chunks;
    }

    private Chunk InitChunk(int posX, int posY, int posZ){
        Chunk chunk = new Chunk();
        chunk.x = (uint) posX;
        chunk.y = (uint) posY << Constants.CHUNK_EXPONENT;
        chunk.z = (uint) posZ;

        chunk.voxels =  new uint[Constants.CHUNK_SIZE3D];

        chunk.materials = 3;
        return chunk;
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
        return instance;
    }
}