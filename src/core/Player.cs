using System;
using Godot;
using Threading = System.Threading.Thread;

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
    private Threading generation;
    private CanvasLayer GUI;
    private Label fps;
    private Label memory;
    private Label chunks;
    private Label vertices;
    private Vector3 lastPosition;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventKey)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                this.SetRotation(new Vector3(
                    (float) Math.Max(
                        Math.Min(this.GetRotation().x - eventKey.Relative.y * MOUSE_SENSITIVITY, Math.PI / 2),
                        -Math.PI / 2), GetRotation().y - eventKey.Relative.x * MOUSE_SENSITIVITY,
                    this.GetRotation().z));
            }
        }
        else if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed &&
                 eventMouseButton.ButtonIndex == 1)
        {
            Vector3 from = camera.ProjectRayOrigin(eventMouseButton.Position);
            Vector3 to = camera.ProjectRayNormal(eventMouseButton.Position) * RAY_LENGHT;
            GD.Print(to);
            ray.SetTranslation(from);
            ray.SetCastTo(to);
            ray.Enabled = true;
        }
    }

    public override void _Ready()
    {
        loadRadius = LOAD_RADIUS;

        foreach (Node child in GetParent().GetChildren())
        {
            if (child.Name.Equals("GameController"))
            {
                gameController = (GameController) child;
            }
            else if (child.Name.Equals("Picker"))
            {
                ray = (RayCast) child;
            }
        }

        foreach (Node child in GetChildren())
        {
            if (child.Name.Equals("Camera"))
            {
                camera = (Camera) child;
            }
        }

        foreach (Node child in camera.GetChildren())
        {
            if (child.Name.Equals("GUI"))
            {
                GUI = (CanvasLayer) child;
            }
        }

        foreach (Node child in GUI.GetChildren())
        {
            if (child.Name.Equals("FPS"))
            {
                fps = (Label) child;
            }
            else if (child.Name.Equals("Memory"))
            {
                memory = (Label) child;
            }
            else if (child.Name.Equals("Chunks"))
            {
                chunks = (Label) child;
            } else if (child.Name.Equals("Vertices"))
            {
                vertices = (Label) child;
            }
        }

        //  GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe; 
        // VisualServer.SetDebugGenerateWireframes(true);
        initialRotation = new Vector3();

        picker = gameController.GetPicker();

        generation = new Threading(Begin);

        Input.SetMouseMode(Input.MouseMode.Captured);

        gameController.Prepare(camera);
        lastPosition = this.Translation;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (ray.IsColliding())
        {
            picker.Pick(ray.GetCollisionPoint(), ray.GetCollisionNormal());
            ray.Enabled = false;
        }
    }

    private void Begin()
    {
        gameController.Generate(this);
    }

    public override void _ExitTree()
    {
        Input.SetMouseMode(Input.MouseMode.Visible);
    }

    public override void _Process(float delta)
    {
        chunks.SetText("Chunks: " + gameController.GetChunkCount());
        vertices.SetText("Vertices: " + Performance.GetMonitor(Performance.Monitor.RenderVerticesInFrame));
        fps.SetText("FPS: " + Performance.GetMonitor(Performance.Monitor.TimeFps));
        memory.SetText("Memory: " + Performance.GetMonitor(Performance.Monitor.MemoryStatic) / (1024 * 1024) + " MB");

        Begin();

        if (Input.IsActionPressed("toggle_mouse_capture"))
        {
            Input.SetMouseMode(Input.GetMouseMode() == Input.MouseMode.Captured
                ? Input.MouseMode.Visible
                : Input.MouseMode.Captured);
        }

        if (Input.IsActionPressed("ui_cancel"))
        {
            GetTree().Quit();
        }

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

        motion = motion.Rotated(new Vector3(0, 1, 0), GetRotation().y - initialRotation.y)
            .Rotated(new Vector3(1, 0, 0), (float) Math.Cos(GetRotation().y) * GetRotation().x)
            .Rotated(new Vector3(0, 0, 1), -(float) Math.Sin(GetRotation().y) * GetRotation().x);

        velocity += motion * MOVE_SPEED;

        velocity = new Vector3(velocity.x * 0.9f, velocity.y * 0.9f, velocity.z * 0.9f);

        Translation = new Vector3(Translation.x + (velocity.x * delta), Translation.y + (velocity.y * delta),
            Translation.z + (velocity.z * delta));
    }
}