shader_type canvas_item;
render_mode unshaded;

uniform float max_value = 1.0;

void fragment() {
	float value = UV.y * max_value;
	COLOR = vec4(value, value, value, 1.0);
}
