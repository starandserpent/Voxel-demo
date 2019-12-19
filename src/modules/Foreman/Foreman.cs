using System;
using System.Buffers;
using System.Numerics;
using Node = Godot.Node;
using GodotVector3 = Godot.Vector3;
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
    private Terra terra;
    private int viewDistance;
    private float fov;
    SortedSet<Tuple<int, int, int>> localCenters;
    int lol;
    public Foreman(Weltschmerz weltschmerz, Node parent, Terra terra, GameMesher mesher,
     int viewDistance, float fov)
    {
        this.weltschmerz = weltschmerz;
        this.terra = terra;
        this.mesher = mesher;
        this.octree = terra.GetOctree();
        this.parent = parent;
        this.viewDistance = viewDistance;
        this.fov = fov;
        debugMeasures = new List<long>[3];
        debugMeasures[0] = new List<long>();
        localCenters = new SortedSet<Tuple<int, int, int>>();

               for(int l = 0; l < viewDistance; l +=8){
            for(int y = 0; y < Utils.GetPosFromFOV(fov, l); y +=8){
                for(int x = 0; x <  Utils.GetPosFromFOV(fov, l); x +=8){
                    Tuple<int, int, int> center = new Tuple<int, int, int>(x, y, -l);
                        localCenters.Add(center);

                    center = new Tuple<int, int, int>(-x, y, -l);
                        localCenters.Add(center);

                center = new Tuple<int, int, int>(x, -y, -l);
                        localCenters.Add(center);

                    center = new Tuple<int, int, int>(-x, -y, -l);
                        localCenters.Add(center);
                }
            }
        }
    }

    //Initial generation
    public void GenerateTerrain(LoadMarker loadMarker)
    {
        SortedSet<Tuple<int, int, int>> topPriority = new SortedSet<Tuple<int, int, int>>();

        for(int y = 0; y < (loadMarker.loadRadius/2) * 8; y+=8){
            for(int z = 0; z < (loadMarker.loadRadius/2) * 8; z+=8){
                for(int x = 0; x < (loadMarker.loadRadius/2) * 8; x+=8){
                    Tuple<int, int, int> center = new Tuple<int, int, int>(x, y, z);
                    if(!localCenters.Contains(center)){
                        topPriority.Add(center);
                    }

                    if(!localCenters.Contains(center)){
                    center = new Tuple<int, int, int>(x, y, z);
                    topPriority.Add(center);    
                    }     
                }
            }     
        }

        foreach (Tuple<int, int, int> center in topPriority){
            GodotVector3 newPos = loadMarker.ToGlobal(new GodotVector3(center.Item1, center.Item2, center.Item3));
            LoadArea((int)newPos.x/8,(int)newPos.y/8, (int)newPos.z/8, loadMarker);
        }

        foreach (Tuple<int, int, int> center in localCenters){
            GodotVector3 newPos = loadMarker.ToGlobal(new GodotVector3(center.Item1, center.Item2, center.Item3));
            LoadArea((int)newPos.x/8,(int)newPos.y/8, (int)newPos.z/8, loadMarker);
        }

        topPriority.Clear();
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

    //Loads chunks
    private void LoadArea(int x, int y, int z, LoadMarker marker)
    {
         if (x >= 0 && z >= 0 && y >= 0){
        int lolong = (int) Morton3D.encode(x, y, z);
        int size = octree.sizeX * octree.sizeY * octree.sizeZ;

        if(lolong < size){
        Stopwatch watch = new Stopwatch();
        watch.Start();

        if(terra.TraverseOctree(x, y, z, 0).chunk == null && lolong < size){

        OctreeNode childNode = new OctreeNode();

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

        terra.PlaceChunk(x, y, z, chunk);

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
}