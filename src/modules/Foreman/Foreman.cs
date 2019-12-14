using System.Buffers;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Foreman
{
     //This is recommend max static octree size because it takes 134 MB
    private volatile GameMesher mesher;
    private volatile Octree octree;
    private volatile Node parent;
    private volatile List<long>[] debugMeasures;
    private volatile int dirtID;
    private volatile int grassID;
    private int grassMeshID;
    private volatile Weltschmerz weltschmerz;
    int lol;
    public Foreman(Weltschmerz weltschmerz, Node parent, Terra terra, GameMesher mesher)
    {
        this.weltschmerz = weltschmerz;
        this.mesher = mesher;
        this.octree = terra.GetOctree();
        this.parent = parent;
        debugMeasures = new List<long>[3];
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

    private void Connect(OctreeNode chunk, int layer, OctreeNode node){
        int[] pos = Morton3D.Decode(chunk.locCode);
        int size = octree.sizeX * octree.sizeY * octree.sizeZ;
        int posX = pos[0];
        int posY = pos[1];
        int posZ = pos[2];

        if(layer == 0){
        if(posX % 2 == 0 && posY % 2 == 0 && posZ % 2 == 0){
            node.children[0] = chunk;
        }else if(posX % 2 == 1 && posY % 2 == 0 && posZ % 2 == 0){
            node.children[1] = chunk;
        }else if(posX % 2 == 0 && posY % 2 == 0 && posZ % 2 == 1){
            node.children[2] = chunk;
        }else if(posX % 2 == 1 && posY % 2 == 0 && posZ % 2 == 1){
            node.children[3] = chunk;
        }else if(posX % 2 == 0 && posY % 2 == 1 && posZ % 2 == 0){
            node.children[4] = chunk;
        }else if(posX % 2 == 1 && posY % 2 == 1 && posZ % 2 == 0){
            node.children[5] = chunk;
        }else if(posX % 2 == 0 && posY % 2 == 1 && posZ % 2 == 1){
            node.children[6] = chunk;
        }else if(posX % 2 == 1 && posY % 2 == 1 && posZ % 2 == 1){
            node.children[7] = chunk;           
        }
        }else if(chunk.locCode < size && layer >= 1 && node.materialID == -1){

        int nodePosX = (int)(posX/(layer * 2));
        int nodePosY = (int)(posY/(layer * 2));
        int nodePosZ = (int)(posZ/(layer * 2));

        int lolong = (int) Morton3D.encode(nodePosX, nodePosY, nodePosZ);

                 MeshInstance instance = DebugMesh();
            instance.Scale = new Vector3(32 * (float) Math.Pow(2, layer - 2), 32 * (float) Math.Pow(2, layer - 2),
                32 * (float) Math.Pow(2, layer - 2));
            instance.Name = "layer: " + layer + " "+ nodePosX * 16 * (float) Math.Pow(2, layer) + " " + nodePosY * 16 * (float) Math.Pow(2, layer) +
                            " " + nodePosZ * 16 * (float) Math.Pow(2, layer);
            instance.Translation = new Vector3(nodePosX * 16 * (float) Math.Pow(2, layer - 1),
                nodePosY * 16 * (float) Math.Pow(2, layer - 1), nodePosZ * 16 * (float) Math.Pow(2, layer - 1));
            parent.CallDeferred("add_child", instance);
            
            OctreeNode parentNode;
            if(octree.nodes.ContainsKey(layer)){
                if(octree.nodes[layer][lolong] != null){
                  parentNode = octree.nodes[layer][lolong];
                }else{
                    parentNode = new OctreeNode();
                    parentNode.locCode = lolong;
                    parentNode.children = new OctreeNode[8];
                    octree.nodes[layer][lolong] = parentNode;
                }
            }else{
                octree.nodes[layer] = ArrayPool<OctreeNode>.Shared.Rent(size);
                parentNode = new OctreeNode();
                parentNode.locCode = lolong;
                parentNode.children = new OctreeNode[8];
                octree.nodes[layer][lolong] = parentNode;
            }

            if(nodePosX % 2 == 0 && nodePosY % 2 == 0 && nodePosZ % 2 == 0){
            node.children[0] = parentNode;
            }else if(nodePosX % 2 == 1 && nodePosY % 2 == 0 && nodePosZ % 2 == 0){
                parentNode.children[1] = chunk;
            }else if(nodePosX % 2 == 0 && nodePosY % 2 == 0 && nodePosZ % 2 == 1){
            parentNode.children[2] = chunk;
            }else if(nodePosX % 2 == 1 && nodePosY % 2 == 0 && nodePosZ % 2 == 1){
            parentNode.children[3] = chunk;
            }else if(nodePosX % 2 == 0 && nodePosY % 2 == 1 && nodePosZ % 2 == 0){
            parentNode.children[4] = chunk;
            }else if(nodePosX % 2 == 1 && nodePosY % 2 == 1 && nodePosZ % 2 == 0){
            parentNode.children[5] = chunk;
            }else if(nodePosX % 2 == 0 && nodePosY % 2 == 1 && nodePosZ % 2 == 1){
            parentNode.children[6] = chunk;
            }else if(nodePosX % 2 == 1 && nodePosY % 2 == 1 && nodePosZ % 2 == 1){
            parentNode.children[7] = chunk;           
            }
             Connect(chunk, layer - 1, parentNode);
        }

    /*    if(node.children[0] != null){
                List<int> materialIDs = new List<int>(8);
                materialIDs.Add(node.children[0].materialID);
        for(int i = 0; i < 8; i++){
            if(node.children[i] != null){
            if(materialIDs.Contains(node.children[i].materialID)){
                materialIDs.Add(node.children[i].materialID);
            }
            }else{
                materialIDs = null;
                break;
            }
        }

        if(materialIDs != null){
           JoinChildren(node, layer, materialIDs[0]);
        }
        }
        
        lol++;
        GD.Print(lol);
        */
    }

    private void JoinChildren(OctreeNode parentNode, int layer, int materialID){
        if(layer > 1){
        for(int i = 0; i < 8; i++){
            OctreeNode octreeNode = parentNode.children[i];
                octree.nodes[layer - 1][octreeNode.locCode] = null;
            parentNode.children[i] = null;
            
            int[] pos = Morton3D.Decode(octreeNode.locCode);
            int posX = pos[0];
            int posY = pos[1];
            int posZ = pos[2];

            string name = "layer: " + (layer - 1) + " "+ posX * 16 * (float) Math.Pow(2, (layer - 1) ) + " " + posY * 16 * (float) Math.Pow(2, (layer - 1) ) +
                            " " + posZ * 16 * (float) Math.Pow(2, (layer - 1) );
            foreach (Node node in parent.GetChildren()){
             if(node.Name.Equals(name)){
                node.Dispose();
            }
            }
        }
        }
        parentNode.materialID = materialID;
    }

    //Loads chunks
    private void LoadArea(int x, int y, int z, LoadMarker marker)
    {
         if (x >= 0 && z >= 0 && y >= 0){
        int lolong = (int) Morton3D.encode(x/((int)octree.layers*2), y/((int)octree.layers*2), z/((int)octree.layers*2));
        int size = octree.sizeX * octree.sizeY * octree.sizeZ;

        if(lolong < size){
        Stopwatch watch = new Stopwatch();
        watch.Start();

        OctreeNode parentNode;
        if(octree.nodes[(int)octree.layers][lolong] == null){
            parentNode = new OctreeNode();
            parentNode.locCode = lolong;
            parentNode.children = new OctreeNode[8];
        }else{
           parentNode = octree.nodes[(int)octree.layers][lolong];
        }

        OctreeNode childNode;
        lolong = (int) Morton3D.encode(x, y, z);
        if(octree.nodes[0][lolong] == null && lolong < size){

        childNode = new OctreeNode();

        Chunk chunk;
        if(y << Constants.CHUNK_EXPONENT > weltschmerz.GetMaxElevation()){
            chunk = new Chunk();
            chunk.isEmpty = true;
            chunk.x = (uint)x <<  Constants.CHUNK_EXPONENT;
            chunk.y =  (uint)y <<  Constants.CHUNK_EXPONENT;
            chunk.z =  (uint)z <<  Constants.CHUNK_EXPONENT;
        }else{
            chunk = GenerateChunk(x <<  Constants.CHUNK_EXPONENT, y <<Constants.CHUNK_EXPONENT, z << Constants.CHUNK_EXPONENT);
            if(!chunk.isSurface){
                childNode.materialID = (int)chunk.voxels[0];
                chunk.voxels = new uint[1];
                chunk.voxels[0] = (uint)childNode.materialID;
                chunk.x = (uint)x <<  Constants.CHUNK_EXPONENT;
                chunk.y =  (uint)y <<  Constants.CHUNK_EXPONENT;
                chunk.z =  (uint)z <<  Constants.CHUNK_EXPONENT;       
            }
        }

        childNode.locCode = lolong;
        octree.nodes[0][lolong] = childNode;
        Connect(childNode, (int) octree.layers - 1, parentNode);
        watch.Stop();
        debugMeasures[0].Add(watch.ElapsedMilliseconds);
        mesher.MeshChunk(chunk, false);
        }
        }
        }
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

  public Chunk GenerateChunk(float posX, float posY, float posZ)
    {
        Chunk chunk = new Chunk();

        chunk.x = (uint) posX;
        chunk.y = (uint) posY;
        chunk.z = (uint) posZ;

        chunk.materials = 1;

        chunk.voxels =  ArrayPool<uint>.Shared.Rent(Constants.CHUNK_SIZE3D);

        chunk.isEmpty = true;

        int posx = (int) (posX * 4);
        int posz = (int) (posZ * 4);
        int posy = (int) (posY * 4);

        int lastPosition = 0;

        chunk.isSurface = false;
        for (int z = 0; z < Constants.CHUNK_SIZE1D; z++)
        {
            for (int x = 0; x < Constants.CHUNK_SIZE1D; x++)
            {
                int elevation = (int) weltschmerz.GetElevation(x + posx, z + posz);

                if (elevation / Constants.CHUNK_SIZE1D == posy / Constants.CHUNK_SIZE1D)
                {
                    int elev = elevation % Constants.CHUNK_SIZE1D;
                    uint bitPos;
                    uint bitValue;
                        bitPos = (uint) elev << 8;
                        bitValue = (uint) dirtID;

                        chunk.voxels[lastPosition] = (bitPos | bitValue);

                        lastPosition++;

                    bitPos = (uint) 1 << 8;
                    bitValue = (uint) grassID;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;
                    bitPos = (uint) (Constants.CHUNK_SIZE1D - elev - 1) << 8;
                    bitValue = (uint) 0;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;

                    chunk.isSurface = true;
                    chunk.isEmpty = false;
                }else if (elevation / Constants.CHUNK_SIZE1D > posy / Constants.CHUNK_SIZE1D){
                    uint bitPos = (uint) (Constants.CHUNK_SIZE1D) << 8;
                    uint bitValue = (uint) dirtID;
                    chunk.isEmpty = false;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;
                }else if(elevation / Constants.CHUNK_SIZE1D < posy / Constants.CHUNK_SIZE1D){
                    uint bitPos = (uint) (Constants.CHUNK_SIZE1D) << 8;
                    uint bitValue = (uint) 0;

                    chunk.voxels[lastPosition] = (bitPos | bitValue);

                    lastPosition++;
                }
            }
        }

        if(chunk.isSurface){
            chunk.materials = 3;
        }

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