@tool
class_name CurveView
extends Node2D

enum EncodingType { LINEAR, LOG2}

@export var curves: Node

@export var num_points: int = 1000

@export var middle_grey: float = 0.18
@export var x_encoding_type: EncodingType = EncodingType.LINEAR
@export var y_encoding_type: EncodingType = EncodingType.LINEAR
@export var linear_min_x: float = 0.0
@export var linear_max_x: float = 16.2917402385381
@export var linear_min_y: float = 0.0
@export var linear_max_y: float = 1.0
@export var log2_min_x: float = -12.0
@export var log2_max_x: float = 4.0
@export var log2_min_y: float = -12.0
@export var log2_max_y: float = 4.0
@export var clip: bool = true
@export var show_linear: bool = true

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if !curves:
		return

	match x_encoding_type:
		EncodingType.LINEAR:
			%XLowerLable.text = "%.2f" % linear_min_x
			%XUpperLable.text = "%.2f" % linear_max_x
			%XLable.text = "Input (linear scale)"
			%XMiddleGreyLine.position.x = (middle_grey - linear_min_x) / (linear_max_x - linear_min_x) * 1000.0
			%X1Line.position.x = (1.0 - linear_min_x) / (linear_max_x - linear_min_x) * 1000.0
			%XWhiteLine.position.x = (curves.white - linear_min_x) / (linear_max_x - linear_min_x) * 1000.0
		EncodingType.LOG2:
			%XLowerLable.text = "%+.2f" % log2_min_x
			%XUpperLable.text = "%+.2f" % log2_max_x
			%XLable.text = "Input (log2 scale, middle grey: %.2f)" % middle_grey
			%XMiddleGreyLine.position.x = (log2(middle_grey / middle_grey) + abs(log2_min_x)) / (log2_max_x - log2_min_x) * 1000.0
			%X1Line.position.x = (log2(1.0 / middle_grey) + abs(log2_min_x)) / (log2_max_x - log2_min_x) * 1000.0
			%XWhiteLine.position.x = (log2(curves.white / middle_grey) + abs(log2_min_x)) / (log2_max_x - log2_min_x) * 1000.0
	%XMiddleGreyLine.visible = %XMiddleGreyLine.position.x >= 0.0 && %XMiddleGreyLine.position.x <= 1000.0
	%X1Line.visible = %X1Line.position.x >= 0.0 && %X1Line.position.x <= 1000.0
	%XWhiteLine.visible = %XWhiteLine.position.x >= 0.0 && %XWhiteLine.position.x <= 1000.0

	match y_encoding_type:
		EncodingType.LINEAR:
			%YLowerLable.text = "%.2f" % linear_min_y
			%YUpperLable.text = "%.2f" % linear_max_y
			%YLable.text = "Output (linear scale)"
			%YMiddleGreyLine.position.y = (1.0 - (middle_grey - linear_min_y) / (linear_max_y - linear_min_y)) * 1000.0
			%Y1Line.position.y = (1.0 - (1.0 - linear_min_y) / (linear_max_y - linear_min_y)) * 1000.0
		EncodingType.LOG2:
			%YLowerLable.text = "%+.2f" % log2_min_y
			%YUpperLable.text = "%+.2f" % log2_max_y
			%YLable.text = "Output (log2 scale, middle grey: %.2f)" % middle_grey
			%YMiddleGreyLine.position.y = (1.0 - (log2(middle_grey / middle_grey) - log2_min_y) / (log2_max_y - log2_min_y)) * 1000.0
			%Y1Line.position.y = (1.0 - (log2(1.0 / middle_grey) - log2_min_y) / (log2_max_y - log2_min_y)) * 1000.0
	%YMiddleGreyLine.visible = %YMiddleGreyLine.position.y >= 0.0 && %YMiddleGreyLine.position.y <= 1000.0
	%Y1Line.visible = %Y1Line.position.y >= 0.0 && %Y1Line.position.y <= 1000.0

	var linear_points: PackedVector2Array
	var reference_points: PackedVector2Array
	var approx_points: PackedVector2Array
	for i in range(num_points):
		var x: float

		match x_encoding_type:
			EncodingType.LINEAR:
				x = (float(i) / (num_points - 1)) * (linear_max_x - linear_min_x) + linear_min_x
			EncodingType.LOG2:
				x = (float(i) / (num_points - 1)) * (log2_max_x - log2_min_x) + log2_min_x
				x = pow(2.0, x) * middle_grey # convert from log2 encoding to linear encoding

		var y_linear: float = x
		var y_reference: float = curves.ReferenceCurve(x)
		var y_approx: float = curves.ApproxCurve(x)

		x = prepare_x_for_graph(x)

		y_linear = prepare_y_for_graph(y_linear)
		y_reference = prepare_y_for_graph(y_reference)
		y_approx = prepare_y_for_graph(y_approx)

		if show_linear && (x >= 0.0 && x <= 1000.0 && y_linear <= 0.0 && y_linear >= -1000.0):
			linear_points.push_back(Vector2(x, y_linear))
		if !clip || (x >= 0.0 && x <= 1000.0 && y_reference <= 0.0 && y_reference >= -1000.0):
			reference_points.push_back(Vector2(x, y_reference))
		if !clip || (x >= 0.0 && x <= 1000.0 && y_approx <= 0.0 && y_approx >= -1000.0):
			approx_points.push_back(Vector2(x, y_approx))

	%LinearLine.points = linear_points
	%ReferenceLine.points = reference_points
	%ApproxLine.points = approx_points

	var reference_inflection: Vector2 = curves.reference_inflection_point
	if reference_inflection.x > 0:
		reference_inflection = Vector2(prepare_x_for_graph(reference_inflection.x), prepare_y_for_graph(reference_inflection.y))
		if reference_inflection.x >= 0.0 && reference_inflection.x <= 1000.0 && reference_inflection.y <= 0.0 && reference_inflection.y >= -1000.0:
			%ReferenceInflectionMiddle.position = reference_inflection
			%ReferenceInflectionMiddle.position.y += 1000.0
			%ReferenceInflectionMiddle.visible = true
		else:
			%ReferenceInflectionMiddle.visible = false
	else:
		%ReferenceInflectionMiddle.visible = false

	var approx_inflection: Vector2 = curves.approx_inflection_point
	if approx_inflection.x > 0:
		approx_inflection = Vector2(prepare_x_for_graph(approx_inflection.x), prepare_y_for_graph(approx_inflection.y))
		if approx_inflection.x >= 0.0 && approx_inflection.x <= 1000.0 && approx_inflection.y <= 0.0 && approx_inflection.y >= -1000.0:
			%ApproxInflectionMiddle.position = approx_inflection
			%ApproxInflectionMiddle.position.y += 1000.0
			%ApproxInflectionMiddle.visible = true
		else:
			%ApproxInflectionMiddle.visible = false
	else:
		%ApproxInflectionMiddle.visible = false

