using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static CurveComparison;

[Tool]
public partial class CurveComparison : Node
{
    [Export]
    public bool OptionB { get; set; } = false;

    [Export] public double insomniac_b = 0.5;
    [Export] public double insomniac_c = 2;
    [Export] public double insomniac_t = 0.7;
    [Export] public double insomniac_s = 0.8;

    [Export] public float jh_toeStrength = 0.0f; // as a ratio
    [Export] public float jh_toeLength = 0.5f; // as a ratio
    [Export] public float jh_shoulderStrength = 0.0f; // as a ratio
    [Export] public float jh_shoulderLength = 0.5f; // in F stops
    [Export] public float jh_shoulderAngle = 0.0f; // as a ratio
    [Export] public float jh_gamma = 1.0f;

    [Export] public double Lottes_A = 1.36989969378897;
    [Export] public double Lottes_D = 0.903916850555009;

    #region Generic paramters as starting point for brute force fitting

    // alternative starting point calculated from John Hable's, but matching AgX
    //[Export] public double A = 1.0526;
    //[Export] public double B = -0.000268387;
    //[Export] public double C = 0.0;
    //[Export] public double D = 1;
    //[Export] public double E = 0.856447;
    //[Export] public double F = 0.00264133;
    //[Export] public double G = 0.0;

    // default to John Hable's Uncharted 2:
    const double exposure_bias = 2.0;
    //[Export] public double A = 0.22;// * exposure_bias * exposure_bias;
    //[Export] public double B = 0.3;// * exposure_bias;
    //[Export] public double C = 0.1;
    //[Export] public double D = 0.2;
    //[Export] public double E = 0.01;
    //[Export] public double F = 0.3;
    //[Export] public double G = 16.291;

    // Starting point for John Hable's, but matching AgX:
    //[Export] public double A = 0.03;
    //[Export] public double B = 0.245;
    //[Export] public double C = -11.37;
    //[Export] public double D = 0.47;
    //[Export] public double E = -22.52;
    //[Export] public double F = 0.37;
    //[Export] public double G = 16.291;

    // self_shadow ACES:
    // use with 
    //[Export] public double A = 1.0;
    //[Export] public double B = 0.0245786;
    //[Export] public double C = -0.000090537;
    //[Export] public double D = 0.983729;
    //[Export] public double E = 0.4329510;
    //[Export] public double F = 0.238081;
    //[Export] public double G = 0.0;

    //const double ballparkStartingPoint = 30.0;
    //[Export] public double A = ballparkStartingPoint;
    //[Export] public double B = ballparkStartingPoint;
    //[Export] public double C = ballparkStartingPoint;
    //[Export] public double D = ballparkStartingPoint;
    //[Export] public double E = ballparkStartingPoint;
    //[Export] public double F = ballparkStartingPoint;
    //[Export] public double G = ballparkStartingPoint;

    // Pretty close with power functions
    [Export] public double A = 307.292;
    [Export] public double B = 278.351;
    [Export] public double C = -0.0832486;
    [Export] public double D = 1501.91;
    [Export] public double E = 1514.37;
    [Export] public double F = 913.696;
    [Export] public double G = 0.0;

    #endregion

    public double agxRefMiddleGrey = 0.18f;
    [Export]
    public double agxRefLog2Min = -10.0f;
    [Export]
    public double agxRefLog2Max = 6.5f;

    [Export]
    public double white = Math.Pow(2.0, 6.5f) * 0.18;

    [Export]
    public double white_hdr = Math.Pow(2.0, 6.5f) * 0.18;

    [Export]
    public double ref_luminance = 200;

    [Export]
    public double max_luminance = 1000;

    [Export]
    public double input_exposure_scale = 1.0;

    [Export]
    public double pivot_x = 118.835;

    public Vector2 reference_inflection_point;
    public Vector2 approx_inflection_point;

    double A_scaled { get { return A / 1000.0; } }
    double B_scaled { get { return B / 1000.0; } }
    double C_scaled { get { return C / 1000.0; } }
    double D_scaled { get { return D / 1000.0; } }
    double E_scaled { get { return E / 1000.0; } }
    double F_scaled { get { return F / 1000.0; } }
    double G_scaled { get { return G / 1000.0; } }

    public double pivot_x_scaled { get { return pivot_x / 1000.0; } }

    public double ReferenceCurve(double x)
    {
        return AgxReference(x, agxRefLog2Max);
        //return TimothyLottes(x);
        //return GodotJohHableUncharted2(x, white);
        //return KrzysztofNarkowiczACES(x);
        //return tonemap_reinhard(x, white);
        //return TimothyLottes_white(x);
        //return BasicSecondOrderCurve(x, A, B, C, D, E, F, G);
        //return JohHableUncharted2(x, A, B, C, D, E, F, white);
        //return TimothyLottes(x);
        //return tonemap_reinhard(x, white);
    }

    public double ApproxCurve(double x)
    {
        if (OptionB)
        {
            return HDRTimothyLottesA(x);
            //return insomniac(x);
            //return JohnHablePiecewise(x);
            //return LearningFunc(x, A, B, C, D, E, F, G);
            //return reinhard_scaled(x, white);
            //return TimothyLottesXStephenHill(x);
            //return TimothyLottes(x, Lottes_A, Lottes_D);
            //return TimothyLottes_white(x);
            //return HDRTimothyLottes2(x);
            //return nonlinearfit_amdform(x);
            //return AgXLog2Approx(x);
            //return AgXNewWhiteParam1(x);
            //return MinimaxApproximation(x);
            //return NonlinearModelFitApproximation2(x);
        }
        else
        {
            return TimothyLottes(x);
            //return ACES2_0(x);
            //return KrzysztofNarkowiczACESFilmRec2020(x);
            //return GodotJohHableUncharted2HDR(x, white);
            //return KrzysztofNarkowiczACESFilmRec2020(x);
            //return HDRTimothyLottes(x);
            //return TimothyLottesModifed(x, Lottes_A_new, Lottes_D_new, Lottes_additional);
            //return RandomNonsense(x);
            //return NonlinearModelFitApproximation(x);
            //return AgXNewWhiteParam(x);
            //return GodotACES(x, A, B, C, D, E, F, G);

            //agxRefLog2Max = Math.Log2(white / 0.18);
            //return AgxReference(x, agxRefLog2Max);
            //return AgxReference(x, agxRefLog2Max);
            //return BruteForceResult2(x);
            //return BasicSecondOrderCurve(x, A, B, C, D, E, F, G);
        }
    }


