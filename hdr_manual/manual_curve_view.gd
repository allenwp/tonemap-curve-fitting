@tool
extends CurveView

var Configurations: Array[Dictionary] = [{
	"title":"SDR (no tonemapping)",
	"show_white":false,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":1.0,
	"white":1.0,
	"reinhard":false,
	},{
	"title":"HDR (no tonemapping)",
	"show_white":false,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":3.0,
	"white":1.0,
	"reinhard":false,
	},{
	"title":"SDR (Reinhard)",
	"show_white":true,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":1.0,
	"white":1.0,
	"reinhard":true,
	},{
	"title":"SDR (Reinhard)",
	"show_white":true,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":1.0,
	"white":3.0,
	"reinhard":true,
	},{
	"title":"HDR (Reinhard)",
	"show_white":true,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":1.0,
	"white":3.0,
	"reinhard":true,
	},{
	"title":"HDR (Reinhard)",
	"show_white":true,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":2.0,
	"white":3.0,
	"reinhard":true,
	},{
	"title":"HDR (Reinhard)",
	"show_white":true,
	"max_x":4.0,
	"max_y":4.0,
	"max_value":3.0,
	"white":3.0,
	"reinhard":true,
	}]

@export var current_config: int = 0

func _process(_delta: float) -> void:
	_constrain_config()
	
	curves.max_value = Configurations[current_config]["max_value"]
	linear_max_x = Configurations[current_config]["max_x"]
	linear_max_y = Configurations[current_config]["max_y"]
	show_white = Configurations[current_config]["show_white"]
	curves.white = Configurations[current_config]["white"]
	curves.OptionB = Configurations[current_config]["reinhard"]
	super._process(_delta);
	
	_constrain_config()
	
	var sm: ShaderMaterial = %XGradient.material as ShaderMaterial
	sm.set_shader_parameter("max_value", linear_max_x)
	sm = %YGradient.material as ShaderMaterial
	sm.set_shader_parameter("max_value", linear_max_y)
	
	%Title.text = str(Configurations[current_config]["title"])
	%XLable.text = "Value in Godot scene"
	%YLable.text = "Value shown on screen"
	
func _constrain_config() -> void:
	if current_config < 0:
		current_config = 0
	elif current_config >= Configurations.size():
		current_config = Configurations.size() - 1
