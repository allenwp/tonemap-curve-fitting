# tonemap-curve-fitting

This Godot project tool has two parts:
1) Visualization of different tonemapping curves and manual tweaking of curve paramters
2) A brute force fitting algorithm that tries to fit a curve based on minimizing error defined by the error algorithm

The brute force fitting is probably best ignored, as it doesn't do as good of a job as Mathematica's `NonlinearModelFit` function when it is configured correctly.

The [sibling repository](https://github.com/allenwp/AgX-GLSL-Shaders) has the Mathematica notebooks used for curve fitting. I'd recommend ignoring the notebooks and text documents in this repository, as they're just scrap notes of mine.
