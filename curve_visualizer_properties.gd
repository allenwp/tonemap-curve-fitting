extends Node

@export var curve_comparison: Node
@export var label: Label
@export var animation: AnimationPlayer
@export var mesh: MeshInstance3D

@export var contrast: float = 1.25
@export var high_clip: float = 3.0
@export var output_max_value: float = 1.0
@export var crossover_point: float = 0.18
@export var low_clip: float = 0.0
@export var brightness: float = 0.0

enum CURVE_FEATURE { contrast, high_clip, output_max_value, crossover_point, low_clip, brightness }

@export var curve_feature: CURVE_FEATURE = CURVE_FEATURE.contrast

func _ready() -> void:
	match curve_feature:
		CURVE_FEATURE.contrast:
			animation.play("contrast")
		CURVE_FEATURE.high_clip:
			animation.play("high_clip")
		CURVE_FEATURE.output_max_value:
			animation.play("output_max_value")
		CURVE_FEATURE.crossover_point:
			animation.play("crossover_point")
		CURVE_FEATURE.low_clip:
			animation.play("low_clip")
		CURVE_FEATURE.brightness:
			animation.play("brightness")

func _process(_delta: float) -> void:
	
	mesh.material_override.set("shader_parameter/awp_contrast", contrast)
	curve_comparison.A = contrast;
	mesh.material_override.set("shader_parameter/awp_high_clip_uniform", high_clip)
	curve_comparison.white = high_clip;
	mesh.material_override.set("shader_parameter/awp_crossover_point", crossover_point)
	curve_comparison.agxRefMiddleGrey = crossover_point;
	
	if curve_feature == CURVE_FEATURE.contrast:
		label.text = "Contrast: %0.2f" % contrast
	if curve_feature == CURVE_FEATURE.high_clip:
		label.text = "High Clip: %0.2f" % high_clip
	if curve_feature == CURVE_FEATURE.crossover_point:
		label.text = "High Clip: %0.2f\nCrossover Point: %0.2f" % [high_clip, crossover_point]
	
	#mesh.material_override.set("shader_parameter/output_max_value", output_max_value)
	#curve_comparison.crossover_point = crossover_point;
	#if curve_feature == CURVE_FEATURE.crossover_point:
		#label.text = "Output Max Value: %0.2f" % output_max_value
	
