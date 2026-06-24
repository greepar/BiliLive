using System;
using System.Collections.Generic;

namespace Material3.UI.Theme.Color;

/// <summary>
/// A palette generated from a single (hue, chroma) pair, producing tones 0..100.
/// </summary>
public class TonalPalette
{
    private readonly Dictionary<int, int> _cache = new();

    public double Hue { get; }
    public double Chroma { get; }

    /// <summary>The HCT color used to seed this palette (tone closest to 50 with usable chroma).</summary>
    public Hct KeyColor { get; }

    private TonalPalette(double hue, double chroma, Hct keyColor)
    {
        Hue = hue;
        Chroma = chroma;
        KeyColor = keyColor;
    }

    public static TonalPalette FromInt(int argb)
    {
        var hct = Hct.FromInt(argb);
        return FromHct(hct);
    }

    public static TonalPalette FromHueAndChroma(double hue, double chroma)
    {
        var keyColor = CreateKeyColor(hue, chroma);
        return new TonalPalette(hue, chroma, keyColor);
    }

    public static TonalPalette FromHct(Hct hct)
    {
        return new TonalPalette(hct.Hue, hct.Chroma, hct);
    }

    /// <summary>Returns the ARGB color at the given tone (0..100).</summary>
    public int Tone(int tone)
    {
        if (_cache.TryGetValue(tone, out var cached))
            return cached;

        var argb = Hct.From(Hue, Chroma, tone).ToInt();
        _cache[tone] = argb;
        return argb;
    }

    /// <summary>Returns the HCT color at the given tone (0..100).</summary>
    public Hct GetHct(int tone)
    {
        return Hct.From(Hue, Chroma, tone);
    }

    /// <summary>
    /// Picks the HCT tone closest to 50 that still produces the requested chroma.
    /// Walks outward from tone 50 in 1-tone steps, picking the candidate whose
    /// resulting chroma is closest to the desired value.
    /// </summary>
    private static Hct CreateKeyColor(double hue, double chroma)
    {
        const int startTone = 50;
        var smallestDeltaHct = Hct.From(hue, chroma, startTone);
        var smallestDelta = Math.Abs(smallestDeltaHct.Chroma - chroma);

        for (double delta = 1.0; delta < 50.0; delta += 1.0)
        {
            // If the chroma matches what we asked for to within rounding, we're done.
            if (Math.Round(chroma) == Math.Round(smallestDeltaHct.Chroma))
                return smallestDeltaHct;

            var hctAdd = Hct.From(hue, chroma, startTone + delta);
            var hctAddDelta = Math.Abs(hctAdd.Chroma - chroma);
            if (hctAddDelta < smallestDelta)
            {
                smallestDelta = hctAddDelta;
                smallestDeltaHct = hctAdd;
            }

            var hctSub = Hct.From(hue, chroma, startTone - delta);
            var hctSubDelta = Math.Abs(hctSub.Chroma - chroma);
            if (hctSubDelta < smallestDelta)
            {
                smallestDelta = hctSubDelta;
                smallestDeltaHct = hctSub;
            }
        }

        return smallestDeltaHct;
    }
}
