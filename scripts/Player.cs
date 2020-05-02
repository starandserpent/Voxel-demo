using System.Linq;
using System;
using Godot;
using System.Collections.Generic;

public class Player : LoadMarker
{
    [Export] public float MOUSE_SENSITIVITY = 0.002F;
    [Export] public float MOVE_SPEED = 0.9F;
    [Export] public int LOAD_RADIUS = 2;
    private Vector3 motion;
    private Vector3 velocity;
    private Vector3 initialRotation;
    private const float RAY_LENGHT = 10;
    private RayCast ray;
    private GameController gameController;
    private Camera camera;
    private Picker picker;
    private Label fps;
    private Label memory;
    private Label chunks;
    private Label vertices;
    private Spatial lastPosition;
    private bool wireframe = false;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("toggle_mouse_capture"))
        {
            Input.SetMouseMode(Input.GetMouseMode() == Input.MouseMode.Captured
                ? Input.MouseMode.Visible
                : Input.MouseMode.Captured);
        }
        else if (Input.IsActionPressed("toggle_wireframe_mode"))
        {
            if (wireframe)
            {
                GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            }
            else
            {
                GetViewport().DebugDraw = Viewport.DebugDrawEnum.Disabled;
            }

            wireframe = !wireframe;
        }
        else if (Input.IsActionPressed("ui_cancel"))
        {
            gameController.Clear();
            GetTree().Quit();
        }
        else if (@event is InputEventMouseMotion eventKey)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                this.Rotation = (new Vector3(
                    (float) Math.Max(
                        Math.Min(this.Rotation.x - eventKey.Relative.y * MOUSE_SENSITIVITY, Math.PI / 2),
                        -Math.PI / 2), Rotation.y - eventKey.Relative.x * MOUSE_SENSITIVITY,
                    this.Rotation.z));
            }
        }
        else if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed &&
                 eventMouseButton.ButtonIndex == 1)
        {
            Vector3 from = camera.ProjectRayOrigin(eventMouseButton.Position);
            Vector3 to = camera.ProjectRayNormal(eventMouseButton.Position) * RAY_LENGHT;
            GD.Print(to);
            ray.Translation = from;
            ray.CastTo = to;
            ray.Enabled = true;
        }
    }

    public override void _Ready()
    {
        loadRadius = LOAD_RADIUS;
        VisualServer.SetDebugGenerateWireframes(true);
        gameController = (GameController) FindParent("GameController");
        ray = (RayCast) gameController.FindNode("Picker");
        camera = (Camera) FindNode("Camera");
        gameController.Prepare(camera);
        fps = (Label) camera.FindNode("FPS");
        memory = (Label)  camera.FindNode("Memory");
        chunks = (Label)  camera.FindNode("Chunks");
        vertices = (Label)  camera.FindNode("Vertices");

        initialRotation = new Vector3();

        picker = gameController.GetPicker();

        Input.SetMouseMode(Input.MouseMode.Captured);

        lastPosition = (Spatial) gameController.FindNode("Shadow");
        lastPosition.GlobalTransform = new Transform(this.GlobalTransform.basis, this.GlobalTransform.origin);
        gameController.Generate(lastPosition);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!lastPosition.GlobalTransform.Equals(this.GlobalTransform)){
            lastPosition.GlobalTransform = new Transform(this.GlobalTransform.basis, this.GlobalTransform.origin);
            gameController.Generate(lastPosition);
        }

        if (ray.IsColliding())
        {
            picker.Pick(ray.GetCollisionPoint(), ray.GetCollisionNormal());
            ray.Enabled = false;
        }
    }

    public override void _ExitTree()
    {
        Input.SetMouseMode(Input.MouseMode.Visible);
    }

    public override void _Process(float delta)
    {
        chunks.Text = "Chunks: " + gameController.GetChunkCount();
        vertices.Text = "Vertices: " + Performance.GetMonitor(Performance.Monitor.RenderVerticesInFrame);
        fps.Text = "FPS: " + Performance.GetMonitor(Performance.Monitor.TimeFps);
        memory.Text = "Memory: " + Performance.GetMonitor(Performance.Monitor.MemoryStatic) / (1024 * 1024) + " MB";

        if (Input.IsActionPressed("walk_left"))
        {
            motion.x = 1;
        }
        else if (Input.IsActionPressed("walk_right"))
        {
            motion.x = -1;
        }
        else
        {
            motion.x = 0;
        }

        if (Input.IsActionPressed("walk_forward"))
        {
            motion.z = -1;
        }
        else if (Input.IsActionPressed("walk_backward"))
        {
            motion.z = 1;
        }
        else
        {
            motion.z = 0;
        }

        if (Input.IsActionPressed("move_up"))
        {
            motion.y = 1;
        }
        else if (Input.IsActionPressed("move_down"))
        {
            motion.y = -1;
        }
        else
        {
            motion.y = 0;
        }

        motion = motion.Normalized();

        if (Input.IsActionPressed("move_speed"))
        {
            motion *= 2;
        }

        motion = motion.Rotated(new Vector3(0, 1, 0), Rotation.y - initialRotation.y)
            .Rotated(new Vector3(1, 0, 0), (float) Math.Cos(Rotation.y) * Rotation.x)
            .Rotated(new Vector3(0, 0, 1), -(float) Math.Sin(Rotation.y) * Rotation.x);

        velocity += motion * MOVE_SPEED;

        velocity = new Vector3(velocity.x * 0.9f, velocity.y * 0.9f, velocity.z * 0.9f);

        Translation = new Vector3(Translation.x + (velocity.x), Translation.y + (velocity.y),
            Translation.z + (velocity.z));
    }
}