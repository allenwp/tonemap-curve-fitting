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

func _ready() -> void:
	pass
	#animation.play("contrast_animation")

func _process(_delta: float) -> void:
	mesh.material_override.set("shader_parameter/awp_contrast", contrast)
	curve_comparison.A = contrast;
	label.text = "Contrast: " + str(contrast)
