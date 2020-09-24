using System;
using Godot;

public class Player : Spatial 
{
    [Export] public float MOUSE_SENSITIVITY = 0.002F;
    [Export] public float MOVE_SPEED = 0.9F;

    private Vector3 motion;
    private Vector3 initialRotation;
    private const float RAY_LENGHT = 10;
    private RayCast ray;
    public Camera camera;
    private Picker picker;
    private Label fps;
    private Label position;
    private Label chunks;
    private Label vertices;
    private Label memory;
    private Label speed;
    private Spatial shadow;
    private bool wireframe = false;

    public override void _Input (InputEvent @event) {
        if (Input.IsActionPressed ("toggle_mouse_capture")) {
            Input.SetMouseMode (Input.GetMouseMode () == Input.MouseMode.Captured ?
                Input.MouseMode.Visible :
                Input.MouseMode.Captured);
        } else if (Input.IsActionPressed ("toggle_wireframe_mode")) {
            if (wireframe) {
                GetViewport ().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            } else {
                GetViewport ().DebugDraw = Viewport.DebugDrawEnum.Disabled;
            }

            wireframe = !wireframe;
        } else if (Input.IsActionPressed ("ui_cancel")) {
            GetTree ().Quit ();
        } else if (@event is InputEventMouseMotion eventKey) {

            if (Input.GetMouseMode () == Input.MouseMode.Captured) {

                Vector3 rotation = new Vector3 (
                    (float) Math.Max (Math.Min (Rotation.x - eventKey.Relative.y * MOUSE_SENSITIVITY, Math.PI / 2), -Math.PI / 2),
                    Rotation.y - eventKey.Relative.x * MOUSE_SENSITIVITY,
                    Rotation.z);

                shadow.Rotation = rotation;

                Rotation = rotation;
            }

        } else if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed &&
            eventMouseButton.ButtonIndex == 1) {

            Vector3 from = camera.ProjectRayOrigin (eventMouseButton.Position);
            Vector3 to = camera.ProjectRayNormal (eventMouseButton.Position) * RAY_LENGHT;
            GD.Print (to);
           // ray.Translation = from;
            //ray.CastTo = to;
            //ray.Enabled = true;
        }
    }

    public override void _Ready () {
        GD.Print (ConfigManager.BASE_CONFIG_FILE_DIRECTORY_PATH);
        GD.Print (ConfigManager.BASE_DIRECTORY);
        GD.Print (ConfigManager.BASE_CONFIG_FILE_PATH);
        VisualServer.SetDebugGenerateWireframes (true);
        camera = (Camera) FindNode ("Camera");
        fps = (Label) camera.FindNode ("FPS");
        position = (Label) camera.FindNode ("Position");
        chunks = (Label) camera.FindNode ("Chunks");
        vertices = (Label) camera.FindNode ("Vertices");
        memory = (Label) camera.FindNode ("Memory");
        speed = (Label) camera.FindNode ("Movement Speed");

        shadow = (Spatial) FindNode ("Shadow");

        initialRotation = new Vector3 ();

        Input.SetMouseMode (Input.MouseMode.Captured);
    }

    public override void _PhysicsProcess (float delta) {
      /*  if (ray.IsColliding ()) {
            picker.Pick (ray.GetCollisionPoint (), ray.GetCollisionNormal ());
            ray.Enabled = false;
        }*/

        chunks.Text = "Chunks: ";
        vertices.Text = "Vertices: " + Performance.GetMonitor (Performance.Monitor.RenderVerticesInFrame);
        fps.Text = "FPS: " + Performance.GetMonitor (Performance.Monitor.TimeFps);
        position.Text = "X: " + GlobalTransform.origin.x + "Y: " +
            GlobalTransform.origin.y + "Z:" + GlobalTransform.origin.z;
        memory.Text = "Memory: " + GC.GetTotalMemory (false) / (1048576) + "MB";

        Vector3 velocity = new Vector3 ();

        if (Input.IsActionPressed ("walk_left")) {
            motion.x = 1;
        } else if (Input.IsActionPressed ("walk_right")) {
            motion.x = -1;
        } else {
            motion.x = 0;
        }

        if (Input.IsActionPressed ("walk_forward")) {
            motion.z = -1;
        } else if (Input.IsActionPressed ("walk_backward")) {
            motion.z = 1;
        } else {
            motion.z = 0;
        }

        if (Input.IsActionPressed ("move_up")) {
            motion.y = 1;
        } else if (Input.IsActionPressed ("move_down")) {
            motion.y = -1;
        } else {
            motion.y = 0;
        }

        motion = motion.Normalized ();

        if (Input.IsActionPressed ("move_speed")) {
            motion *= 2;
        }

        motion = motion.Rotated (new Vector3 (0, 1, 0), Rotation.y - initialRotation.y)
            .Rotated (new Vector3 (1, 0, 0), (float) Math.Cos (Rotation.y) * Rotation.x)
            .Rotated (new Vector3 (0, 0, 1), -(float) Math.Sin (Rotation.y) * Rotation.x);

        velocity = motion * MOVE_SPEED;

        speed.Text = "Movement Speed: " + velocity.Length () + "m/s";

        Vector3 translation = new Vector3 (Translation.x + velocity.x, Translation.y + velocity.y,
            Translation.z + velocity.z);

        Translation = translation;

        shadow.Translation = Translation;
    }

    public override void _ExitTree () {
        Input.SetMouseMode (Input.MouseMode.Visible);
    }

    public void AddShadow(Spatial spatial)
    {
        shadow = spatial;
    }

    public override void _Process (float delta) {

    }
}