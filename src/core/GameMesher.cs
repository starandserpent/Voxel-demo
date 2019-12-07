using System.Linq;
using System.Collections.Generic;
using Godot;
using GodotArray = Godot.Collections.Array;

public class GameMesher
{
    private volatile Node parent;
    private volatile SplatterMesher splatterMesher;
    private volatile Registry registry;

    private GreedyMesher mesher = null;
    public GameMesher(Node parent, Registry reg, bool profile)
    {
        this.parent = parent;
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.Shader = (GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        splatterMesher = new SplatterMesher(shaderMat, reg);
        this.registry = registry;
    }

    public void MeshChunk(Chunk chunk, bool splatter)
    {
        MeshInstance meshInstance = new MeshInstance();
        if (!splatter)
        {
            StartMeshing(meshInstance, chunk);
        }
        else
        {
            meshInstance = splatterMesher.CreateChunkMesh(chunk);
        }
    }

    private void StartMeshing(MeshInstance meshInstance, Chunk chunk)
    {
        if (!chunk.isEmpty)
        {
            mesher = new GreedyMesher(registry, true);
            Dictionary<Texture, GodotArray> arrays = mesher.cull(chunk);

            ArrayMesh mesh = new ArrayMesh();

            List<Vector3> faces = new List<Vector3>();

            meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
            meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));

            foreach (Texture texture in arrays.Keys.ToArray())
            {
                GodotArray array = arrays[texture];
                SpatialMaterial material = new SpatialMaterial();
                texture.Flags = 2;
                material.AlbedoTexture = texture;
              //  faces.AddRange((Vector3[]) array[0]);

                mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
                mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, material);
            }

            ConcavePolygonShape shape = new ConcavePolygonShape();
           // shape.SetFaces(faces.ToArray());
            StaticBody body = new StaticBody();
            CollisionShape colShape = new CollisionShape();
            colShape.SetShape(shape);
           // body.AddChild(colShape);
         //   meshInstance.AddChild(body);

            meshInstance.SetMesh(mesh);
            Node node = parent.FindNode(meshInstance.Name);
            if (node != null)
            {
                parent.RemoveChild(node);
            }

            parent.AddChild(meshInstance);

            arrays.Clear();
        }
    }

    public List<long> GetAddingMeasures(){
        return mesher.GetAddingMeasures();
    }

    public List<long> GetMeshingMeasures(){
        return mesher.GetMesherMeasures();
    }
}
