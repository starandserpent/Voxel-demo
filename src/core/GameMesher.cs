using System.Collections.Generic;
using Godot;

public class GameMesher
{
    private volatile List<MeshInstance> instances;
    private volatile SplatterMesher splatterMesher;
    private volatile NaiveGreedyMesher mesher = null;

    public GameMesher(List<MeshInstance> instances, Registry reg, bool profile)
    {
        this.instances = instances;
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

            instances.Add(meshInstance);
        }
    }

    public List<long> GetAddingMeasures()
    {
        return mesher.GetAddingMeasures();
    }

    public List<long> GetMeshingMeasures()
    {
        return mesher.GetMesherMeasures();
    }
}