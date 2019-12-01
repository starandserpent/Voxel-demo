using System.Collections.Generic;
using Godot;
public class GameController : Spatial
{
    private List<MeshInstance> instances;
    private Terra terra;
    private PrimitiveResources resources;
    private Picker picker;
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
        world = new Terra(16, 16, 16, registry, mesher);
        picker = new Picker(world, mesher);
        world.SetMeshInstaces(instances);
    }

    public void InitialWorldGeneration(LoadMarker marker){
        world.InitialWorldGeneration(marker);
    }

    public Picker GetPicker(){
        return picker;
    }

//  Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
  {
      if(instances.Count > 0){
        MeshInstance chunk = instances[instances.Count - 1];
        foreach(Node node in GetChildren()){
            if(node.Name.Equals(chunk.Name)){
                RemoveChild(node);
            }
        }
        this.AddChild(chunk);
        instances.RemoveAt(instances.Count - 1);
      }
  }
}
