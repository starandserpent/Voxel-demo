extends Camera

const MOUSE_SENSITIVITY = 0.002


# The camera movement speed (tweakable using the mouse wheel)
var move_speed = 1.5

# Stores where the camera is wanting to go (based on pressed keys and speed modifier)
var motion = Vector3()

# Stores the effective camera velocity
var velocity = Vector3()

# The initial camera node rotation
var initial_rotation = self.rotation.y

func _input(event):
	# Mouse look (effective only if the mouse is captured)
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		# Horizontal mouse look
		rotation.y -= event.relative.x*MOUSE_SENSITIVITY
		# Vertical mouse look, clamped to -90..90 degrees
		rotation.x = clamp(rotation.x - event.relative.y*MOUSE_SENSITIVITY, deg2rad(-90), deg2rad(90))


	# Toggle HUD
#	if event.is_action_pressed("toggle_hud"):
#		FPSCounter.visible = !FPSCounter.visible

	# These actions do not make sense when the settings GUI is visible, hence the check
#	if not SettingsGUI.visible:
#		# Toggle mouse capture (only while the menu is not visible)
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

		# Movement speed change

func _process(delta):
	
	if Input.is_action_just_pressed("ui_cancel"):
		get_tree().quit()

	# Movement

	if Input.is_action_pressed("walk_left"):
		motion.x = -1
	elif Input.is_action_pressed("walk_right"):
		motion.x = 1
	else:
		motion.x = 0

	if Input.is_action_pressed("walk_forward"):
		motion.z = 1
	elif Input.is_action_pressed("walk_backward"):
		motion.z = -1
	else:
		motion.z = 0

	if Input.is_action_pressed("move_up"):
		motion.y = 1
	elif Input.is_action_pressed("move_down"):
		motion.y = -1
	else:
		motion.y = 0

	# Normalize motion
	# (prevents diagonal movement from being `sqrt(2)` times faster than straight movement)
	motion = motion.normalized()

	# Speed modifier
	if Input.is_action_pressed("move_speed"):
		motion *= 2

	# Rotate the motion based on the camera angle
	motion = motion \
		.rotated(Vector3(0, 1, 0), rotation.y - initial_rotation) \
		.rotated(Vector3(1, 0, 0), cos(rotation.y)*rotation.x) \
		.rotated(Vector3(0, 0, 1), -sin(rotation.y)*rotation.x)

	# Add motion
	velocity += motion*move_speed

	# Friction
	velocity *= 0.9

	# Apply velocity
	translation += velocity*delta


func _exit_tree():
	# Restore the mouse cursor upon quitting
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)