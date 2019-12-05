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

    public Terra(uint sizeX, uint sizeY, uint sizeZ, Registry registry, GameMesher mesher, Node parent)
    {
        octree = new Octree();
        octree.sizeX = sizeX;
        octree.sizeY = sizeY;
        octree.sizeZ = sizeZ;

        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
        octree.layers = (uint) Utils.calculateLayers(size);

        octree.nodes = new Dictionary<int, Memory<OctreeNode>>();
        octree.nodes[0] = new OctreeNode[size];

        foreman = new Foreman();
        foreman.SetMaterials(registry);
        generator = new WorldGenerator(parent, octree, mesher, foreman);
    }

    public Chunk TraverseOctree(int posX, int posY, int posZ)
    {
        if (posX >= 0 && posY >= 0 && posZ >= 0)
        {
            int lolong = (int) Morton3D.encode(posX, posY, posZ);
            OctreeNode node = octree.nodes[0].Span[lolong];
            return node.chunk;
        }

        return default(Chunk);
    }

    public void ReplaceChunk(int posX, int posY, int posZ, Chunk chunk)
    {
        int lolong = (int) Morton3D.encode(posX, posY, posZ);
        OctreeNode node = octree.nodes[0].Span[lolong];
        node.chunk = chunk;
        octree.nodes[0].Span[lolong] = node;
    }

    public void InitialWorldGeneration(LoadMarker loadMarker)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        generator.SeekSector(loadMarker);
        watch.Stop();
        GD.Print("Generation finsihed " + watch.Elapsed.Seconds);
    }
}