    //(points[i-1] - points[i-2]).x * v1.y - (points[i-1] - points[i-2]).y * v1.x
    private Vector2 CalculateInflectionPoint(Func<double, double> value, int numSteps = 10000)
    {
        Vector2 result = new Vector2(-1, -1);
        int skipped = 10;
        double[] x_vals = new double[numSteps - skipped];
        double[] y_vals = new double[numSteps - skipped];
        for (int i = skipped; i < numSteps; i++)
        {
            x_vals[i - skipped] = (double)i / numSteps;
            y_vals[i - skipped] = value(x_vals[i - skipped]);
        }

        List<int> inflection_indices = new List<int>();
        bool positive = true;
        for (int i = 1; i < x_vals.Length - 1; i++)
        {
            double v1x = x_vals[i] - x_vals[i - 1];
            double v1y = y_vals[i] - y_vals[i - 1];
            double v2x = x_vals[i + 1] - x_vals[i];
            double v2y = y_vals[i + 1] - y_vals[i];

            double cross = v1x * v2y - v2x * v1y;
            if (cross < 0.0)
            {
                if (positive)
                {
                    positive = false;
                    if (i > 1)
                    {
                        inflection_indices.Add(i);
                    }
                }
            }
            else
            {
                if (!positive)
                {
                    positive = true;
                    if (i > 1)
                    {
                        inflection_indices.Add(i);
                    }
                }
            }
        }

        if (inflection_indices.Count > 1)
        {
            double middleX = (x_vals[inflection_indices[0]] + x_vals[inflection_indices[inflection_indices.Count - 1]]) / 2.0;

            if (numSteps > 10000)
            {
                GD.Print($"Middle: {middleX:N15} Upper: {x_vals[inflection_indices[inflection_indices.Count - 1]]:N15} Lower: {x_vals[inflection_indices[0]]:N15}");
            }

            double middleY = (y_vals[inflection_indices[0]] + y_vals[inflection_indices[inflection_indices.Count - 1]]) / 2.0;
            result = new Vector2((float)middleX, (float)middleY);
        }
        else if (inflection_indices.Count == 1)
        {
            int i = inflection_indices[0];
            result = new Vector2((float)x_vals[i], (float)y_vals[i]);
        }

        return result;
    }

    // Rational polynomial, 2nd order / 2nd order
    public static double BasicSecondOrderCurve(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        return (x * (a * x + b) + c) / (x * (d * x + e) + f) + g;
    }

    // Based on Timothy Lottes' curve
    public static double ComplexPowerCurve(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        a /= 1000.0;
        b /= 1000.0;
        c /= 1000.0;
        d /= 1000.0;
        e /= 1000.0;
        f /= 1000.0;
        g /= 1000.0;

        x = Math.Pow(x, a) / (b / x) + c;
        x = Math.Max(0, x); // x might be negative from C
        return x / (Math.Pow(x, f) * d + e) + g;
    }

    #region Timothy Lottes' curves and variations

    static double timothy_lottes_a;
    static double timothy_lottes_b;
    static double timothy_lottes_c;
    static double timothy_lottes_d;
    public static double TimothyLottes(double x, double mid_in = 0.18, double mid_out = 0.18, double white = 16.2917402385381, double max_val = 1.0, double a = 1.36989969378897, double d = 0.903916850555009)
    {
        double b = (max_val * Math.Pow(mid_in, a) - mid_out * Math.Pow(white, a)) / (max_val * mid_out * (Math.Pow(Math.Pow(mid_in, a), d) - Math.Pow(Math.Pow(white, a), d)));
        double c = (Math.Pow(Math.Pow(mid_in, a), d) * mid_out * Math.Pow(white, a) - max_val * Math.Pow(mid_in, a) * Math.Pow(Math.Pow(white, a), d)) / (max_val * mid_out * (Math.Pow(Math.Pow(mid_in, a), d) - Math.Pow(Math.Pow(white, a), d)));

        timothy_lottes_a = a;
        timothy_lottes_b = b;
        timothy_lottes_c = c;
        timothy_lottes_d = d;

        double z = Math.Pow(x, a);
        return z / (Math.Pow(z, d) * b + c);
    }

    public double TimothyLottes_white(double x)
    {
        return TimothyLottes(x, 0.18, 0.18, white);
    }

    public double HDRTimothyLottesA(double x)
    {
        double max_val = max_luminance / ref_luminance;
        // This is basically the same thing as a 0.18 mid_out with a white of ~0.765 so the contrast is all out of whack 😬
        x = TimothyLottes(x, 0.18, 0.18, 16.2917402385381, max_val, Lottes_A, Lottes_D);
        return x;
    }

    public double HDRTimothyLottesB(double x)
    {
        // This approach gives same result as A, but with more GPU calculations.
        x = TimothyLottes(x, 0.18, 0.18 * (ref_luminance / max_luminance), 16.2917402385381, 1.0, Lottes_A, Lottes_D);
        x *= max_luminance / ref_luminance;
        return x;
    }

