extends Camera


var currVal:String
#Shader params
var horizontalFOV:float = 140.0
var strength:float = 0.5
var cylindricalRatio:float = 2
var height:float = 0;
var screenSize = Vector2(0,0)
var aspect:float = 0.0


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _enter_tree():
	horizontalFOV = fov
	$GUI/HUDCanvas/ShaderMenu/CheckButton.pressed = false
	screenSize.x = get_viewport().get_visible_rect().size.x # Get Width
	screenSize.y = get_viewport().get_visible_rect().size.y # Get Height
	aspect = screenSize.x/screenSize.y;
	height = tan(deg2rad(horizontalFOV)/2.0)/aspect
	
	fov = atan(height)*2*180/PI
	$GUI/HUDCanvas/ShaderMenu/FOVSlider.value = fov
	$GUI/HUDCanvas/ShaderMenu/BarrelPower.text = str(strength)
	$GUI/HUDCanvas/ShaderMenu/ShaderPropLabel2.text = "FOV: "+str(horizontalFOV)
	$GUI/HUDCanvas/ShaderMenu/cylinratio.value = cylindricalRatio
	
	$GUI/BarrelDist.material.set_shader_param("strength",strength)
	$GUI/BarrelDist.material.set_shader_param("height",height)
	$GUI/BarrelDist.material.set_shader_param("aspectRatio",aspect)
	$GUI/BarrelDist.material.set_shader_param("cylindricalRatio",cylindricalRatio)
	
func _input(event):
	
	if event.is_action_pressed("toggle_mouse_capture"):
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			$GUI/HUDCanvas/ShaderMenu/BarrelPower.release_focus()
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

#uniform float BarrelPower =1.1;
func _on_BarrelPower_text_changed(new_text:String):
	if(new_text.is_valid_float()):
		currVal = new_text
		$GUI/BarrelDist.material.set_shader_param("strength",currVal)
	else:
		$GUI/HUDCanvas/ShaderMenu/BarrelPower.text = currVal


func _on_FOVSlider_value_changed(value:float):
	horizontalFOV = value
	height = tan(deg2rad(horizontalFOV)/2.0)/aspect
	
	fov = atan(height)*2*180/PI
	$GUI/HUDCanvas/ShaderMenu/ShaderPropLabel2.text = "FOV: "+str(horizontalFOV)
	$GUI/BarrelDist.material.set_shader_param("height",height)


func _on_CheckButton_pressed():
	if($GUI/HUDCanvas/ShaderMenu/CheckButton.pressed):
		$GUI/BarrelDist.show()
	else:
		$GUI/BarrelDist.hide()


func _on_cylinratio_value_changed(value:float):
	cylindricalRatio = value
	$GUI/HUDCanvas/ShaderMenu/ShaderPropLabel4.text = "Cyln Ratio: "+str(cylindricalRatio)
	$GUI/BarrelDist.material.set_shader_param("cylindricalRatio",cylindricalRatio)
