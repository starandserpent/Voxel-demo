using System.Collections.Generic;
using Godot;

public class GameController : Spatial
{
    private volatile List<MeshInstance> instances;
    private Picker picker;
    private Terra terra;
    private Foreman foreman;
    [Export] public bool Profiling = false;
    [Export] public int SEED = 19083;
    [Export] public int AVERAGE_TERRAIN_HIGHT = 130;
    [Export] public int TERRAIN_GENERATION_MULTIPLIER = 10;
    [Export] public int MAX_ELEVATION = 100;
    [Export] public float NOISE_FREQUENCY = 0.45F;
    [Export] public int VIEW_DISTANCE = 100;
    [Export] public int WORLD_SIZEX = 32;
    [Export] public int WORLD_SIZEY = 32;
    [Export] public int WORLD_SIZEZ = 32;

    private GameMesher mesher;
    private Weltschmerz weltschmerz;
    private Registry registry;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Has to be devidable by 16
        instances = new List<MeshInstance>();
        registry = new Registry();
        PrimitiveResources.register(registry);
        mesher = new GameMesher(this, registry, Profiling);
        weltschmerz = new Weltschmerz(SEED, TERRAIN_GENERATION_MULTIPLIER, AVERAGE_TERRAIN_HIGHT, MAX_ELEVATION, NOISE_FREQUENCY);
        terra = new Terra(WORLD_SIZEX, WORLD_SIZEY, WORLD_SIZEZ, this);
        picker = new Picker(terra, mesher);
    }

    public void Prepare(Camera camera){
        foreman = new Foreman(weltschmerz, this, terra, mesher, VIEW_DISTANCE, camera.Fov);
        foreman.SetMaterials(registry);
    }

    public void Generate(LoadMarker marker, Basis basis)
    {
        foreman.GenerateTerrain(marker, basis);
    }

    public Picker GetPicker()
    {
        return picker;
    }
}
