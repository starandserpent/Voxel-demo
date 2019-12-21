using System;
using System.Linq;
using Godot.Collections;
using System.Collections.Concurrent;
using Godot;

public class GameController : Spatial
{
    private volatile ConcurrentQueue<RawChunk> instances;
    private Picker picker;
    private volatile Terra terra;
    private Foreman foreman;
    [Export] public bool Profiling = false;
    [Export] public int SEED = 19083;
    [Export] public int AVERAGE_TERRAIN_HIGHT = 130;
    [Export] public int TERRAIN_GENERATION_MULTIPLIER = 10;
    [Export] public int MAX_ELEVATION = 100;
    [Export] public float NOISE_FREQUENCY = 0.45F;
    [Export] public int VIEW_DISTANCE = 100;
    [Export] public int WORLD_SIZE_X = 32;
    [Export] public int WORLD_SIZE_Y = 32;
    [Export] public int WORLD_SIZE_Z = 32;
    [Export] public int GENERATION_THREADS = 4;
    private GameMesher mesher;
    private Weltschmerz weltschmerz;
    private Registry registry;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Has to be devidable by 16
        instances = new ConcurrentQueue<RawChunk>();
        registry = new Registry();
        PrimitiveResources.register(registry);
        weltschmerz = new Weltschmerz(SEED, TERRAIN_GENERATION_MULTIPLIER, AVERAGE_TERRAIN_HIGHT, MAX_ELEVATION,
            NOISE_FREQUENCY);
        mesher = new GameMesher(registry, false);
        terra = new Terra(WORLD_SIZE_X, WORLD_SIZE_Y, WORLD_SIZE_Z);
        picker = new Picker(terra, mesher);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!instances.IsEmpty)
        {
            RawChunk chunk;
            if (instances.TryDequeue(out chunk))
            {
                MeshInstance meshInstance = new MeshInstance();
                ArrayMesh mesh = new ArrayMesh();
                StaticBody body = new StaticBody();
                
                for(int t = 0; t < chunk.arrays.Count(); t ++){
                    Texture texture = chunk.textures[t];
                    Vector3[] vertice = chunk.colliderFaces[t];
                    Godot.Collections.Array godotArray = chunk.arrays[t];

                    SpatialMaterial material = new SpatialMaterial();
                    texture.Flags = 2;
                    material.AlbedoTexture = texture;

                    ConcavePolygonShape shape = new ConcavePolygonShape();
                    shape.SetFaces(vertice);

                    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, godotArray);
                    mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, material);
                    CollisionShape colShape = new CollisionShape();
                    colShape.SetShape(shape);
                    body.AddChild(colShape);
                }

                meshInstance.AddChild(body);
                 meshInstance.Mesh = mesh;

            meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
            meshInstance.Translation = new Vector3(chunk.x, chunk.y, chunk.z);

                this.AddChild(meshInstance);
            }
        }
    }

    public void Prepare(Camera camera)
    {
        foreman = new Foreman(weltschmerz, terra, registry, mesher, VIEW_DISTANCE, camera.Fov, GENERATION_THREADS, 
        instances);
        foreman.SetMaterials(registry);
    }

    public void Generate(LoadMarker marker)
    {
        foreman.GenerateTerrain(marker);
    }

    public Picker GetPicker()
    {
        return picker;
    }
}