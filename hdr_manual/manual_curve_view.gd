@tool
extends CurveView

func _process(_delta: float) -> void:
	super._process(_delta);
	var sm: ShaderMaterial = %XGradient.material as ShaderMaterial
	sm.set_shader_parameter("max_value", linear_max_x)
	sm = %YGradient.material as ShaderMaterial
	sm.set_shader_parameter("max_value", linear_max_y)
	
	
