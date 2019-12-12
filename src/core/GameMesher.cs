using System.Collections.Generic;
using Godot;

public class GameMesher
{
    private volatile Node parent;
    private volatile SplatterMesher splatterMesher;
    private volatile NaiveGreedyMesher mesher = null;
    public GameMesher(Node parent, Registry reg, bool profile)
    {
        this.parent = parent;
        mesher = new NaiveGreedyMesher(reg);
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.Shader = (GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        splatterMesher = new SplatterMesher(shaderMat, reg);
    }

    public void MeshChunk(Chunk chunk, bool splatter)
    {
        if (!splatter)
        {
            StartMeshing(chunk);
        }
        else
        {
            splatterMesher.CreateChunkMesh(chunk);
        }
    }

    private void StartMeshing(Chunk chunk)
    {
        if (!chunk.isEmpty)
        {
           MeshInstance meshInstance = mesher.cull(chunk);

            meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
            meshInstance.Translation = new Vector3(chunk.x, chunk.y, chunk.z);

            parent.CallDeferred("add_child", meshInstance);
        }
    }

    public List<long> GetAddingMeasures(){
        return mesher.GetAddingMeasures();
    }

    public List<long> GetMeshingMeasures(){
        return mesher.GetMesherMeasures();
    }
}
