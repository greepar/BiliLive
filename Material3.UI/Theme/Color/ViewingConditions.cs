// Ported from Google's Material Color Utilities
// (https://github.com/material-foundation/material-color-utilities)
// Original code licensed under the Apache License, Version 2.0.

using System;

namespace Material3.UI.Theme.Color;

/// <summary>
/// In traditional color spaces, a color can be identified solely by the observer's measurement of
/// the color. Color appearance models such as CAM16 also consider the viewer's environment, the
/// "viewing conditions". <see cref="ViewingConditions"/> captures the parameters required by CAM16
/// and HCT.
/// </summary>
public sealed class ViewingConditions
{
    /// <summary>Background, expressed as a fraction of the adapting field. (Adapted)</summary>
    public double N { get; }

    /// <summary>Achromatic response of the white point under these viewing conditions.</summary>
    public double Aw { get; }

    /// <summary>Brightness inducing factor for the background.</summary>
    public double Nbb { get; }

    /// <summary>Chromatic induction factor.</summary>
    public double Ncb { get; }

    /// <summary>Surround / impact of the surround on the appearance.</summary>
    public double C { get; }

    /// <summary>Chromatic surround induction factor (also called Nc).</summary>
    public double NcF { get; }

    /// <summary>Alias of <see cref="NcF"/>.</summary>
    public double Nc => NcF;

    /// <summary>Luminance level adaptation factor.</summary>
    public double FL { get; }

    /// <summary>FL ^ 0.25, precomputed.</summary>
    public double FLRoot { get; }

    /// <summary>Base exponential nonlinearity (1.48 + sqrt(n)).</summary>
    public double Z { get; }

    /// <summary>RGB cone response of the white point after chromatic adaptation.</summary>
    public double[] RgbD { get; }

    /// <summary>Default sRGB / D65 viewing conditions.</summary>
    public static readonly ViewingConditions Default = DefaultWithBackgroundLstar(50.0);

    private ViewingConditions(
        double n,
        double aw,
        double nbb,
        double ncb,
        double c,
        double nc,
        double[] rgbD,
        double fl,
        double flRoot,
        double z)
    {
        N = n;
        Aw = aw;
        Nbb = nbb;
        Ncb = ncb;
        C = c;
        NcF = nc;
        RgbD = rgbD;
        FL = fl;
        FLRoot = flRoot;
        Z = z;
    }

    /// <summary>
    /// Creates <see cref="ViewingConditions"/> for the standard sRGB / D65 environment with the
    /// supplied background lightness L*.
    /// </summary>
    public static ViewingConditions DefaultWithBackgroundLstar(double lstar) =>
        Make(
            ColorUtils.WhitePointD65(),
            (200.0 / Math.PI) * ColorUtils.YFromLstar(50.0) / 100.0,
            lstar,
            2.0,
            false);

    /// <summary>
    /// Creates a <see cref="ViewingConditions"/> describing the supplied environment.
    /// </summary>
    /// <param name="whitePoint">White point in CIE XYZ (Y=100); defaults to D65 if null.</param>
    /// <param name="adaptingLuminance">Luminance of the adapting field, cd/m^2.</param>
    /// <param name="backgroundLstar">Background L* (0..100).</param>
    /// <param name="surround">0 = dark, 1 = dim, 2 = average.</param>
    /// <param name="discountingIlluminant">If true, treat the eye as fully adapted to the illuminant.</param>
    public static ViewingConditions Make(
        double[]? whitePoint = null,
        double adaptingLuminance = -1.0,
        double backgroundLstar = 50.0,
        double surround = 2.0,
        bool discountingIlluminant = false)
    {
        whitePoint ??= ColorUtils.WhitePointD65();
        if (adaptingLuminance < 0)
        {
            adaptingLuminance = (200.0 / Math.PI) * ColorUtils.YFromLstar(50.0) / 100.0;
        }
        // Avoid pathologically dark backgrounds.
        backgroundLstar = Math.Max(0.1, backgroundLstar);

        // Transform white point XYZ -> 'cone'/'rgb' responses via M16.
        double[][] matrix = Cam16.XyzToCam16Rgb;
        double[] xyz = whitePoint;
        double rW = (xyz[0] * matrix[0][0]) + (xyz[1] * matrix[0][1]) + (xyz[2] * matrix[0][2]);
        double gW = (xyz[0] * matrix[1][0]) + (xyz[1] * matrix[1][1]) + (xyz[2] * matrix[1][2]);
        double bW = (xyz[0] * matrix[2][0]) + (xyz[1] * matrix[2][1]) + (xyz[2] * matrix[2][2]);

        double f = 0.8 + (surround / 10.0);
        double c = (f >= 0.9)
            ? MathUtils.Lerp(0.59, 0.69, (f - 0.9) * 10.0)
            : MathUtils.Lerp(0.525, 0.59, (f - 0.8) * 10.0);

        double d = discountingIlluminant
            ? 1.0
            : f * (1.0 - ((1.0 / 3.6) * Math.Exp((-adaptingLuminance - 42.0) / 92.0)));
        d = MathUtils.ClampDouble(0.0, 1.0, d);
        double nc = f;

        double[] rgbD =
        {
            d * (100.0 / rW) + 1.0 - d,
            d * (100.0 / gW) + 1.0 - d,
            d * (100.0 / bW) + 1.0 - d,
        };

        double k = 1.0 / (5.0 * adaptingLuminance + 1.0);
        double k4 = k * k * k * k;
        double k4MinusOne = 1.0 - k4;
        double fl = (k4 * adaptingLuminance) +
                    (0.1 * k4MinusOne * k4MinusOne * Math.Cbrt(5.0 * adaptingLuminance));

        double n = ColorUtils.YFromLstar(backgroundLstar) / whitePoint[1];
        double z = 1.48 + Math.Sqrt(n);
        double nbb = 0.725 / Math.Pow(n, 0.2);
        double ncb = nbb;

        double[] rgbAFactors =
        {
            Math.Pow(fl * rgbD[0] * rW / 100.0, 0.42),
            Math.Pow(fl * rgbD[1] * gW / 100.0, 0.42),
            Math.Pow(fl * rgbD[2] * bW / 100.0, 0.42),
        };

        double[] rgbA =
        {
            (400.0 * rgbAFactors[0]) / (rgbAFactors[0] + 27.13),
            (400.0 * rgbAFactors[1]) / (rgbAFactors[1] + 27.13),
            (400.0 * rgbAFactors[2]) / (rgbAFactors[2] + 27.13),
        };

        double aw = ((2.0 * rgbA[0]) + rgbA[1] + (0.05 * rgbA[2])) * nbb;

        return new ViewingConditions(n, aw, nbb, ncb, c, nc, rgbD, fl, Math.Pow(fl, 0.25), z);
    }
}
