using Godot;
using Threading = System.Threading.Thread;
public class SolidCameraPoint : LoadMarker
{
	private GameController gameController;
	private Camera camera;
    private Spatial lastPosition;
    private Threading GenerateThread;
	public override void _Ready()
	{
		VisualServer.SetDebugGenerateWireframes(true);
		gameController = (GameController) FindParent("SceneController");
		camera = (Camera) FindNode("Camera");
		gameController.Prepare(camera);
		Input.SetMouseMode(Input.MouseMode.Captured);

		lastPosition = (Spatial) gameController.FindNode("Shadow");
        lastPosition.GlobalTransform = new Transform(this.GlobalTransform.basis, this.GlobalTransform.origin);
        gameController.Generate(lastPosition);
	}

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("ui_cancel"))
        {
			gameController.Clear();
            GetTree().Quit();
        }
    }

	public override void _Process(float delta)
	{
	}
}
