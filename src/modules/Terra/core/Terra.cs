using System.Collections.Generic;
using Godot;
using System;

public class Terra
{
    // Declare member variables here. Examples:
    private List<OctreeNode> octreeNodes;
    private Foreman foreman;
    private WorldGenerator generator;
    public Terra(int centX, int centY, int centZ, WorldGenerator worldGenerator){
        octreeNodes = new List<OctreeNode>();
        this.generator = worldGenerator;
    }
    
    public void initialWorldGeneration(LoadMarker loadMarker){
        generator.seekSector(loadMarker);
    }
}
