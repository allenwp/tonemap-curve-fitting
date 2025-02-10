using Godot;
using System;

[Tool]
public partial class CurveComparison : Node
{
    [Export]
    public bool OptionB { get; set; } = false;


    [Export]
    public float agxRefLog2MiddleGrey = 0.18f;
    [Export]
    public float agxRefLog2Min = -10.0f;
    [Export]
    public float agxRefLog2Max = 6.5f;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }

    public double ReferenceCurve(double x)
    {
        return AgxReference(x);
    }

    public double ApproxCurve(double x)
    {
        if (OptionB)
        {
            return AgXReferenceSimplified(x);
        }
        else
        {
            return RationalInterpolation(x);
        }
    }

    public static double KrzysztofNarkowiczACES(double x)
    {
        x *= 0.6;
        double a = 2.51;
        double b = 0.03;
        double c = 2.43;
        double d = 0.59;
        double e = 0.14;
        return (x * (a * x + b)) / (x * (c * x + d) + e);
    }

    public static double RationalInterpolation(double x)
    {
        return (-0.00169047 + 1.49227 * x + 0.194039 * x * x) / (1 + 1.86073 * x + 0.167678 * x * x);
    }

    public static double MinimaxApproximation(double x)
    {
        return (-0.000264666 + 1.50561 * x + 0.225389 * x * x) / (1 + 1.91729 * x + 0.196494 * x * x);
    }

    #region AgX Reference

    private double ScaleFunction(double transitionX, double transitionY, double power, double slope)
    {
        double termA = Math.Pow(slope * (1.0 - transitionX), -1.0 * power);
        double termB = Math.Pow((slope * (1.0 - transitionX)) / (1.0 - transitionY), power) - 1.0;
        return Math.Pow(termA * termB, -1.0 / power);
    }

    private double ExponentialCurve(double xIn, double scaleInput, double slope, double power, double transitionX, double transitionY)
    {
        double input = (slope * (xIn - transitionX)) / scaleInput;
        double result = (scaleInput * Exponential(input, power)) + transitionY;
        return Math.Max(result, 0.0); // Clipping to avoid negative values due to rounding errors
    }

    private double Exponential(double xIn, double power)
    {
        return xIn / Math.Pow(1 + Math.Pow(xIn, power), 1 / power);
    }

    private double CalculateSigmoid(double x)
    {
        const double slope = 2.4;
        const double power = 1.5;
        double pivotX = Math.Abs(agxRefLog2Min / (agxRefLog2Max - agxRefLog2Min));
        double pivotY = Math.Pow(agxRefLog2MiddleGrey, 1.0 / 2.4);

        double scaleValue = (x < pivotX)
            ? -1.0 * ScaleFunction(1.0 - pivotX, 1.0 - pivotY, power, slope)
            : ScaleFunction(pivotX, pivotY, power, slope);

        return ExponentialCurve(x, scaleValue, slope, power, pivotX, pivotY);
    }

    private double LogEncodingLog2(double lin, double middleGrey, double minExposure, double maxExposure)
    {
        double lg2 = Math.Log2(lin / middleGrey);
        double logNorm = (lg2 - minExposure) / (maxExposure - minExposure);
        return logNorm; // Might be negative, but negatives are clipped later.
    }

    public double AgxReference(double color)
    {
        color = Math.Max(color, 1e-10);

        color = LogEncodingLog2(color, agxRefLog2MiddleGrey, agxRefLog2Min, agxRefLog2Max);

        // Apply sigmoid function approximation.
        color = CalculateSigmoid(color);

        // Convert back to linear.
        color = Math.Pow(color, 2.4);

        return color;
    }

    public double AgXReferenceSimplified(double color)
    {
        const double NormalizedLog2Minimum = -10.0;
        const double MidGrey = 0.18;
        const double Power = 1.5;
        const double InversePower = 1.0 / Power;

        const double LOG2_MAX = 6.5; // Define the LOG2_MAX value

        double normalizedLog2Maximum = LOG2_MAX;
        double logRange = normalizedLog2Maximum - NormalizedLog2Minimum;

        color = (Math.Log2(color / MidGrey) - NormalizedLog2Minimum) / logRange;
        color = Math.Max(color, 0.0);

        double xPivot = 10.0 / logRange;
        double pivotDistance = xPivot - color;

        double aBottom = (10.858542784410849080 - (1.0 / Math.Pow(xPivot, Power))) * Math.Pow(pivotDistance, Power);
        double aTop = (-1 + 10.191614048660063014 * Math.Pow(1.0 - xPivot, Power)) / Math.Pow((xPivot - 1.0) / pivotDistance, Power);

        double A = (color < xPivot) ? aBottom : aTop;

        color = 0.48943708957387834110 + ((-2.4 * xPivot) + (2.4 * color)) / Math.Pow(1.0 + A, InversePower);
        color = Math.Pow(color, 2.4);

        return color;
    }
    #endregion

}
