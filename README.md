# tonemap-curve-fitting

This Godot project tool has two parts:
1) Visualization of different tonemapping curves and manual tweaking of curve paramters
2) A brute force fitting algorithm that tries to fit a curve based on minimizing error defined by the error algorithm

The brute force fitting is probably best ignored, as it doesn't do as good of a job as Mathematica's `NonlinearModelFit` function when it is configured correctly.

The [sibling repository](https://github.com/allenwp/AgX-GLSL-Shaders) has the Mathematica notebooks used for curve fitting. I'd recommend ignoring the notebooks and text documents in this repository, as they're just scrap notes of mine.

# Version
This project is made with Godot 4.3 with .NET support.

# Usage
The CurveComparison C# script in the main scene is the main point for configuration. It is meant to be configured by changing the script itself and using the properties in the Godot inspector. `public double ReferenceCurve(double x)` allows you to change the reference curve (white line) and `public double ApproxCurve(double x)` allows you to change the approximation curve (orange line). The approximation curve has two options that can be switched with the `Option B` property in the inspector.

## Log scale
Instead of simple base 2 log scale, I am scaling by middle grey:

- log2 encoding: `log2(x / middle_grey)`
- inverse linearlization: `pow(2, x) * middle_grey`

This is the convetion used by AgX for curve design, so I caried that over. I makes it so that the `log2` evalulation of `middle_grey` equals `0.0`.

## Curve views
6 curve views are in the main scene. Each of these can be configured in the inspector. The **bottom left four curve views** are the ones that are best for getting a quick view of the curve.

### Lines
- White: reference curve
- Orange: approximation curve
- Grey: unity curve (x = y)
- Grey X/Y intersection marker: middle grey (default to 18% (0.18))
- Dark yellow X/Y intersection marker: 100% (1.0)
- Vertical white: tonemapper `white` value/parameter
- `+` marker: approximate curve inflection point (only works when the curve has a single inflection point)

### HDR
To view HDR, you'll need to change the `Linear Max Y` value on the views that show a linear scale on the Y axis.
