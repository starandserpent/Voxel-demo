using System.Collections.Generic;
using Godot;
public class GameController : Spatial
{
    private volatile List<MeshInstance> instances;
    private Picker picker;
    private Terra world;
    private LoadMarker player;
    private static readonly int MAX_WORLD_SIZE = 2097151;
    private static readonly long WORLD_SIZE = 4000;

    [Export]
    public uint WORLD_SIZEX = 32;
    [Export]
    public uint WORLD_SIZEY = 32;
    [Export]
    public uint WORLD_SIZEZ = 32;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
     //Has to be devidable by 16
        instances = new List<MeshInstance>();
        Registry registry = new Registry();
        PrimitiveResources.register(registry);

        Foreman foreman = new Foreman();
        foreman.SetMaterials(registry);
        GameMesher mesher = new GameMesher(this, registry);
        world = new Terra(WORLD_SIZEX, WORLD_SIZEY, WORLD_SIZEZ, registry, mesher, this);
        picker = new Picker(world, mesher);
    }

    public void InitialWorldGeneration(LoadMarker marker){
        world.InitialWorldGeneration(marker);
    }

    public Picker GetPicker(){
        return picker;
    }
}
