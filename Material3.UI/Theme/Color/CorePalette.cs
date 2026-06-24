using System;

namespace Material3.UI.Theme.Color;

/// <summary>
/// Group of 6 TonalPalettes derived from a single seed color, used to generate Material 3 schemes.
/// </summary>
public class CorePalette
{
    /// <summary>Primary palette.</summary>
    public TonalPalette A1 { get; }

    /// <summary>Secondary palette.</summary>
    public TonalPalette A2 { get; }

    /// <summary>Tertiary palette.</summary>
    public TonalPalette A3 { get; }

    /// <summary>Neutral palette.</summary>
    public TonalPalette N1 { get; }

    /// <summary>Neutral variant palette.</summary>
    public TonalPalette N2 { get; }

    /// <summary>Error palette.</summary>
    public TonalPalette Error { get; }

    private CorePalette(
        TonalPalette a1,
        TonalPalette a2,
        TonalPalette a3,
        TonalPalette n1,
        TonalPalette n2,
        TonalPalette error)
    {
        A1 = a1;
        A2 = a2;
        A3 = a3;
        N1 = n1;
        N2 = n2;
        Error = error;
    }

    /// <summary>Generates a Material 3 palette from a seed color (ARGB), enforcing M3 chroma minimums.</summary>
    public static CorePalette Of(int argb)
    {
        var hct = Hct.FromInt(argb);
        var hue = hct.Hue;
        var chroma = hct.Chroma;

        var a1 = TonalPalette.FromHueAndChroma(hue, Math.Max(48.0, chroma));
        var a2 = TonalPalette.FromHueAndChroma(hue, 16.0);
        var a3 = TonalPalette.FromHueAndChroma(hue + 60.0, 24.0);
        var n1 = TonalPalette.FromHueAndChroma(hue, 4.0);
        var n2 = TonalPalette.FromHueAndChroma(hue, 8.0);
        var error = TonalPalette.FromHueAndChroma(25.0, 84.0);

        return new CorePalette(a1, a2, a3, n1, n2, error);
    }

    /// <summary>Content-style: keep the seed's chroma rather than enforcing M3 minimums. Used for Material You.</summary>
    public static CorePalette ContentOf(int argb)
    {
        var hct = Hct.FromInt(argb);
        var hue = hct.Hue;
        var chroma = hct.Chroma;

        var a1 = TonalPalette.FromHueAndChroma(hue, chroma);
        var a2 = TonalPalette.FromHueAndChroma(hue, Math.Max(chroma / 3.0, 16.0));
        var a3 = TonalPalette.FromHueAndChroma(hue + 60.0, Math.Max(chroma / 2.0, 24.0));
        var n1 = TonalPalette.FromHueAndChroma(hue, Math.Min(chroma / 12.0, 4.0));
        var n2 = TonalPalette.FromHueAndChroma(hue, Math.Min(chroma / 6.0, 8.0));
        var error = TonalPalette.FromHueAndChroma(25.0, 84.0);

        return new CorePalette(a1, a2, a3, n1, n2, error);
    }
}