    // Timothy Lottes' curve fitted to AgX by Allen Pestaluky and optimized by Stephen Hill
    public double TimothyLottesXStephenHill(double x)
    {
        const double a = 1.36989969378897;
        const double c = 0.3589386656982;
        const double b = 1.4325264680543;
        const double d = 0.903916850555009;
        const double e = a * d;

        //x = exp2(log2(x) * a) / (exp2(log2(exp2(log2(x) * a)) * d) * b + c);

        //x = log2(x);
        //x = exp2(x * a) / (exp2(log2(exp2(x * a)) * d) * b + c);

        x = log2(x);
        x = exp2(x * a) / (exp2(x * a * d) * b + c);

        return x;

        x = Math.Max(x, 1e-10);
        x = Math.Log2(x);
        x = Math.Pow(2.0, x * a) / (Math.Pow(2.0, x * e) * b + c);
        return x;
    }
    #endregion

    #region Insomniac's rienhard-like curve
    public double insomniac(double x)
    {
        double b = insomniac_b;
        double c = insomniac_c;
        double w = white;
        double t = insomniac_t;
        double s = insomniac_s;

        double k = ((1 - t) * (c - b)) / ((1 - s) * (w - c) + (1 - t) * (c - b));
        double toe = (k * (1 - t) * (x - b)) / (c - (1 - t) * b - t * x);
        double shoulder = ((1 - k) * (x - c)) / (s * x + (1 - s) * w - c) + k;
        if (x < c)
        {
            return toe;
        }
        else
        {
            return shoulder;
        }
    }
    #endregion

    #region Godot 4.4 AgX
    public static double AgXLog2PolynomialApprox(double color)
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
    #endregion

    #region Attempts to add white parameter to AgX reference implementation
    // Hacky attempt at adding white paramter to AgxReference implementation
    public double AgXNewWhiteParam1(double x)
    {
        double log2Max = 6.5;
        double white_mapped = AgxReference(white, log2Max);
        //double midgrey_mapped = AgxReference(agxRefMiddleGrey, log2Max);
        double midgrey_mapped = agxRefMiddleGrey / white_mapped;

        input_exposure_scale = agxRefMiddleGrey / midgrey_mapped;

        double white_scale = AgxReference(white, log2Max);
        //double white_scale = AgxReference(input_exposure_scale * white, log2Max);

        double agx = AgxReference(input_exposure_scale * x, log2Max);
        return agx / white_scale;
    }

    // Hacky attempt at adding white paramter to AgxReference implementation
    public double AgXNewWhiteParam(double x)
    {
        double log2Max = 6.5;

        double white_scale = AgxReference(input_exposure_scale * white, log2Max);
        double agx = AgxReference(input_exposure_scale * x, log2Max);
        return agx / white_scale;
    }
    #endregion

    #region AgX Reference Functions

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

