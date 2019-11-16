using System.Collections.Generic;
using Godot;
using Threading = System.Threading.Thread;
using ThreadingStart = System.Threading.ThreadStart;
public class GameController : Spatial
{
    private List<MeshInstance> instances;
    private Terra terra;
    private PrimitiveResources resources;

    private Terra world;
    private LoadMarker player;
    private static readonly int MAX_WORLD_SIZE = 2097151;
    private static readonly long WORLD_SIZE = 4000;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
     //Has to be devidable by 16
        instances = new List<MeshInstance>();
        Registry registry = new Registry();
        PrimitiveResources.register(registry);

        Foreman foreman = new Foreman();
        foreman.SetMaterials(registry);
        GameMesher mesher = new GameMesher(instances, registry);
        WorldGenerator generator = new WorldGenerator(0, 0, mesher, foreman);
        world = new Terra(0, 0, 0, generator);

        ThreadingStart start = new ThreadingStart(generation); 
        AddChild(new Node(), true);

        Threading thread = new Threading(start);
        thread.Start();
    }

    public void generation(){
        LoadMarker player = new LoadMarker(0.0f, 0.0f, 0.0f, 2, 2);
        world.initialWorldGeneration(player);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
  {
      if(instances.Count > 0){
        MeshInstance chunk = instances[instances.Count - 1];
        instances.RemoveAt(instances.Count - 1);
        this.AddChild(chunk, true);
        chunk.SetOwner(this);
      }
  }
}
