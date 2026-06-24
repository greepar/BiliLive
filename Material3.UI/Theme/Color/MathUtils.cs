// Ported from Google's Material Color Utilities
// (https://github.com/material-foundation/material-color-utilities)
// Original code licensed under the Apache License, Version 2.0.

using System;

namespace Material3.UI.Theme.Color;

/// <summary>
/// Utility math helpers used by the HCT/CAM16 color science primitives.
/// </summary>
internal static class MathUtils
{
    /// <summary>The sign of <paramref name="num"/>: -1, 0, or 1.</summary>
    public static int Signum(double num)
    {
        if (num < 0) return -1;
        if (num == 0) return 0;
        return 1;
    }

    /// <summary>Linear interpolation between <paramref name="start"/> and <paramref name="stop"/>.</summary>
    public static double Lerp(double start, double stop, double amount) =>
        (1.0 - amount) * start + amount * stop;

    /// <summary>Clamps <paramref name="input"/> into the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static int ClampInt(int min, int max, int input)
    {
        if (input < min) return min;
        if (input > max) return max;
        return input;
    }

    /// <summary>Clamps <paramref name="input"/> into the inclusive range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public static double ClampDouble(double min, double max, double input)
    {
        if (input < min) return min;
        if (input > max) return max;
        return input;
    }

    /// <summary>Wraps an integer degree value into the half-open interval [0, 360).</summary>
    public static int SanitizeDegreesInt(int degrees)
    {
        degrees %= 360;
        if (degrees < 0) degrees += 360;
        return degrees;
    }

    /// <summary>Wraps a degree value into the half-open interval [0, 360).</summary>
    public static double SanitizeDegreesDouble(double degrees)
    {
        degrees %= 360.0;
        if (degrees < 0) degrees += 360.0;
        return degrees;
    }

    /// <summary>
    /// The shortest rotation direction (1 for CCW, -1 for CW) from
    /// <paramref name="from"/> to <paramref name="to"/>, both in degrees.
    /// </summary>
    public static double RotationDirection(double from, double to)
    {
        double increasingDifference = SanitizeDegreesDouble(to - from);
        return increasingDifference <= 180.0 ? 1.0 : -1.0;
    }

    /// <summary>The smallest angular distance between two angles in degrees.</summary>
    public static double DifferenceDegrees(double a, double b) =>
        180.0 - Math.Abs(Math.Abs(a - b) - 180.0);

    /// <summary>
    /// Multiplies a length-3 row vector by a 3x3 matrix and returns the resulting length-3 row vector.
    /// </summary>
    public static double[] MatrixMultiply(double[] row, double[][] matrix)
    {
        double a = row[0] * matrix[0][0] + row[1] * matrix[0][1] + row[2] * matrix[0][2];
        double b = row[0] * matrix[1][0] + row[1] * matrix[1][1] + row[2] * matrix[1][2];
        double c = row[0] * matrix[2][0] + row[1] * matrix[2][1] + row[2] * matrix[2][2];
        return new[] { a, b, c };
    }
}
