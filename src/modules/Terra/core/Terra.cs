using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Godot;

public class Terra
{
    // Declare member variables here. Examples:
    private Octree octree;
    private Foreman foreman;
    private WorldGenerator generator;
    private bool profiling;
    public Terra(uint sizeX, uint sizeY, uint sizeZ, Registry registry, GameMesher mesher, Node parent, bool profiling)
    {
        octree = new Octree();
        octree.sizeX = sizeX;
        octree.sizeY = sizeY;
        octree.sizeZ = sizeZ;

        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
        octree.layers = (uint) Utils.calculateLayers(size);

        octree.nodes = new Dictionary<int, OctreeNode[]>();
        octree.nodes[0] = new OctreeNode[size];

        foreman = new Foreman();
        foreman.SetMaterials(registry);
        generator = new WorldGenerator(parent, octree, mesher, foreman, profiling);
        this.profiling = profiling;
    }

    public Chunk TraverseOctree(int posX, int posY, int posZ)
    {
        if (posX >= 0 && posY >= 0 && posZ >= 0)
        {
            int lolong = (int) Morton3D.encode(posX, posY, posZ);
            OctreeNode node = octree.nodes[0][lolong];
            return node.chunk;
        }

        return default(Chunk);
    }

    public void ReplaceChunk(int posX, int posY, int posZ, Chunk chunk)
    {
        int lolong = (int) Morton3D.encode(posX, posY, posZ);
        OctreeNode node = octree.nodes[0][lolong];
        node.chunk = chunk;
        octree.nodes[0][lolong] = node;
    }

    public void InitialWorldGeneration(LoadMarker loadMarker)
    {
        if(profiling){
        GD.Print("Profiling started at " + DateTime.Now);
        Stopwatch watch = new Stopwatch();
        watch.Start();
        generator.SeekSector(loadMarker);
        List<long>[] measures = generator.GetMeasures();
        watch.Stop();
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
        }else{
            generator.SeekSector(loadMarker);
        }
    }
}