using System.Collections.Generic;
using Godot;
using Threading = System.Threading.Thread;
using ThreadingStart = System.Threading.ThreadStart;

public class GameController : Spatial
{
    private volatile List<MeshInstance> instances;
    private Picker picker;
    private Terra terra;
    private Foreman foreman;
    [Export] public bool Profiling = false;
    [Export] public int AVERAGE_TERRAIN_HIGHT = 130;
    [Export] public int SEED = 19083;
    [Export] public int TERRAIN_GENERATION_MULTIPLIER = 10;
    [Export] public int MAX_ELEVATION = 30;
    [Export] public float NOISE_FREQUENCY = 0.45F;

    [Export] public int WORLD_SIZEX = 32;
    [Export] public int WORLD_SIZEY = 32;
    [Export] public int WORLD_SIZEZ = 32;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Has to be devidable by 16
        instances = new List<MeshInstance>();
        Registry registry = new Registry();
        PrimitiveResources.register(registry);

        GameMesher mesher = new GameMesher(this, registry, Profiling);
        Weltschmerz weltschmerz = new Weltschmerz(SEED, TERRAIN_GENERATION_MULTIPLIER, AVERAGE_TERRAIN_HIGHT, MAX_ELEVATION, NOISE_FREQUENCY);
        terra = new Terra(WORLD_SIZEX, WORLD_SIZEY, WORLD_SIZEZ, this);
        foreman = new Foreman(weltschmerz, this, terra, mesher);
        foreman.SetMaterials(registry);
        picker = new Picker(terra, mesher);
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
