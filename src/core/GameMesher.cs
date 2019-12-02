using System.Linq;
using System;
using System.Collections.Generic;
using Godot;
using GodotArray = Godot.Collections.Array;
public class GameMesher
{
    private volatile Node parent;
    private volatile GreedyMesher greedyMesher;
    private volatile SplatterMesher splatterMesher;

    public GameMesher(Node parent, Registry reg){        
        this.parent = parent;
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.Shader = (GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        greedyMesher = new GreedyMesher(reg);
        splatterMesher = new SplatterMesher(shaderMat, reg);
    }

    public void MeshChunk(Chunk chunk, bool splatter){
        MeshInstance meshInstance = new MeshInstance();
        if(!splatter){
            StartMeshing(meshInstance, chunk);
        }else{
            meshInstance = splatterMesher.CreateChunkMesh(chunk);
        }
    }

    private void StartMeshing(MeshInstance meshInstance, Chunk chunk){
        greedyMesher.cull(chunk, parent, meshInstance);
    }

    private static void JoinReversed(Dictionary<int, Face> faces, int index, int side) {
        int neighbor = 64;
        switch (side) {
            case 2:
            case 3:
                neighbor = 4096;
                break;
        }

        if(faces.ContainsKey(index - neighbor) && faces.ContainsKey(index)){
        Face nextFace = faces[index - neighbor];

        Face face = faces[index];
        if (face.terraObject == (nextFace.terraObject)) {
            if (nextFace.vector3s[2] == face.vector3s[1] && nextFace.vector3s[3] == face.vector3s[0]) {
                nextFace.vector3s[2] = face.vector3s[2];
                nextFace.vector3s[3] = face.vector3s[3];
                faces.Remove(index);
            }
        }
        }
    }
}