    private double CalculateSigmoid(double x, double middleGrey, double minExposure, double maxExposure)
    {
        const double slope = 2.4;
        const double power = 1.5;
        double pivotX = Math.Abs(minExposure / (maxExposure - minExposure));
        double pivotY = Math.Pow(middleGrey, 1.0 / 2.4);

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

    public double AgxReference(double color, double log2Max)
    {
        color = Math.Max(color, 1e-10);

        color = LogEncodingLog2(color, agxRefMiddleGrey, agxRefLog2Min, log2Max);

        // Apply sigmoid function approximation.
        color = CalculateSigmoid(color, agxRefMiddleGrey, agxRefLog2Min, log2Max);

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

    #region Reinhard
    double reinhard_scaled(double color, double white)
    {
        // modified white makes white behave the same between SDR and HDR, but causes apparent brightness to increase.
        white *= ref_luminance / max_luminance;
        white = Math.Max(1.0, white);
        color *= ref_luminance / max_luminance;
        color = tonemap_reinhard(color, white);
        color *= max_luminance / ref_luminance;
        return color;
    }
    double tonemap_reinhard(double color, double white)
    {
        double white_squared = white * white;
        return color * (1 + color / white_squared) / (1 + color);
    }

    double tonemap_reinhard_simple(double color)
    {
        return color / (1 + color);
    }
    #endregion

    #region ACES
    public static double KrzysztofNarkowiczACESFilmRec2020(double x)
    {
        x *= 0.6;
        double a = 15.8f;
        double b = 2.12f;
        double c = 1.2f;
        double d = 5.92f;
        double e = 1.9f;
        return (x * (a * x + b)) / (x * (c * x + d) + e);
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

    public double GodotACES(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        double exposure = 1.8;
        double color_tonemapped = BasicSecondOrderCurve(x * exposure, a, b, c, d, e, f, g);
        double white_tonemapped = BasicSecondOrderCurve(white * exposure, a, b, c, d, e, f, g);
        return color_tonemapped / white_tonemapped;
    }

    public double GodotACES(double x)
    {
        double exposure = 1.8;
        double a = 0.0245786f;
        double b = 0.000090537f;
        double d = 0.983729f;
        double e = 0.432951f;
        double f = 0.238081f;

        double color_tonemapped = BasicSecondOrderCurve(x * exposure, a, b, 0, d, e, f, 0);
        double white_tonemapped = BasicSecondOrderCurve(white * exposure, a, b, 0, d, e, f, 0);
        return color_tonemapped / white_tonemapped;
    }

    public double ACES2_0(double x)
    {
        double linear_scale_factor = 1.0; // In practice, this would be something like this: ref_luminance / 100.0;

        // Preset constants that set the desired behavior for the curve
        double n = max_luminance / linear_scale_factor;

        double n_r = 100;    // normalized white in nits (what 1.0 should be)
        double g = 1.15;       // surround / contrast
        double c = 0.18;       // anchor for 18% grey
        double c_d = 10.013;   // output luminance of 18% grey (in nits)
        double w_g = 0.14;     // change in grey between different peak luminance
        double t_1 = 0.04;     // shadow toe or flare/glare compensation
        double r_hit_min = 128.0;   // scene-referred value "hitting the roof"
        double r_hit_max = 896.0;   // scene-referred value "hitting the roof"

        // Calculate output constants
        double r_hit = r_hit_min + r_hit_max * (Math.Log(n / n_r) / Math.Log(10000.0 / 100.0));
        double m_0 = (n / n_r);
        double m_1 = 0.5 * (m_0 + Math.Sqrt(m_0 * (m_0 + 4.0 * t_1)));
        double u = Math.Pow((r_hit / m_1) / ((r_hit / m_1) + 1), g);
        double m = m_1 / u;
        double w_i = Math.Log(n / 100.0) / Math.Log(2.0);
        double c_t = c_d / n_r * (1.0 + w_i * w_g);
        double g_ip = 0.5 * (c_t + Math.Sqrt(c_t * (c_t + 4.0 * t_1)));
        double g_ipp2 = -(m_1 * Math.Pow((g_ip / m), (1.0 / g))) / (Math.Pow(g_ip / m, 1.0 / g) - 1.0);
        double w_2 = c / g_ipp2;
        double s_2 = w_2 * m_1;
        double u_2 = Math.Pow((r_hit / m_1) / ((r_hit / m_1) + w_2), g);
        double m_2 = m_1 / u_2;

        // forward MM tone scale
        double f = m_2 * Math.Pow(Math.Max(0.0, x) / (x + s_2), g);
        double h = Math.Max(0.0, f * f / (f + t_1));         // forward flare 
        return h * linear_scale_factor;
    }
    #endregion

    #region John Hable's Uncharted 2 functions
    public static double JohHableUncharted2Func(double x, double A, double B, double C, double D, double E, double F)
    {
        return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
    }

    public static double JohHableUncharted2(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        return JohHableUncharted2Func(x, a, b, c, d, e, f)
            / JohHableUncharted2Func(g, a, b, c, d, e, f);
    }

    public double GodotJohHableUncharted2(double x, double white)
    {
        double exposure_bias = 2.0f;
        double A = 0.22f * exposure_bias * exposure_bias; // bias baked into constants for performance
        double B = 0.30f * exposure_bias;
        double C = 0.10f;
        double D = 0.20f;
        double E = 0.01f;
        double F = 0.30f;

        double color_tonemapped = JohHableUncharted2Func(x, A, B, C, D, E, F);
        double white_tonemapped = JohHableUncharted2Func(white, A, B, C, D, E, F);

        return color_tonemapped / white_tonemapped;
    }

    // Broken: contrast goes all out of whack.
    public double GodotJohHableUncharted2HDR(double x, double white)
    {
        double max_val = max_luminance / ref_luminance;
        x /= max_val;
        white /= max_val;
        x = GodotJohHableUncharted2(x, white);
        return x * max_val;
    }

    #endregion

    #region John Hable's Piecewise Power Curves
    public double JohnHablePiecewise(double x)
    {
        CurveParamsUser parameters = new CurveParamsUser();
        parameters.m_toeStrength = jh_toeStrength;
        parameters.m_toeLength = jh_toeLength;
        parameters.m_shoulderStrength = jh_shoulderStrength;
        parameters.m_shoulderLength = jh_shoulderLength;
        parameters.m_shoulderAngle = jh_shoulderAngle;
        parameters.m_gamma = jh_gamma;

        var directParams = CalcDirectParamsFromUser(parameters);
        var curve = CreateCurve(directParams);
        return curve.Eval((float)x);
    }

    public struct CurveParamsUser
    {
        public CurveParamsUser() { }

        public float m_toeStrength = 0.0f; // as a ratio
        public float m_toeLength = 0.5f; // as a ratio
        public float m_shoulderStrength = 0.0f; // as a ratio
        public float m_shoulderLength = 0.5f; // in F stops
        public float m_shoulderAngle = 0.0f; // as a ratio

        public float m_gamma = 1.0f;
    }

    public struct CurveParamsDirect
    {
        public CurveParamsDirect()
        {
            m_x0 = .25f;
            m_y0 = .25f;
            m_x1 = .75f;
            m_y1 = .75f;
            m_W = 1.0f;

            m_gamma = 1.0f;

            m_overshootX = 0.0f;
            m_overshootY = 0.0f;
        }

        public void Reset()
        {
            m_x0 = .25f;
            m_y0 = .25f;
            m_x1 = .75f;
            m_y1 = .75f;
            m_W = 1.0f;

            m_gamma = 1.0f;

            m_overshootX = 0.0f;
            m_overshootY = 0.0f;
        }

        public float m_x0;
        public float m_y0;
        public float m_x1;
        public float m_y1;
        public float m_W;

        public float m_overshootX;
        public float m_overshootY;

        public float m_gamma;
    }

    public struct CurveSegment
    {
        public CurveSegment()
        {
            m_offsetX = 0.0f;
            m_offsetY = 0.0f;
            m_scaleX = 1.0f; // always 1 or -1
            m_scaleY = 0;
            m_lnA = 0.0f;
            m_B = 1.0f;
        }

        public void Reset()
        {
            m_offsetX = 0.0f;
            m_offsetY = 0.0f;
            m_scaleX = 1.0f; // always 1 or -1
            m_lnA = 0.0f;
            m_B = 1.0f;
        }

        public float Eval(float x)
        {
            float x0 = (x - m_offsetX) * m_scaleX;
            float y0 = 0.0f;

            // log(0) is undefined but our function should evaluate to 0. There are better ways to handle this,
            // but it's doing it the slow way here for clarity.
            if (x0 > 0)
            {
                y0 = Mathf.Exp(m_lnA + m_B * Mathf.Log(x0));
            }

            return y0 * m_scaleY + m_offsetY;
        }

        public float EvalInv(float y)
        {
            float y0 = (y - m_offsetY) / m_scaleY;
            float x0 = 0.0f;

            // watch out for log(0) again
            if (y0 > 0)
            {
                x0 = Mathf.Exp((Mathf.Log(y0) - m_lnA) / m_B);
            }
            float x = x0 / m_scaleX + m_offsetX;

            return x;
        }

        public float m_offsetX;
        public float m_offsetY;
        public float m_scaleX; // always 1 or -1
        public float m_scaleY;
        public float m_lnA;
        public float m_B;
    }

    public struct FullCurve
    {
        public FullCurve()
        {
            m_W = 1.0f;
            m_invW = 1.0f;

            m_x0 = .25f;
            m_y0 = .25f;
            m_x1 = .75f;
            m_y1 = .75f;

            m_segments = new CurveSegment[3];
            m_invSegments = new CurveSegment[3];
            for (int i = 0; i < 3; i++)
            {
                m_segments[i].Reset();
                m_invSegments[i].Reset();
            }
        }

        public void Reset()
        {
            m_W = 1.0f;
            m_invW = 1.0f;

            m_x0 = .25f;
            m_y0 = .25f;
            m_x1 = .75f;
            m_y1 = .75f;


            for (int i = 0; i < 3; i++)
            {
                m_segments[i].Reset();
                m_invSegments[i].Reset();
            }
        }

        public float Eval(float srcX)
        {
            float normX = srcX * m_invW;
            int index = (normX < m_x0) ? 0 : ((normX < m_x1) ? 1 : 2);
            CurveSegment segment = m_segments[index];
            float ret = segment.Eval(normX);
            return ret;
        }

        public float EvalInv(float y)
        {
            int index = (y < m_y0) ? 0 : ((y < m_y1) ? 1 : 2);
            CurveSegment segment = m_segments[index];

            float normX = segment.EvalInv(y);
            return normX * m_W;
        }

        public float m_W;
        public float m_invW;

        public float m_x0;
        public float m_x1;
        public float m_y0;
        public float m_y1;


        public CurveSegment[] m_segments;
        public CurveSegment[] m_invSegments;
    }

    // find a function of the form:
    //   f(x) = e^(lnA + Bln(x))
    // where
    //   f(0)   = 0; not really a constraint
    //   f(x0)  = y0
    //   f'(x0) = m
    public static void SolveAB(out float lnA, out float B, float x0, float y0, float m)
    {
        B = (m * x0) / y0;
        lnA = Mathf.Log(y0) - B * Mathf.Log(x0);
    }

    // convert to y=mx+b
    public static void AsSlopeIntercept(out float m, out float b, float x0, float x1, float y0, float y1)
    {
        float dy = (y1 - y0);
        float dx = (x1 - x0);
        if (dx == 0)
            m = 1.0f;
        else
            m = dy / dx;

        b = y0 - x0 * m;
    }

    // f(x) = (mx+b)^g
    // f'(x) = gm(mx+b)^(g-1)
    public static float EvalDerivativeLinearGamma(float m, float b, float g, float x)
    {
        float ret = g * m * Mathf.Pow(m * x + b, g - 1.0f);
        return ret;
    }

    public static FullCurve CreateCurve(CurveParamsDirect srcParams)
    {
        CurveParamsDirect parameters = srcParams;

        FullCurve dstCurve = new FullCurve();
        dstCurve.m_W = srcParams.m_W;
        dstCurve.m_invW = 1.0f / srcParams.m_W;

        // normalize parameters to 1.0 range
        parameters.m_W = 1.0f;
        parameters.m_x0 /= srcParams.m_W;
        parameters.m_x1 /= srcParams.m_W;
        parameters.m_overshootX = srcParams.m_overshootX / srcParams.m_W;

        float toeM = 0.0f;
        float shoulderM = 0.0f;
        float endpointM = 0.0f;
        {
            float m, b;
            AsSlopeIntercept(out m, out b, parameters.m_x0, parameters.m_x1, parameters.m_y0, parameters.m_y1);

            float g = srcParams.m_gamma;

            // base function of linear section plus gamma is
            // y = (mx+b)^g

            // which we can rewrite as
            // y = exp(g*ln(m) + g*ln(x+b/m))

            // and our evaluation function is (skipping the if parts):
            /*
                float x0 = (x - m_offsetX)*m_scaleX;
                y0 = Mathf.Exp(m_lnA + m_B*Mathf.Log(x0));
                return y0*m_scaleY + m_offsetY;
            */

            CurveSegment midSegment;
            midSegment.m_offsetX = -(b / m);
            midSegment.m_offsetY = 0.0f;
            midSegment.m_scaleX = 1.0f;
            midSegment.m_scaleY = 1.0f;
            midSegment.m_lnA = g * Mathf.Log(m);
            midSegment.m_B = g;

            dstCurve.m_segments[1] = midSegment;

            toeM = EvalDerivativeLinearGamma(m, b, g, parameters.m_x0);
            shoulderM = EvalDerivativeLinearGamma(m, b, g, parameters.m_x1);

            // apply gamma to endpoints
            parameters.m_y0 = Mathf.Max(1e-5f, Mathf.Pow(parameters.m_y0, parameters.m_gamma));
            parameters.m_y1 = Mathf.Max(1e-5f, Mathf.Pow(parameters.m_y1, parameters.m_gamma));

            parameters.m_overshootY = Mathf.Pow(1.0f + parameters.m_overshootY, parameters.m_gamma) - 1.0f;
        }

        dstCurve.m_x0 = parameters.m_x0;
        dstCurve.m_x1 = parameters.m_x1;
        dstCurve.m_y0 = parameters.m_y0;
        dstCurve.m_y1 = parameters.m_y1;

        // toe section
        {
            CurveSegment toeSegment;
            toeSegment.m_offsetX = 0;
            toeSegment.m_offsetY = 0.0f;
            toeSegment.m_scaleX = 1.0f;
            toeSegment.m_scaleY = 1.0f;

            SolveAB(out toeSegment.m_lnA, out toeSegment.m_B, parameters.m_x0, parameters.m_y0, toeM);
            dstCurve.m_segments[0] = toeSegment;
        }

        // shoulder section
        {
            // use the simple version that is usually too flat 
            CurveSegment shoulderSegment;

            float x0 = (1.0f + parameters.m_overshootX) - parameters.m_x1;
            float y0 = (1.0f + parameters.m_overshootY) - parameters.m_y1;

            float lnA = 0.0f;
            float B = 0.0f;
            SolveAB(out lnA, out B, x0, y0, shoulderM);

            shoulderSegment.m_offsetX = (1.0f + parameters.m_overshootX);
            shoulderSegment.m_offsetY = (1.0f + parameters.m_overshootY);

            shoulderSegment.m_scaleX = -1.0f;
            shoulderSegment.m_scaleY = -1.0f;
            shoulderSegment.m_lnA = lnA;
            shoulderSegment.m_B = B;

            dstCurve.m_segments[2] = shoulderSegment;
        }

        // Normalize so that we hit 1.0 at our white point. We wouldn't have do this if we 
        // skipped the overshoot part.
        {
            // evaluate shoulder at the end of the curve
            float scale = dstCurve.m_segments[2].Eval(1.0f);
            float invScale = 1.0f / scale;

            dstCurve.m_segments[0].m_offsetY *= invScale;
            dstCurve.m_segments[0].m_scaleY *= invScale;

            dstCurve.m_segments[1].m_offsetY *= invScale;
            dstCurve.m_segments[1].m_scaleY *= invScale;

            dstCurve.m_segments[2].m_offsetY *= invScale;
            dstCurve.m_segments[2].m_scaleY *= invScale;
        }
        return dstCurve;
    }

    public static CurveParamsDirect CalcDirectParamsFromUser(CurveParamsUser srcParams)
    {
        CurveParamsDirect dstParams = new CurveParamsDirect();

        float toeStrength = srcParams.m_toeStrength;
        float toeLength = srcParams.m_toeLength;
        float shoulderStrength = srcParams.m_shoulderStrength;
        float shoulderLength = srcParams.m_shoulderLength;

        float shoulderAngle = srcParams.m_shoulderAngle;
        float gamma = srcParams.m_gamma;

        // This is not actually the display gamma. It's just a UI space to avoid having to 
        // enter small numbers for the input.
        float perceptualGamma = 2.2f;

        // constraints
        {
            toeLength = Mathf.Pow(Saturate(toeLength), perceptualGamma);
            toeStrength = Saturate(toeStrength);
            shoulderAngle = Saturate(shoulderAngle);
            shoulderLength = Mathf.Max(1e-5f, Saturate(shoulderLength));

            shoulderStrength = Mathf.Max(0.0f, shoulderStrength);
        }

        // apply base params
        {
            // toe goes from 0 to 0.5
            float x0 = toeLength * .5f;
            float y0 = (1.0f - toeStrength) * x0; // lerp from 0 to x0

            float remainingY = 1.0f - y0;

            float initialW = x0 + remainingY;

            float y1_offset = (1.0f - shoulderLength) * remainingY;
            float x1 = x0 + y1_offset;
            float y1 = y0 + y1_offset;

            // filmic shoulder strength is in F stops
            float extraW = Mathf.Pow(2, shoulderStrength) - 1.0f;

            float W = initialW + extraW;

            dstParams.m_x0 = x0;
            dstParams.m_y0 = y0;
            dstParams.m_x1 = x1;
            dstParams.m_y1 = y1;
            dstParams.m_W = W;

            // bake the linear to gamma space conversion
            dstParams.m_gamma = gamma;
        }

        dstParams.m_overshootX = (dstParams.m_W * 2.0f) * shoulderAngle * shoulderStrength;
        dstParams.m_overshootY = 0.5f * shoulderAngle * shoulderStrength;

        return dstParams;
    }
    #endregion

    #region Learning and playing around
    // Learning function
    public double LearningFunc(double x, double a, double b, double c, double d, double e, double f, double g)
    {
        double w = white;
        double t = a;
        double s = d;
        double k = ((1 - t) * (c - b)) / ((1 - s) * (w - c) + (1 - t) * (c - b));
        double toe = (k * (1 - t) * (x - b)) / (c - (1 - t) * b - t * x);
        double shoulder = ((1 - k) * (x - c)) / (s * x + (1 - s) * w - c) + k;
        //double toe = (0.0658537 * (-0.5 + x)) / (1.85 - 0.7 * x);
        //double shoulder = 0.219512 + ((0.780488 * (-2 + x)) / (-4.44089 * 10e-16 + 0.8 * x));
        return x > c ? shoulder : toe;


        x = x * (1 / 0.18);
        //x = x * c;
        x = Math.Log2(x);
        //x = x * b;
        x = (x + 10) / 16.5;

        //double x2 = x * x;
        //double x4 = x2 * x2;
        //x = 0.021 * x + 4.0111 * x2 - 25.682 * x2 * x + 70.359 * x4 - 74.778 * x4 * x + 27.069 * x4 * x2;

        //x = ;// / (b / x) + c;
        x = Math.Max(0, x); // x might be negative from C
        x = Math.Pow(x, a) / (Math.Pow(x, b) * c + d);// + e;

        x = Math.Pow(x, 2.4);
        return x;
    }

    // Learning function
    public double RandomNonsense(double x)
    {
        x = Math.Pow(x, A_scaled) / (B_scaled / x) + C_scaled;
        x = Math.Max(0, x); // x might be negative from C
        return x / (Math.Pow(x, F_scaled) * D_scaled + E_scaled) + G_scaled;

        if (x < pivot_x_scaled)
        {
            return Math.Pow(x, A_scaled) / (B_scaled / x) + C_scaled;
        }
        else
        {
            //return RationalInterpolation(x);
            return x / (Math.Pow(x, F_scaled) * D_scaled + E_scaled) + G_scaled;
        }

        //return (0.0000247468 - 0.277006 * x + 774.964 * x * x) / (1 + 1062.35 * x - 1950.91 * x * x);
    }

    // Old attempt at a fit with nonlinearfitmodel in mathematica
    public static double nonlinearfit_amdform(double x)
    {
        x = -529.915417071248 + (0.00188942690895081 / Math.Pow(x, 2.67228665029058));
        return x / (-527.558188047974 + 0.564397144632092 * Math.Pow(x, 1.07961998489504));
    }

    // Attempt at a 2nd order / 2nd order rational polynomial brute force fitting
    public static double BruteForceResult(double x)
    {
        return (x * (x * 26.995000031999972 + (0.291144222600000)) + (-0.000090537000000)) / (x * (x * 26.605032966046089 + (19.804014854971200)) + (0.778588437627000)) + (0.000000000000000);
    }

    // Attempt at a 2nd order / 2nd order rational polynomial brute force fitting
    public static double BruteForceResult2(double x)
    {
        return (x * (x * 0.000072989207176 + (0.006719101119792)) + (-0.000002255264375)) / (x * (x * 0.000039693739511 + (0.006902476363533)) + (0.005847131799769)) + (0.000000000000000);
    }

    public static double RationalInterpolation(double x)
    {
        return (-0.00169047 + 1.49227 * x + 0.194039 * x * x) / (1 + 1.86073 * x + 0.167678 * x * x);
    }

    public static double MinimaxApproximation(double x)
    {
        return (-0.000264666 + 1.50561 * x + 0.225389 * x * x) / (1 + 1.91729 * x + 0.196494 * x * x);
    }

    public static double NonlinearModelFitApproximation(double x)
    {
        return (0.000659361441666740 - 0.696889279695934 * x + 155.821863293030 * x * x) / (0.701797928424950 + 122.551728247573 * x + 155.782469136453 * x * x);
    }

    public static double NonlinearModelFitApproximation2(double x)
    {
        return (0.000759321162862364 - 0.764868897313159 * x + 166.417533651432 * x * x) / (0.676982309500238 + 131.398917467506 * x + 166.074755402244 * x * x);
    }
    #endregion

    #region Helper functions
    public static double exp2(double x)
    {
        return Math.Pow(2.0, x);
    }

    public static double log2(double x)
    {
        return Math.Log2(x);
    }
    public static float Saturate(float x)
    {
        return Mathf.Max(0.0f, Mathf.Min(1.0f, x));
    }
    #endregion

    #region Brute force fitting, error calculaiton, etc.

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

        //AddValue(Math.Pow(2, agxRefLog2Min) * agxRefMiddleGrey, 1.0);
        //AddValue(Math.Pow(2, -9.0) * agxRefMiddleGrey, 1.0);
        //AddValue(Math.Pow(2, -8.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -7.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -6.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -5.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -4.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -3.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -2.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, -1.0) * agxRefMiddleGrey, 1.0);
        AddValue(agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 1.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 2.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 3.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 3.5) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 4.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 4.5) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 5.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 5.5) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, 6.0) * agxRefMiddleGrey, 1.0);
        AddValue(Math.Pow(2, agxRefLog2Max) * agxRefMiddleGrey, 1.0);

        ErrorValue[] errorValuesArray = new ErrorValue[errorValues.Count];
        errorValues.CopyTo(errorValuesArray, 0);
        (double totalErrorLinear, double totalErrorLog2) error = CalculateTotalError(ref errorValuesArray);

        foreach (ErrorValue value in errorValues)
        {
            TreeItem treeItem = tree.CreateItem();
            treeItem.SetText(0, $"{value.Input:F7}");
            treeItem.SetText(1, $"{value.Reference:F7}");
            treeItem.SetText(2, $"{value.Approx:F7}");
            treeItem.SetText(3, $"{value.ErrorLinear:F7}");
            treeItem.SetText(4, $"{value.ErrorLog2:F7}");
            treeItem.SetText(5, $"{value.ErrorWeight:F7}");
        }
        GetNode<Label>("%TotalErrorLinearLabel").Text = $"Total weighted error (linear): {error.totalErrorLinear:F7}";
        GetNode<Label>("%TotalErrorLog2Label").Text = $"Total weighted error (log2, middle grey: {agxRefMiddleGrey:F2}): {error.totalErrorLog2:F7}";
        GetNode<TextEdit>("%RationalApproxTextEdit").Text = $"(x * (x * {A:F15} + ({B:F15})) + ({C:F15})) / (x * (x * {D:F15} + ({E:F15})) + ({F:F15})) + ({G:F15})\n\nx  = pow(x, vec3({(A / 1000.0):F15})) / ({(B / 1000.0):F15} / x) + {(C / 1000.0):F15};\nx = max(x, 0.0); // x might be negative from c\nx = x / (pow(x, vec3({(F / 1000.0):F15})) * {(D / 1000.0):F15} + {(E / 1000.0):F15});" +
            $"\n\nx = pow(x, vec3({timothy_lottes_a:F15}));\nx = x / (pow(x, vec3({timothy_lottes_d:F15})) * {timothy_lottes_b:F15} + {timothy_lottes_c:F15});";

        reference_inflection_point = CalculateInflectionPoint((double x) => { return ReferenceCurve(x); });
        approx_inflection_point = CalculateInflectionPoint((double x) => { return ApproxCurve(x); });

        if (GD.Randf() < 0.1)
        {
            //CalculateInflectionPoint((double x) => { return ReferenceCurve(x); }, 1000000);
        }
    }

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
        public int numSteps = 10;
        public double minHalfRange = 0.000005;
        public double half_range_denom = 1.5;
        public ErrorValue[] originalErrorValues;
        public double agxRefLog2MiddleGrey = 0.18;
    }

    public void AddValue(double input, double errorWeight)
    {
        ErrorValue newErrorValue = new ErrorValue();
        newErrorValue.Input = input;
        newErrorValue.Reference = ReferenceCurve(input);
        newErrorValue.Approx = ApproxCurve(input);
        newErrorValue.ErrorWeight = errorWeight;

        CalculateError(ref newErrorValue, agxRefMiddleGrey);

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
        for (int i = 0; i < errorValues.Length; i++)
        {
            double thisWeight = errorValues[i].ErrorWeight;
            if ((i == 0 && errorValues[i].Approx > Math.Max(1e-4, errorValues[i].Reference))
                || (i == errorValues.Length - 1 && errorValues[i].Approx < Math.Min(1.0 - 1e-4, errorValues[i].Reference)))
            {
                //thisWeight *= 100.0;
            }
            else
            {
                double pivot_x = 0.118835;
                if (errorValues[i].Input < pivot_x && errorValues[i].Approx > errorValues[i].Reference)
                {
                    //thisWeight *= 10.0;
                }
                else if (errorValues[i].Input > pivot_x && errorValues[i].Approx < errorValues[i].Reference)
                {
                    //thisWeight *= 10.0;
                }
            }
            result.totalErrorLinear += errorValues[i].ErrorLinear * thisWeight;
            result.totalErrorLog2 += errorValues[i].ErrorLog2 * thisWeight;
        }
        return result;
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
            //return totalErrorLinear_A < this.totalErrorLinear;
            return totalErrorLog2_A < this.totalErrorLog2;
            double linearWeight = 10.0;
            return (totalErrorLinear_A * linearWeight + totalErrorLog2_A) < (this.totalErrorLinear * linearWeight + this.totalErrorLog2);
        }
    }

    public static void RefreshApprox(ref ErrorValue[] errorValues, double middleGrey, double a, double b, double c, double d, double e, double f, double g)
    {
        for (int i = 0; i < errorValues.Length; i++)
        {
            //errorValues[i].Approx = JohHableUncharted2(errorValues[i].Input, a, b, c, d, e, f, g);
            //errorValues[i].Approx = BasicSecondOrderCurve(errorValues[i].Input, a, b, c, d, e, f, g);
            errorValues[i].Approx = ComplexPowerCurve(errorValues[i].Input, a, b, c, d, e, f, g);
            CalculateError(ref errorValues[i], middleGrey);
        }
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
        bfInput.agxRefLog2MiddleGrey = agxRefMiddleGrey;

        //int variationsCount = bfInput.numSteps * 2 + 3;
        //BestResult[] bestResults = new BestResult[variationsCount];
        //Parallel.For(0, variationsCount, i => bestResults[i] = BruteForceBallparkFunction(bfInput, i));
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

    public static BestResult BruteForceBallparkFunction(BruteForceInput bfInput, int A_index)
    {
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
        ErrorValue[] newErrorValues = new ErrorValue[bfInput.originalErrorValues.Length];

        double[] values = new double[bfInput.numSteps * 2 + 3];
        values[0] = bfInput.A;
        for (int i = 1; i < bfInput.numSteps * 2; i++)
        {
            values[i] = values[i - 1] / (i % 2 == 0 ? -1.0 * bfInput.half_range_denom : -1.0);
        }
        values[values.Length - 1] = 0.0;
        values[values.Length - 2] = 1.0;
        values[values.Length - 3] = -1.0;

        this_A = values[A_index];
        for (int i_B = 0; i_B < values.Length; i_B++)
        {
            this_B = values[i_B];
            for (int i_C = 0; i_C < values.Length; i_C++)
            {
                this_C = values[i_C] / 1000.0;
                for (int i_D = 0; i_D < values.Length; i_D++)
                {
                    this_D = values[i_D];
                    for (int i_E = 0; i_E < values.Length; i_E++)
                    {
                        this_E = values[i_E];
                        for (int i_F = 0; i_F < values.Length; i_F++)
                        {
                            this_F = values[i_F];
                            //for (int i_G = 0; i_G < values.Length; i_G++)
                            {
                                //this_G = values[i_G];
                                this_G = 0.0;
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

    public static BestResult BruteForceFitFunction(BruteForceInput bfInput, int A_index)
    {
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
        ErrorValue[] newErrorValues = new ErrorValue[bfInput.originalErrorValues.Length];

        double halfRange_A = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.A / bfInput.half_range_denom));
        double start_A = bfInput.A - halfRange_A;
        double step_A = Math.Abs(halfRange_A / bfInput.numSteps);
        double max_A = bfInput.A + halfRange_A;
        this_A = start_A + step_A * A_index;

        double halfRange_B = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.B / bfInput.half_range_denom));
        double start_B = bfInput.B - halfRange_B;
        double step_B = Math.Abs(halfRange_B / bfInput.numSteps);
        double max_B = bfInput.B + halfRange_B;
        for (this_B = start_B; this_B <= max_B; this_B += step_B)
        {
            double halfRange_C = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.C / bfInput.half_range_denom));
            double start_C = bfInput.C - halfRange_C;
            double step_C = Math.Abs(halfRange_C / bfInput.numSteps);
            double max_C = bfInput.C + halfRange_C;
            for (this_C = start_C; this_C <= max_C; this_C += step_C)
            {
                double halfRange_D = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.D / bfInput.half_range_denom));
                double start_D = bfInput.D - halfRange_D;
                double step_D = Math.Abs(halfRange_D / bfInput.numSteps);
                double max_D = bfInput.D + halfRange_D;
                for (this_D = start_D; this_D <= max_D; this_D += step_D)
                {
                    double halfRange_E = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.E / bfInput.half_range_denom));
                    double start_E = bfInput.E - halfRange_E;
                    double step_E = Math.Abs(halfRange_E / bfInput.numSteps);
                    double max_E = bfInput.E + halfRange_E;
                    for (this_E = start_E; this_E <= max_E; this_E += step_E)
                    {
                        double halfRange_F = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.F / bfInput.half_range_denom));
                        double start_F = bfInput.F - halfRange_F;
                        double step_F = Math.Abs(halfRange_F / bfInput.numSteps);
                        double max_F = bfInput.F + halfRange_F;
                        for (this_F = start_F; this_F <= max_F; this_F += step_F)
                        {
                            //double halfRange_G = Math.Max(bfInput.minHalfRange, Math.Abs(bfInput.G / bfInput.half_range_denom));
                            //double start_G = bfInput.G - halfRange_G;
                            //double step_G = Math.Abs(halfRange_G / bfInput.numSteps);
                            //double max_G = bfInput.G + halfRange_G;
                            //for (this_G = start_G; this_G <= max_G; this_G += step_G)
                            {
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
    #endregion

}
