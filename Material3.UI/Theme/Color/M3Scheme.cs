namespace Material3.UI.Theme.Color;

/// <summary>
/// Final mapping of palettes + variant + dark/light to all M3 role colors as ARGB ints.
/// Property names match the resource keys used in Material3.UI/Theme/Colors.axaml.
/// </summary>
public class M3Scheme
{
    public bool IsDark { get; }
    public CorePalette Palette { get; }

    public int Primary { get; }
    public int OnPrimary { get; }
    public int PrimaryContainer { get; }
    public int OnPrimaryContainer { get; }
    public int InversePrimary { get; }
    public int PrimaryFixed { get; }
    public int PrimaryFixedDim { get; }
    public int OnPrimaryFixed { get; }
    public int OnPrimaryFixedVariant { get; }

    public int Secondary { get; }
    public int OnSecondary { get; }
    public int SecondaryContainer { get; }
    public int OnSecondaryContainer { get; }
    public int SecondaryFixed { get; }
    public int SecondaryFixedDim { get; }
    public int OnSecondaryFixed { get; }
    public int OnSecondaryFixedVariant { get; }

    public int Tertiary { get; }
    public int OnTertiary { get; }
    public int TertiaryContainer { get; }
    public int OnTertiaryContainer { get; }
    public int TertiaryFixed { get; }
    public int TertiaryFixedDim { get; }
    public int OnTertiaryFixed { get; }
    public int OnTertiaryFixedVariant { get; }

    public int Error { get; }
    public int OnError { get; }
    public int ErrorContainer { get; }
    public int OnErrorContainer { get; }

    public int Background { get; }
    public int OnBackground { get; }
    public int Surface { get; }
    public int OnSurface { get; }
    public int SurfaceVariant { get; }
    public int OnSurfaceVariant { get; }
    public int SurfaceDim { get; }
    public int SurfaceBright { get; }
    public int SurfaceContainerLowest { get; }
    public int SurfaceContainerLow { get; }
    public int SurfaceContainer { get; }
    public int SurfaceContainerHigh { get; }
    public int SurfaceContainerHighest { get; }
    public int InverseSurface { get; }
    public int InverseOnSurface { get; }
    public int SurfaceTint { get; }

    public int Outline { get; }
    public int OutlineVariant { get; }
    public int Shadow { get; }
    public int Scrim { get; }

