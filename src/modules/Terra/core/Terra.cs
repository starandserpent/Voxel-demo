using System.Collections.Generic;
using Godot;
public class Terra
{
    // Declare member variables here. Examples:
    private Octree octree;
    private Foreman foreman;
    private WorldGenerator generator;
    private Registry registry;
    public Terra(int centX, int centY, int centZ, Registry registry, GameMesher mesher){
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
