using Godot;
using Threading = System.Threading.Thread;
public class SolidCameraPoint : LoadMarker
{
	private GameController gameController;
	private Camera camera;
    private Transform lastPosition;
    private Threading GenerateThread;
	public override void _Ready()
	{
		VisualServer.SetDebugGenerateWireframes(true);
		gameController = (GameController) FindParent("SceneController");
		camera = (Camera) FindNode("Camera");
		gameController.Prepare(camera);
		Input.SetMouseMode(Input.MouseMode.Captured);
        lastPosition = this.Transform;
        gameController.Generate(this);
	}

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_cancel"))
        {
            GetTree().Quit();
        }
    }

	public override void _PhysicsProcess(float delta)
	{
        if(!lastPosition.Equals(this.Transform)){
            lastPosition = this.Transform;
            gameController.Generate(this);
        }
	}
	public override void _Process(float delta)
	{
	}
}
