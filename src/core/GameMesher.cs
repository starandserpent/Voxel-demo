using System.Linq;
using System.Collections.Generic;
using Godot;
using GodotArray = Godot.Collections.Array;

public class GameMesher
{
    private volatile Node parent;
    private volatile SplatterMesher splatterMesher;
    private GreedyMesher mesher = null;
    public GameMesher(Node parent, Registry reg, bool profile)
    {
        this.parent = parent;
        mesher = new GreedyMesher(reg, true);
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
            meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));

            Node node = parent.FindNode(meshInstance.Name);
            if (node != null)
            {
                parent.RemoveChild(node);
            }

            parent.AddChild(meshInstance);
        }
    }

    public List<long> GetAddingMeasures(){
        return mesher.GetAddingMeasures();
    }

    public List<long> GetMeshingMeasures(){
        return mesher.GetMesherMeasures();
    }
}
