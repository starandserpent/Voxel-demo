using System.Collections.Generic;
using Godot;
using System;

public class Terra
{
    // Declare member variables here. Examples:
    private List<OctreeNode> octreeNodes;
    private Foreman foreman;
    public Terra(){
        foreman = new Foreman();
        octreeNodes = new List<OctreeNode>();
    }
    
    public void initialGeneration(){
        
    }
}
