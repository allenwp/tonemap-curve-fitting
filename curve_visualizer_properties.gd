extends Node

@export var curve_comparison: Node
@export var label: Label
@export var out_max_label: Label
@export var out_max_label_backing: CanvasItem
@export var animation: AnimationPlayer
@export var mesh: MeshInstance3D

@export var contrast: float = 1.5
@export var high_clip: float = 12.0
@export var ref_luminance: float = 100
@export var max_luminance: float = 100
var output_max_value: float = 1.0
@export var low_clip: float = 0.0
@export var brightness: float = 0.0

enum CURVE_FEATURE { contrast, high_clip, output_max_value, low_clip, brightness }

@export var curve_feature: CURVE_FEATURE = CURVE_FEATURE.contrast

func _ready() -> void:
	get_window().hdr_output_auto_adjust_reference_luminance = false
	
	match curve_feature:
		CURVE_FEATURE.contrast:
			animation.play("contrast")
		CURVE_FEATURE.high_clip:
			animation.play("high_clip")
		CURVE_FEATURE.output_max_value:
			animation.play("output_max_value")
		CURVE_FEATURE.low_clip:
			animation.play("low_clip")
		CURVE_FEATURE.brightness:
			animation.play("brightness")

func _process(_delta: float) -> void:
	output_max_value = max_luminance / ref_luminance
	out_max_label.text = ""
	out_max_label_backing.visible = false
	
	mesh.material_override.set("shader_parameter/awp_contrast", contrast)
	curve_comparison.A = contrast;
	mesh.material_override.set("shader_parameter/awp_high_clip_uniform", high_clip)
	curve_comparison.white = high_clip;
	mesh.material_override.set("shader_parameter/output_max_value", output_max_value)
	curve_comparison.max_value = output_max_value;
	
	get_window().hdr_output_reference_luminance = ref_luminance;
	
	if curve_feature == CURVE_FEATURE.contrast:
		label.text = "Contrast: %0.2f" % contrast
	if curve_feature == CURVE_FEATURE.high_clip:
		label.text = "High Clip: %0.2f" % high_clip
	if curve_feature == CURVE_FEATURE.output_max_value:
		label.text = ""
		out_max_label_backing.visible = true
		out_max_label.text = "Reference White Luminance: %.0f nits\nMax Luminance: %.0f nits\nOutput Max Value: %0.2f" % [ref_luminance, max_luminance, output_max_value]


func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if OS.has_feature("movie"):
		get_tree().quit()
