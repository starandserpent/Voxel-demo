using System.Collections.Generic;
using Godot;
using Threading = System.Threading.Thread;
using ThreadingStart = System.Threading.ThreadStart;

public class GameController : Spatial
{
    private volatile List<MeshInstance> instances;
    private Picker picker;
    private Terra world;
    private LoadMarker player;
    [Export] public bool Profiling = false;
    [Export] public uint WORLD_SIZEX = 32;
    [Export] public uint WORLD_SIZEY = 32;
    [Export] public uint WORLD_SIZEZ = 32;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Has to be devidable by 16
        instances = new List<MeshInstance>();
        Registry registry = new Registry();
        PrimitiveResources.register(registry);

        GameMesher mesher = new GameMesher(this, registry, Profiling);
        world = new Terra(WORLD_SIZEX, WORLD_SIZEY, WORLD_SIZEZ, registry, mesher, this, Profiling);
        picker = new Picker(world, mesher);
        if (Profiling)
        {
            ThreadingStart start = Begin;
        Threading thread = new Threading(start);
        thread.Start();
        }
    }

    private void Begin()
    {
        Generate(new LoadMarker());
    }

    public void Generate(LoadMarker marker)
    {
        world.Generate(marker);
    }

    public Picker GetPicker()
    {
        return picker;
    }
}