func prepare_x_for_graph(x: float) -> float:
	# scale x from linear encoding to match the [0,1000] graph range
	match x_encoding_type:
		EncodingType.LINEAR:
			x = (x - linear_min_x) / (linear_max_x - linear_min_x) * 1000.0
		EncodingType.LOG2:
			x = (log2(x / middle_grey) + abs(log2_min_x)) / (log2_max_x - log2_min_x) * 1000.0
	return x


func prepare_y_for_graph(y: float) -> float:
	# scale y from linear encoding to match the [0,-1000] graph range
	match y_encoding_type:
		EncodingType.LINEAR:
			y = (y - linear_min_y) / (linear_max_y - linear_min_y) * -1000.0
		EncodingType.LOG2:
			y = maxf(1e-10, y / middle_grey) # prevent log2(0)
			y = (log2(y) - log2_min_y) / (log2_max_y - log2_min_y) * -1000.0
	return y

#func inflection_points(points: PackedVector2Array) -> PackedVector2Array:
	#var inflection_indices: PackedVector2Array
	#if points.size() < 3:
		#return inflection_indices  # Need at least 3 points to check for curvature change
#
	#for i in range(1, points.size() - 1):
		#var v1 = points[i] - points[i - 1]  # First segment vector
		#var v2 = points[i + 1] - points[i]  # Second segment vector
#
		## Compute the cross product to determine signed curvature
		#var cross_product = v1.cross(v2)
#
		## Check sign change in cross product
		#if i > 1:
			#var prev_cross_product = (points[i-1] - points[i-2]).x * v1.y - (points[i-1] - points[i-2]).y * v1.x
			#if sign(prev_cross_product) != sign(cross_product):
				#inflection_indices.append(points[i])  # Inflection occurs at points[i]
#
	#return inflection_indices

func inflection_points(points: PackedVector2Array) -> PackedVector2Array:
	var inflections: PackedVector2Array
	var positive: bool = true
	for i in range(1, points.size() - 1):
		var v1: Vector2 = points[i] - points[i - 1]
		var v2: Vector2 = points[i + 1] - points [i]
		if v1.cross(v2) < 0:
			if positive:
				positive = false
				if i > 1:
					inflections.push_back((points[i]))
		else:
			if !positive:
				positive = true
				if i > 1:
					inflections.push_back(points[i])
	return inflections


func log2(value: float) -> float:
	return log(value) / log(2)
