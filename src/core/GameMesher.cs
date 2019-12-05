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

    public GameMesher(Node parent, Registry reg)
    {
        this.parent = parent;
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.Shader = (GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        greedyMesher = new GreedyMesher(reg);
        splatterMesher = new SplatterMesher(shaderMat, reg);
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
            Dictionary<Texture, GodotArray> arrays = greedyMesher.cull(chunk);

            ArrayMesh mesh = new ArrayMesh();

            List<Vector3> faces = new List<Vector3>();

            meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
            meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));

            for (int t = 0; t < arrays.Keys.Count; t++)
            {
                Texture texture = arrays.Keys.ToArray()[t];
                GodotArray array = arrays[texture];
                SpatialMaterial material = new SpatialMaterial();
                texture.Flags = 2;
                material.AlbedoTexture = texture;
                faces.AddRange((Vector3[]) array[0]);

                mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
                mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, material);
            }

            ConcavePolygonShape shape = new ConcavePolygonShape();
            shape.SetFaces(faces.ToArray());
            StaticBody body = new StaticBody();
            CollisionShape colShape = new CollisionShape();
            colShape.SetShape(shape);
            body.AddChild(colShape);
            meshInstance.AddChild(body);

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
}