    private M3Scheme(CorePalette palette, bool isDark)
    {
        Palette = palette;
        IsDark = isDark;

        var a1 = palette.A1;
        var a2 = palette.A2;
        var a3 = palette.A3;
        var n1 = palette.N1;
        var n2 = palette.N2;
        var err = palette.Error;

        const int black = unchecked((int)0xFF000000);

        if (!isDark)
        {
            // Light spec
            Primary = a1.Tone(40);
            OnPrimary = a1.Tone(100);
            PrimaryContainer = a1.Tone(90);
            OnPrimaryContainer = a1.Tone(10);
            InversePrimary = a1.Tone(80);
            PrimaryFixed = a1.Tone(90);
            PrimaryFixedDim = a1.Tone(80);
            OnPrimaryFixed = a1.Tone(10);
            OnPrimaryFixedVariant = a1.Tone(30);

            Secondary = a2.Tone(40);
            OnSecondary = a2.Tone(100);
            SecondaryContainer = a2.Tone(90);
            OnSecondaryContainer = a2.Tone(10);
            SecondaryFixed = a2.Tone(90);
            SecondaryFixedDim = a2.Tone(80);
            OnSecondaryFixed = a2.Tone(10);
            OnSecondaryFixedVariant = a2.Tone(30);

            Tertiary = a3.Tone(40);
            OnTertiary = a3.Tone(100);
            TertiaryContainer = a3.Tone(90);
            OnTertiaryContainer = a3.Tone(10);
            TertiaryFixed = a3.Tone(90);
            TertiaryFixedDim = a3.Tone(80);
            OnTertiaryFixed = a3.Tone(10);
            OnTertiaryFixedVariant = a3.Tone(30);

            Error = err.Tone(40);
            OnError = err.Tone(100);
            ErrorContainer = err.Tone(90);
            OnErrorContainer = err.Tone(10);

            Background = n1.Tone(98);
            OnBackground = n1.Tone(10);
            Surface = n1.Tone(98);
            OnSurface = n1.Tone(10);
            SurfaceVariant = n2.Tone(90);
            OnSurfaceVariant = n2.Tone(30);
            SurfaceDim = n1.Tone(87);
            SurfaceBright = n1.Tone(98);
            SurfaceContainerLowest = n1.Tone(100);
            SurfaceContainerLow = n1.Tone(96);
            SurfaceContainer = n1.Tone(94);
            SurfaceContainerHigh = n1.Tone(92);
            SurfaceContainerHighest = n1.Tone(90);
            InverseSurface = n1.Tone(20);
            InverseOnSurface = n1.Tone(95);
            SurfaceTint = a1.Tone(40);

            Outline = n2.Tone(50);
            OutlineVariant = n2.Tone(80);
            Shadow = black;
            Scrim = black;
        }
        else
        {
            // Dark spec
            Primary = a1.Tone(80);
            OnPrimary = a1.Tone(20);
            PrimaryContainer = a1.Tone(30);
            OnPrimaryContainer = a1.Tone(90);
            InversePrimary = a1.Tone(40);
            PrimaryFixed = a1.Tone(90);
            PrimaryFixedDim = a1.Tone(80);
            OnPrimaryFixed = a1.Tone(10);
            OnPrimaryFixedVariant = a1.Tone(30);

            Secondary = a2.Tone(80);
            OnSecondary = a2.Tone(20);
            SecondaryContainer = a2.Tone(30);
            OnSecondaryContainer = a2.Tone(90);
            SecondaryFixed = a2.Tone(90);
            SecondaryFixedDim = a2.Tone(80);
            OnSecondaryFixed = a2.Tone(10);
            OnSecondaryFixedVariant = a2.Tone(30);

            Tertiary = a3.Tone(80);
            OnTertiary = a3.Tone(20);
            TertiaryContainer = a3.Tone(30);
            OnTertiaryContainer = a3.Tone(90);
            TertiaryFixed = a3.Tone(90);
            TertiaryFixedDim = a3.Tone(80);
            OnTertiaryFixed = a3.Tone(10);
            OnTertiaryFixedVariant = a3.Tone(30);

            Error = err.Tone(80);
            OnError = err.Tone(20);
            ErrorContainer = err.Tone(30);
            OnErrorContainer = err.Tone(90);

            Background = n1.Tone(6);
            OnBackground = n1.Tone(90);
            Surface = n1.Tone(6);
            OnSurface = n1.Tone(90);
            SurfaceVariant = n2.Tone(30);
            OnSurfaceVariant = n2.Tone(80);
            SurfaceDim = n1.Tone(6);
            SurfaceBright = n1.Tone(24);
            SurfaceContainerLowest = n1.Tone(4);
            SurfaceContainerLow = n1.Tone(10);
            SurfaceContainer = n1.Tone(12);
            SurfaceContainerHigh = n1.Tone(17);
            SurfaceContainerHighest = n1.Tone(22);
            InverseSurface = n1.Tone(90);
            InverseOnSurface = n1.Tone(20);
            SurfaceTint = a1.Tone(80);

            Outline = n2.Tone(60);
            OutlineVariant = n2.Tone(30);
            Shadow = black;
            Scrim = black;
        }
    }

    public static M3Scheme Light(int seedArgb)
    {
        return new M3Scheme(CorePalette.Of(seedArgb), false);
    }

    public static M3Scheme Dark(int seedArgb)
    {
        return new M3Scheme(CorePalette.Of(seedArgb), true);
    }

    public static M3Scheme LightContent(int seedArgb)
    {
        return new M3Scheme(CorePalette.ContentOf(seedArgb), false);
    }

    public static M3Scheme DarkContent(int seedArgb)
    {
        return new M3Scheme(CorePalette.ContentOf(seedArgb), true);
    }

    public static M3Scheme FromCorePalette(CorePalette palette, bool isDark)
    {
        return new M3Scheme(palette, isDark);
    }
}
