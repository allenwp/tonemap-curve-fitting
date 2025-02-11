using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Transactions;

[Tool]
public partial class CurveComparison : Node
{
    [Export]
    public bool OptionB { get; set; } = false;

    public void set_option_b(bool value)
    {
        OptionB = value;
    }


    // default to self_shadow ACES:
    [Export] public double A = 1.0;
    [Export] public double B = 0.0245786;
    [Export] public double C = -0.000090537;
    [Export] public double D = 0.983729;
    [Export] public double E = 0.4329510;
    [Export] public double F = 0.238081;
    [Export] public double G = 0.0;

    public double agxRefLog2MiddleGrey = 0.18f;
    [Export]
    public double agxRefLog2Min = -10.0f;
    [Export]
    public double agxRefLog2Max = 6.5f;

    public struct ErrorValue
    {
        public double Input;
        public double Reference;
        public double Approx;
        public double ErrorLinear;
        public double ErrorLog2;
        public double ErrorWeight;
    }

    public System.Collections.Generic.List<ErrorValue> errorValues = new System.Collections.Generic.List<ErrorValue>();

	public override void _Process(double delta)
	{
        Tree tree = GetNode<Tree>("%ErrorTree");
        tree.Clear();
        tree.SetColumnTitle(0, "Input");
        tree.SetColumnTitle(1, "Reference");
        tree.SetColumnTitle(2, "Approx");
        tree.SetColumnTitle(3, "ErrorLinear");
        tree.SetColumnTitle(4, "ErrorLog2");
        tree.SetColumnTitle(5, "ErrorWeight");
        TreeItem root = tree.CreateItem();

        errorValues.Clear();

        AddValue(Math.Pow(2, agxRefLog2Min) * agxRefLog2MiddleGrey, 2.0);
        AddValue(Math.Pow(2, -9.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -8.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -7.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -6.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -5.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -4.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -3.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -2.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, -1.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(agxRefLog2MiddleGrey, 2.0);
        AddValue(Math.Pow(2, 1.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, 2.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, 3.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, 4.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, 5.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, 6.0) * agxRefLog2MiddleGrey, 1.0);
        AddValue(Math.Pow(2, agxRefLog2Max) * agxRefLog2MiddleGrey, 2.0);

        double totalErrorLinear = 0;
        double totalErrorLog2 = 0;
        foreach (ErrorValue value in errorValues)
        {
            totalErrorLinear += value.ErrorLinear * value.ErrorWeight;
            totalErrorLog2 += value.ErrorLog2 * value.ErrorWeight;
            TreeItem treeItem = tree.CreateItem();
            treeItem.SetText(0, $"{value.Input:F7}");
            treeItem.SetText(1, $"{value.Reference:F7}");
            treeItem.SetText(2, $"{value.Approx:F7}");
            treeItem.SetText(3, $"{value.ErrorLinear:F7}");
            treeItem.SetText(4, $"{value.ErrorLog2:F7}");
            treeItem.SetText(5, $"{value.ErrorWeight:F7}");
        }
        GetNode<Label>("%TotalErrorLinearLabel").Text = $"Total weighted error (linear): {totalErrorLinear:F7}";
        GetNode<Label>("%TotalErrorLog2Label").Text = $"Total weighted error (log2, middle grey: {agxRefLog2MiddleGrey:F2}): {totalErrorLog2:F7}";
        GetNode<TextEdit>("%RationalApproxTextEdit").Text = $"(x * (x * {A:F15} + ({B:F15})) + ({C:F15})) / (x * (x * {D:F15} + ({E:F15})) + ({F:F15})) + ({G:F15})";
    }

    public void AddValue(double input, double errorWeight)
    {
        ErrorValue newErrorValue = new ErrorValue();
        newErrorValue.Input = input;
        newErrorValue.Reference = ReferenceCurve(input);
        newErrorValue.Approx = ApproxCurve(input);
        newErrorValue.ErrorWeight = errorWeight;

        CalculateError(ref newErrorValue, agxRefLog2MiddleGrey);

        errorValues.Add(newErrorValue);
    }

    public static void CalculateError(ref ErrorValue value, double middleGrey)
    {
        value.ErrorLinear = Math.Abs(value.Reference - value.Approx);
        value.ErrorLog2 = Math.Abs(Math.Log2(Math.Max(value.Reference / middleGrey, 1e-10)) - Math.Log2(Math.Max(value.Approx / middleGrey, 1e-10)));
    }

    public static (double totalErrorLinear, double totalErrorLog2) CalculateTotalError(ref ErrorValue[] errorValues)
    {
        (double totalErrorLinear, double totalErrorLog2) result;
        result.totalErrorLinear = 0.0;
        result.totalErrorLog2 = 0.0;
        foreach (ErrorValue value in errorValues)
        {
            result.totalErrorLinear += value.ErrorLinear * value.ErrorWeight;
            result.totalErrorLog2 += value.ErrorLog2 * value.ErrorWeight;
        }
        return result;
    }

    public static void RefreshApprox(ref ErrorValue[] errorValues, double middleGrey, double a, double b, double c, double d, double e, double f, double g)
    {
        for (int i = 0; i < errorValues.Length; i++)
        {
            errorValues[i].Approx = BasicSecondOrderCurve(errorValues[i].Input, a, b, c, d, e, f, g);
            CalculateError(ref errorValues[i], middleGrey);
        }
    }

    public struct BestResult
    {
        public BestResult() { }
        public double A = 0.0;
        public double B = 0.0;
        public double C = 0.0;
        public double D = 0.0;
        public double E = 0.0;
        public double F = 0.0;
        public double G = 0.0;
        public double totalErrorLinear = 999999;
        public double totalErrorLog2 = 999999;

        /// <summary>
        /// Returns true if A is better than this
        /// </summary>
        public bool isResultBetter(double totalErrorLinear_A, double totalErrorLog2_A)
        {
            return totalErrorLog2_A < this.totalErrorLog2;
            double linearWeight = 10.0;
            return (totalErrorLinear_A * linearWeight + totalErrorLog2_A) < (this.totalErrorLinear * linearWeight + this.totalErrorLog2);
        }
    }

    public struct BruteForceInput
    {
        public BruteForceInput(ErrorValue[] originalErrorValues) { this.originalErrorValues = originalErrorValues; }
        public double A = 0.0;
        public double B = 0.0;
        public double C = 0.0;
        public double D = 0.0;
        public double E = 0.0;
        public double F = 0.0;
        public double G = 0.0;
        public int numSteps = 8;
        public double minHalfRange = 0.05;
        public double half_range_denom = 1.2;
        public ErrorValue[] originalErrorValues;
        public double agxRefLog2MiddleGrey = 0.18;
    }

    public void BruteForceFit()
    {
        BruteForceInput bfInput = new BruteForceInput(errorValues.ToArray());
        bfInput.A = A;
        bfInput.B = B;
        bfInput.C = C;
        bfInput.D = D;
        bfInput.E = E;
        bfInput.F = F;
        bfInput.G = G;
        bfInput.agxRefLog2MiddleGrey = agxRefLog2MiddleGrey;

        int variationsCount = bfInput.numSteps * 2;
        BestResult[] bestResults = new BestResult[variationsCount];
        Parallel.For(0, variationsCount, i => bestResults[i] = BruteForceFitFunction(bfInput, i));

        BestResult bestResult = new BestResult();
        bestResult.A = A;
        bestResult.B = B;
        bestResult.C = C;
        bestResult.D = D;
        bestResult.E = E;
        bestResult.F = F;
        bestResult.G = G;
        ErrorValue[] newErrorValues = new ErrorValue[bfInput.originalErrorValues.Length];
        errorValues.CopyTo(newErrorValues, 0);
        (double totalErrorLinear, double totalErrorLog2) error = CalculateTotalError(ref newErrorValues);
        bestResult.totalErrorLinear = error.totalErrorLinear;
        bestResult.totalErrorLog2 = error.totalErrorLog2;

        for (int i = 0; i < bestResults.Length; i++)
        {
            if (bestResult.isResultBetter(bestResults[i].totalErrorLinear, bestResults[i].totalErrorLog2))
            {
                bestResult = bestResults[i];
            }
        }

        A = bestResult.A;
        B = bestResult.B;
        C = bestResult.C;
        D = bestResult.D;
        E = bestResult.E;
        F = bestResult.F;
        G = bestResult.G;
    }

    public static BestResult BruteForceFitFunction(BruteForceInput bfInput, int A_index)
    {
        double original_A = bfInput.A;
        double original_B = bfInput.B;
        double original_C = bfInput.C;
        double original_D = bfInput.D;
        double original_E = bfInput.E;
        double original_F = bfInput.F;
        double original_G = bfInput.G;

        double this_A = bfInput.A;
        double this_B = bfInput.B;
        double this_C = bfInput.C;
        double this_D = bfInput.D;
        double this_E = bfInput.E;
        double this_F = bfInput.F;
        double this_G = bfInput.G;

        BestResult bestResult = new BestResult();
        bestResult.A = this_A;
        bestResult.B = this_B;
        bestResult.C = this_C;
        bestResult.D = this_D;
        bestResult.E = this_E;
        bestResult.F = this_F;
        bestResult.G = this_G;

        double halfRange_A = Math.Max(bfInput.minHalfRange, Math.Abs(original_A / bfInput.half_range_denom));
        double start_A = original_A - halfRange_A;
        double step_A = Math.Abs(halfRange_A / bfInput.numSteps);
        double max_A = original_A + halfRange_A;
        this_A = start_A + step_A * A_index;

        double halfRange_B = Math.Max(bfInput.minHalfRange, Math.Abs(original_B / bfInput.half_range_denom));
        double start_B = original_B - halfRange_B;
        double step_B = Math.Abs(halfRange_B / bfInput.numSteps);
        double max_B = original_B + halfRange_B;
        for (this_B = start_B; this_B <= max_B; this_B += step_B)
        {
            double halfRange_C = Math.Max(bfInput.minHalfRange, Math.Abs(original_C / bfInput.half_range_denom));
            double start_C = original_C - halfRange_C;
            double step_C = Math.Abs(halfRange_C / bfInput.numSteps);
            double max_C = original_C + halfRange_C;
            for (this_C = start_C; this_C <= max_C; this_C += step_C)
            {
                double halfRange_D = Math.Max(bfInput.minHalfRange, Math.Abs(original_D / bfInput.half_range_denom));
                double start_D = original_D - halfRange_D;
                double step_D = Math.Abs(halfRange_D / bfInput.numSteps);
                double max_D = original_D + halfRange_D;
                for (this_D = start_D; this_D <= max_D; this_D += step_D)
                {
                    double halfRange_E = Math.Max(bfInput.minHalfRange, Math.Abs(original_E / bfInput.half_range_denom));
                    double start_E = original_E - halfRange_E;
                    double step_E = Math.Abs(halfRange_E / bfInput.numSteps);
                    double max_E = original_E + halfRange_E;
                    for (this_E = start_E; this_E <= max_E; this_E += step_E)
                    {
                        double halfRange_F = Math.Max(bfInput.minHalfRange, Math.Abs(original_F / bfInput.half_range_denom));
                        double start_F = original_F - halfRange_F;
                        double step_F = Math.Abs(halfRange_F / bfInput.numSteps);
                        double max_F = original_F + halfRange_F;
                        for (this_F = start_F; this_F <= max_F; this_F += step_F)
                        {
                            double halfRange_G = Math.Max(bfInput.minHalfRange, Math.Abs(original_G / bfInput.half_range_denom));
                            double start_G = original_G - halfRange_G;
                            double step_G = Math.Abs(halfRange_G / bfInput.numSteps);
                            double max_G = original_G + halfRange_G;
                            for (this_G = start_G; this_G <= max_G; this_G += step_G)
                            {
                                ErrorValue[] newErrorValues = new ErrorValue[bfInput.originalErrorValues.Length];
                                bfInput.originalErrorValues.CopyTo(newErrorValues, 0);
                                RefreshApprox(ref newErrorValues, bfInput.agxRefLog2MiddleGrey, this_A, this_B, this_C, this_D, this_E, this_F, this_G);

                                (double totalErrorLinear, double totalErrorLog2) error = CalculateTotalError(ref newErrorValues);
                                if (bestResult.isResultBetter(error.totalErrorLinear, error.totalErrorLog2))
                                {
                                    bestResult.A = this_A;
                                    bestResult.B = this_B;
                                    bestResult.C = this_C;
                                    bestResult.D = this_D;
                                    bestResult.E = this_E;
                                    bestResult.F = this_F;
                                    bestResult.G = this_G;
                                    bestResult.totalErrorLinear = error.totalErrorLinear;
                                    bestResult.totalErrorLog2 = error.totalErrorLog2;
                                }
                            }
                        }
                    }
                }
            }
        }

        return bestResult;
    }

    public double ReferenceCurve(double x)
    {
        return AgxReference(x);
    }

    public double ApproxCurve(double x)
    {
        if (OptionB)
        {
            return AgXLog2Approx(x);
        }
        else
        {
            return BasicSecondOrderCurve(x, A, B, C, D, E, F, G);
        }
    }

    public static double BasicSecondOrderCurve(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        return (x * (a * x + b) + c) / (x * (d * x + e) + f) + g;
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

    public static double BruteForceResult(double x)
    {
        return (x * (x * 26.995000031999972 + (0.291144222600000)) + (-0.000090537000000)) / (x * (x * 26.605032966046089 + (19.804014854971200)) + (0.778588437627000)) + (0.000000000000000);
    }

    public static double AgXLog2Approx(double color)
    {
        const double min_ev = -12.4739311883324; // log2(pow(2, LOG2_MIN) * MIDDLE_GRAY)
        const double max_ev = 4.02606881166759; // log2(pow(2, LOG2_MAX) * MIDDLE_GRAY)
        color = Math.Max(color, 1e-10);

        // Log2 space encoding.
        // Must be clamped because agx_contrast_approx may not work
        // well with values outside of the range [0.0, 1.0]
        color = Math.Clamp(Math.Log2(color), min_ev, max_ev);
        color = (color - min_ev) / (max_ev - min_ev);

        double x = color;
        double x2 = x * x;
        double x4 = x2 * x2;
        color = 0.021 * x + 4.0111 * x2 - 25.682 * x2 * x + 70.359 * x4 - 74.778 * x4 * x + 27.069 * x4 * x2;

        // Convert back to linear before applying outset matrix.
        color = Math.Pow(color, 2.4);

        return color;
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
