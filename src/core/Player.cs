using System;
using Godot;
public class Player : Camera {
public static readonly float MOUSE_SENSITIVITY = 0.002F;
private float move_speed = 1.5F;
private Vector3 motion = new Vector3();
private Vector3 velocity = new Vector3();
private Vector3 initialRotation;
private const float RAY_LENGHT = 10;
private RayCast ray;

public override void _Input(InputEvent @event)
{
    if (@event is InputEventMouseMotion eventKey){
        if (Input.GetMouseMode() == Input.MouseMode.Captured){
         this.SetRotation(new Vector3((float)Math.Max(Math.Min(this.GetRotation().x - eventKey.Relative.y * MOUSE_SENSITIVITY, Math.PI/2), -Math.PI/2), GetRotation().y -  eventKey.Relative.x * MOUSE_SENSITIVITY, this.GetRotation().z));
        }
    }else  if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == 1)
    {
          Vector3 from = ProjectRayOrigin(eventMouseButton.Position);
        Vector3 to = from + ProjectRayNormal(eventMouseButton.Position) * RAY_LENGHT;
         ray.CastTo = to;
      GD.Print("Hit Results" + ray.GetCollisionPoint());
    }
   }
	public override void _Ready()
   {
      ray = new RayCast();
      //GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe; 
      //VisualServer.SetDebugGenerateWireframes(true);
      AddChild(ray);
      initialRotation = GetRotation();
   }
   public override void _ExitTree()
  {
     	Input.SetMouseMode(Input.MouseMode.Visible);
  }
   public override void _Process(float delta)
  {

      if (Input.IsActionPressed("toggle_mouse_capture")) {
		if (Input.GetMouseMode() == Input.MouseMode.Captured){
			Input.SetMouseMode(Input.MouseMode.Visible);
      }else{
   			Input.SetMouseMode(Input.MouseMode.Captured);	
        }
    }

     if (Input.IsActionPressed("ui_cancel")){
        GetTree().Quit();
     }

	if (Input.IsActionPressed("walk_left")){
		motion.x = -1;
   }else if(Input.IsActionPressed("walk_right")){
		motion.x = 1;
   }else{
		motion.x = 0;
   }

	if (Input.IsActionPressed("walk_forward")){
		motion.z = 1;
   } else if (Input.IsActionPressed("walk_backward")){
		motion.z = -1;
   }else{
		motion.z = 0;
   }

	if (Input.IsActionPressed("move_up")){
		motion.y = 1;
   }else if (Input.IsActionPressed("move_down")){
		motion.y = -1;
   }else{
		motion.y = 0;
   }

	motion = motion.Normalized();

	if (Input.IsActionPressed("move_speed")){
		motion *= 2;
   }

	motion = motion.Rotated(new Vector3(0, 1, 0), GetRotation().y - initialRotation.y)
		.Rotated(new Vector3(1, 0, 0), (float)Math.Cos(GetRotation().y)* GetRotation().x)
		.Rotated(new Vector3(0, 0, 1), -(float)Math.Sin(GetRotation().y)* GetRotation().x);

	velocity += motion*move_speed;

	velocity = new Vector3(velocity.x * 0.9f, velocity.y * 0.9f, velocity.z * 0.9f);

	Translation = new Vector3(Translation.x + (velocity.x * delta), Translation.y + (velocity.y * delta), Translation.z + (velocity.z * delta));
  }
}