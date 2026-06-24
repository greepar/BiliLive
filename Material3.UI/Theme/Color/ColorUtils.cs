// Ported from Google's Material Color Utilities
// (https://github.com/material-foundation/material-color-utilities)
// Original code licensed under the Apache License, Version 2.0.

using System;

namespace Material3.UI.Theme.Color;

/// <summary>
/// Conversions between sRGB integer ARGB, linear RGB, CIE XYZ (D65) and CIE L*.
/// </summary>
internal static class ColorUtils
{
    /// <summary>sRGB (D65) -> CIE XYZ matrix.</summary>
    public static readonly double[][] SRGB_TO_XYZ =
    {
        new[] { 0.41233895, 0.35762064, 0.18051042 },
        new[] { 0.2126,     0.7152,     0.0722 },
        new[] { 0.01932141, 0.11916382, 0.95034478 },
    };

    /// <summary>CIE XYZ -> sRGB (D65) matrix.</summary>
    public static readonly double[][] XYZ_TO_SRGB =
    {
        new[] {  3.2413774792388685, -1.5376652402851851, -0.49885366846268053 },
        new[] { -0.9691452513005321,   1.8758853451067872,  0.04156585616912061 },
        new[] {  0.05562093689691305, -0.20395524564742123, 1.0571799111220335 },
    };

    /// <summary>D65 reference white point in CIE XYZ.</summary>
    public static double[] WhitePointD65() => new[] { 95.047, 100.0, 108.883 };

    private const double KE = 216.0 / 24389.0;
    private const double KKappa = 24389.0 / 27.0;

    /// <summary>Packs r, g, b channels (0..255) into a fully-opaque ARGB integer.</summary>
    public static int ArgbFromRgb(int r, int g, int b) =>
        unchecked((int)0xFF000000) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF);

    /// <summary>Packs linear-RGB components (0..1) into an ARGB integer.</summary>
    public static int ArgbFromLinrgb(double[] linrgb)
    {
        int r = Delinearized(linrgb[0]);
        int g = Delinearized(linrgb[1]);
        int b = Delinearized(linrgb[2]);
        return ArgbFromRgb(r, g, b);
    }

    /// <summary>Alpha channel of an ARGB integer (0..255).</summary>
    public static int AlphaFromArgb(int argb) => (argb >> 24) & 0xFF;

    /// <summary>Red channel of an ARGB integer (0..255).</summary>
    public static int RedFromArgb(int argb) => (argb >> 16) & 0xFF;

    /// <summary>Green channel of an ARGB integer (0..255).</summary>
    public static int GreenFromArgb(int argb) => (argb >> 8) & 0xFF;

    /// <summary>Blue channel of an ARGB integer (0..255).</summary>
    public static int BlueFromArgb(int argb) => argb & 0xFF;

    /// <summary>True if the alpha channel is fully opaque (0xFF).</summary>
    public static bool IsOpaque(int argb) => AlphaFromArgb(argb) >= 255;

    /// <summary>ARGB color from CIE XYZ (D65) coordinates.</summary>
    public static int ArgbFromXyz(double x, double y, double z)
    {
        double[][] matrix = XYZ_TO_SRGB;
        double linearR = matrix[0][0] * x + matrix[0][1] * y + matrix[0][2] * z;
        double linearG = matrix[1][0] * x + matrix[1][1] * y + matrix[1][2] * z;
        double linearB = matrix[2][0] * x + matrix[2][1] * y + matrix[2][2] * z;
        int r = Delinearized(linearR);
        int g = Delinearized(linearG);
        int b = Delinearized(linearB);
        return ArgbFromRgb(r, g, b);
    }

    /// <summary>CIE XYZ (D65) coordinates from an ARGB integer.</summary>
    public static double[] XyzFromArgb(int argb)
    {
        double r = Linearized(RedFromArgb(argb));
        double g = Linearized(GreenFromArgb(argb));
        double b = Linearized(BlueFromArgb(argb));
        return MathUtils.MatrixMultiply(new[] { r, g, b }, SRGB_TO_XYZ);
    }

    /// <summary>Grayscale ARGB color matching the given CIE L* (0..100).</summary>
    public static int ArgbFromLstar(double lstar)
    {
        double y = YFromLstar(lstar);
        int component = Delinearized(y);
        return ArgbFromRgb(component, component, component);
    }

    /// <summary>CIE L* (0..100) from an ARGB integer (uses the Y channel of XYZ).</summary>
    public static double LstarFromArgb(int argb)
    {
        double y = XyzFromArgb(argb)[1];
        return 116.0 * LabF(y / 100.0) - 16.0;
    }

    /// <summary>Linear-RGB Y (0..100) from CIE L* (0..100).</summary>
    public static double YFromLstar(double lstar) => 100.0 * LabInvf((lstar + 16.0) / 116.0);

    /// <summary>CIE L* (0..100) from linear-RGB Y (0..100).</summary>
    public static double LstarFromY(double y) => LabF(y / 100.0) * 116.0 - 16.0;

    /// <summary>Linearizes an sRGB component (0..255 integer) to a 0..100 linear value.</summary>
    public static double Linearized(int rgbComponent)
    {
        double normalized = rgbComponent / 255.0;
        if (normalized <= 0.040449936) return normalized / 12.92 * 100.0;
        return Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
    }

    /// <summary>Delinearizes a 0..100 linear-RGB component to an sRGB byte (0..255).</summary>
    public static int Delinearized(double rgbComponent)
    {
        double normalized = rgbComponent / 100.0;
        double delinearized;
        if (normalized <= 0.0031308) delinearized = normalized * 12.92;
        else delinearized = 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;
        return MathUtils.ClampInt(0, 255, (int)Math.Round(delinearized * 255.0));
    }

    private static double LabF(double t)
    {
        if (t > KE) return Math.Pow(t, 1.0 / 3.0);
        return (KKappa * t + 16.0) / 116.0;
    }

    private static double LabInvf(double ft)
    {
        double ft3 = ft * ft * ft;
        if (ft3 > KE) return ft3;
        return (116.0 * ft - 16.0) / KKappa;
    }
}
