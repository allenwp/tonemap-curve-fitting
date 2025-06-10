shader_type spatial;
render_mode unshaded;

uniform sampler2D image;
uniform bool use_side_by_side = false;
uniform float side_by_side = 0.5;

uniform float exposure = 1.5;

uniform float awp_contrast = 1.5; // Should be 1.0 or larger
uniform float awp_high_clip_uniform = 12.0;

// This constant must match the one in the shader code.
// 18% "middle grey" is perceptually 50% of the lightness of reference white.
uniform float awp_crossover_point = 0.1841865;

uniform float output_max_value = 1.0;

/*
Copyright (c) 2025 Allen Pestaluky

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// allenwp tonemapping curve; developed for use in the Godot game engine
// Source and details: https://allenwp.com/blog/2025/05/29/allenwp-tonemapping-curve/
// Input must be a linear scene value
vec3 allenwp_curve(vec3 x,
		float awp_toe_a,
		float awp_slope,
		float awp_w,
		float awp_shoulder_max) {
	// This constant must match the CPU-side code that calculates the parameters.
	// 18% "middle grey" is perceptually 50% of the lightness of reference white.
	//const float awp_crossover_point = 0.1841865;

	x = max(x, 0.0); // Negative input causes undefined behaviour from pow function!

	// Reinhard-like shoulder:
	vec3 s = x - awp_crossover_point;
	vec3 slope_s = awp_slope * s;
	s = slope_s * (1.0 + s / awp_w) / (1.0 + (slope_s / awp_shoulder_max));
	s += awp_crossover_point;

	// Sigmoid power function toe:
	vec3 t = pow(x, vec3(awp_contrast));
	t = t / (t + awp_toe_a);

	return mix(s, t, lessThan(x, vec3(awp_crossover_point)));
}

/*
Copyright (c) 2025 Allen Pestaluky

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

void fragment() {
	vec3 colour;
	
	colour = texture(image, UV).rgb * exposure;
	
	float max_value = output_max_value;
	if (use_side_by_side && UV.x > side_by_side) {
		max_value = 1.0;
	}
	
	// allenwp tonemapping curve; developed for use in the Godot game engine
	// Source and details: https://allenwp.com/blog/2025/05/29/allenwp-tonemapping-curve/
	//void allenwp_curve_cpu_code()
	{
		// TODO: Run this part of the allenwp tonemapping curve code on the
		// CPU and pass in the calculated parameters as uniforms.

		// allenwp tonemapping curve user parameters:
		//float awp_contrast = 1.25; // Should be 1.0 or larger
		//float awp_high_clip = 16.0;

		// This constant must match the one in the shader code.
		// 18% "middle grey" is perceptually 50% of the lightness of reference white.
		//float awp_crossover_point = 0.1841865;

		// Use one of the following four approaches to get your output_max_value:

		// 1) SDR
		//float reference_white_luminance_nits = 100.0;
		//float max_luminance_nits = 100.0;
		//float output_max_value = max_luminance_nits / reference_white_luminance_nits;

		// 2) Traditional HDR
		//float reference_white_luminance_nits = 100.0;
		//float max_luminance_nits = 1000.0;
		//float output_max_value = max_luminance_nits / reference_white_luminance_nits;

		// 3) Variable Extended Dynamic Range (EDR) on Apple
		//float output_max_value = maximumExtendedDynamicRangeColorComponentValue;

		// 4) Variable Extended Dynamic Range (EDR) on Windows or similar
		//float reference_white_luminance_nits = get_sdr_content_brightness_nits();
		//float max_luminance_nits = get_max_luminance_nits();
		//float output_max_value = max_luminance_nits / reference_white_luminance_nits;

		// Calculate allenwp tonemapping curve parameters on the CPU to improve shader performance:

		// Ensure that the Reinhard-like shoulder always behaves nicely in EDR across
		// all ranges of output_max_value (such as when awp_high_clip is less than output_max_value):
		float awp_high_clip = max(awp_high_clip_uniform, max_value);

		// awp_toe_a is a solution generated by Mathematica that ensures intersection at awp_crossover_point
		float awp_toe_a = ((1.0 / awp_crossover_point) - 1.0) * pow(awp_crossover_point, awp_contrast);
		// Slope formula is simply the derivative of the toe function with an input of awp_crossover_point
		float awp_slope_denom = pow(awp_crossover_point, awp_contrast) + awp_toe_a;
		float awp_slope = (awp_contrast * pow(awp_crossover_point, awp_contrast - 1.0) * awp_toe_a) / (awp_slope_denom * awp_slope_denom);

		float awp_shoulder_max = max_value - awp_crossover_point;
		float awp_w = awp_high_clip - awp_crossover_point;
		awp_w = awp_w * awp_w;
		awp_w = awp_w / awp_shoulder_max;
		awp_w = awp_w * awp_slope;

		// Use the allenwp curve to support variable / extended dynamic range (EDR, SDR, and HDR):
		colour = allenwp_curve(colour,
			awp_toe_a,
			awp_slope,
			awp_w,
			awp_shoulder_max);
		ALBEDO = min(colour, vec3(output_max_value));
		
		float line_width = 0.0001;
		if (UV.y < 0.455 && UV.y > 0.435 && use_side_by_side && UV.x > side_by_side - line_width && UV.x < side_by_side + line_width) {
			ALBEDO = vec3(0);
		}
	}
}
