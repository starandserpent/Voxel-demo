using Godot;
public class SolidCameraPoint : LoadMarker
{
	private GameController gameController;
	private Camera camera;
	public override void _Ready()
	{
		VisualServer.SetDebugGenerateWireframes(true);
		gameController = (GameController) FindParent("SceneController");
		camera = (Camera) FindNode("Camera");
		gameController.Prepare(camera, GetWorld().Scenario);
		Input.SetMouseMode(Input.MouseMode.Captured);
	}

	public override void _PhysicsProcess(float delta)
	{
	}
	public override void _Process(float delta)
	{
		gameController.Generate(this);
	}
}
