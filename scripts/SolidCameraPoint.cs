using Godot;
public class SolidCameraPoint : LoadMarker
{
    private GameController gameController;
    private Camera camera;
    public override void _Ready()
    {
        VisualServer.SetDebugGenerateWireframes(true);
        gameController = (GameController) FindParent("GameController");
        camera = (Camera) FindNode("Camera");
        gameController.Prepare(camera);
        Input.SetMouseMode(Input.MouseMode.Captured);

    }

    public override void _PhysicsProcess(float delta)
    {
        gameController.Generate(this);
    }
}