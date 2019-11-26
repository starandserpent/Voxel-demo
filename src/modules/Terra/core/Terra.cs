using System.Collections.Generic;
using System;
using Godot;
public class Terra
{
    // Declare member variables here. Examples:
    private Octree octree;
    private Foreman foreman;
    private WorldGenerator generator;
    private Registry registry;
    public Terra(uint sizeX, uint sizeY, uint sizeZ, Registry registry, GameMesher mesher){
        octree.sizeX = sizeX;
        octree.sizeY = sizeY;
        octree.sizeZ = sizeZ;

        uint size = octree.sizeX * octree.sizeY * octree.sizeZ;
        octree.layers = (uint) Utils.calculateLayers(size);

        octree.nodes = new Dictionary<int, Memory<OctreeNode>>();
        octree.nodes[0]= new OctreeNode[size];

        foreman = new Foreman();
        foreman.SetMaterials(registry);
        generator = new WorldGenerator(octree, mesher, foreman);
        octree = new Octree();
    }
    
    public void initialWorldGeneration(LoadMarker loadMarker){
        generator.SeekSector(loadMarker);
    }
    public void SetMeshInstaces(List<MeshInstance> instances){
        generator.SetMeshInstaces(instances);
    }
}
