// Ported from Google's Material Color Utilities
// (https://github.com/material-foundation/material-color-utilities)
// Original code licensed under the Apache License, Version 2.0.

namespace Material3.UI.Theme.Color;

/// <summary>
/// HCT, hue-chroma-tone. A perceptually-accurate color system based on CAM16 (hue and chroma) and
/// CIE L* (tone), used by Material 3 to construct dynamic color schemes.
/// </summary>
public sealed class Hct
{
    private double _hue;
    private double _chroma;
    private double _tone;
    private int _argb;

    private Hct(int argb)
    {
        SetInternalState(argb);
    }

    /// <summary>Hue of this color, in degrees [0, 360).</summary>
    public double Hue
    {
        get => _hue;
        private set => _hue = value;
    }

    /// <summary>Chroma of this color (>= 0). The maximum representable depends on hue and tone.</summary>
    public double Chroma
    {
        get => _chroma;
        private set => _chroma = value;
    }

    /// <summary>Tone of this color, equivalent to CIE L* (0..100).</summary>
    public double Tone
    {
        get => _tone;
        private set => _tone = value;
    }

    /// <summary>Returns the ARGB integer for this color.</summary>
    public int ToInt() => _argb;

    /// <summary>
    /// Constructs an HCT for the requested hue/chroma/tone. The returned color may have lower
    /// chroma than requested if the requested chroma is unattainable at the given tone.
    /// </summary>
    public static Hct From(double hue, double chroma, double tone)
    {
        int argb = HctSolver.SolveToInt(hue, chroma, tone);
        return new Hct(argb);
    }

    /// <summary>Constructs an HCT from an ARGB color.</summary>
    public static Hct FromInt(int argb) => new Hct(argb);

    /// <summary>Sets the hue and re-renders. Chroma may shift if the requested combo is unrepresentable.</summary>
    public void SetHue(double newHue)
    {
        SetInternalState(HctSolver.SolveToInt(newHue, _chroma, _tone));
    }

    /// <summary>Sets the chroma and re-renders. Chroma may shift if the requested combo is unrepresentable.</summary>
    public void SetChroma(double newChroma)
    {
        SetInternalState(HctSolver.SolveToInt(_hue, newChroma, _tone));
    }

    /// <summary>Sets the tone and re-renders. Chroma may shift if the requested combo is unrepresentable.</summary>
    public void SetTone(double newTone)
    {
        SetInternalState(HctSolver.SolveToInt(_hue, _chroma, newTone));
    }

    /// <summary>
    /// Translates this color into the supplied <see cref="ViewingConditions"/>, returning a fresh
    /// <see cref="Hct"/> whose ARGB is the appearance match in those conditions.
    /// </summary>
    public Hct InViewingConditions(ViewingConditions vc)
    {
        // Translate to CIE XYZ in this VC, then back to ARGB in the default VC.
        Cam16 cam16 = Cam16.FromInt(ToInt());
        double[] viewedInVc = cam16.XyzInViewingConditions(vc);

        // Compute CAM16 in default VC for those XYZ values, find an HCT close to that.
        Cam16 recastInVc = Cam16.FromIntInViewingConditions(
            ColorUtils.ArgbFromXyz(viewedInVc[0], viewedInVc[1], viewedInVc[2]),
            ViewingConditions.Default);

        Hct recastHct = From(
            recastInVc.Hue,
            recastInVc.Chroma,
            ColorUtils.LstarFromY(viewedInVc[1]));
        return recastHct;
    }

    /// <summary>Returns a string in the format "H{hue}C{chroma}T{tone}".</summary>
    public override string ToString() => $"H{_hue}C{_chroma}T{_tone}";

    private void SetInternalState(int argb)
    {
        _argb = argb;
        Cam16 cam = Cam16.FromInt(argb);
        _hue = cam.Hue;
        _chroma = cam.Chroma;
        _tone = ColorUtils.LstarFromArgb(argb);
    }
}
