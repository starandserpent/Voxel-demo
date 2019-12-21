using System.Collections.Generic;
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
               MeshInstance instance = mesher.MeshChunk(chunk);
                this.AddChild(instance);
            }
        }
    }

    public void Prepare(Camera camera)
    {
        foreman = new Foreman(weltschmerz, terra, registry, VIEW_DISTANCE, camera.Fov, GENERATION_THREADS);
        foreman.SetQueue(instances);
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