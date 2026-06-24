using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Material3.UI.Theme.Color;
using ColorUtils = Material3.UI.Theme.Color.ColorUtils;
using AvaColor = Avalonia.Media.Color;

namespace Material3.UI.Theme;

/// <summary>
/// Runtime color and theme switcher for Material3.UI.
///
/// <para>
/// Use <see cref="SetSeed(AvaColor)"/> to regenerate the entire M3 color scheme
/// from a single seed color (Material You behavior). The scheme replaces the
/// brushes registered by <c>Theme/Colors.axaml</c> at runtime.
/// </para>
///
/// <para>
/// Use <see cref="SetVariant(ThemeVariant)"/> to flip between Light, Dark, or
/// the platform default. This is a thin wrapper over
/// <c>Application.Current.RequestedThemeVariant</c>.
/// </para>
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// The seed color currently in effect. Defaults to the M3 baseline seed (#6750A4).
    /// </summary>
    public static AvaColor CurrentSeed { get; private set; } = AvaColor.Parse("#6750A4");

    /// <summary>
    /// Whether <see cref="SetSeed(AvaColor)"/> uses the Content variant of the palette
    /// (preserves the seed's chroma) instead of the Tonal-Spot baseline.
    /// </summary>
    public static bool UseContentPalette { get; set; } = false;

    /// <summary>Switch theme variant (Light/Dark/Default).</summary>
    public static void SetVariant(ThemeVariant variant)
    {
        var app = Application.Current;
        if (app is null) return;
        app.RequestedThemeVariant = variant;
    }

    /// <summary>
    /// Toggle between Light and Dark. If the current variant is the platform
    /// default the call resolves it first via the actual theme variant.
    /// </summary>
    public static void ToggleLightDark()
    {
        var app = Application.Current;
        if (app is null) return;
        var current = app.ActualThemeVariant;
        SetVariant(current == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    /// <summary>
    /// Replace the active M3 color scheme with one generated from <paramref name="seed"/>.
    /// Both light and dark variant dictionaries are updated; the active variant takes
    /// effect immediately.
    /// </summary>
    public static void SetSeed(AvaColor seed)
    {
        var app = Application.Current;
        if (app is null) return;

        CurrentSeed = seed;
        var seedArgb = ToArgb(seed);

        var lightScheme = UseContentPalette ? M3Scheme.LightContent(seedArgb) : M3Scheme.Light(seedArgb);
        var darkScheme = UseContentPalette ? M3Scheme.DarkContent(seedArgb) : M3Scheme.Dark(seedArgb);

        ApplyScheme(app.Resources, lightScheme, ThemeVariant.Light);
        ApplyScheme(app.Resources, darkScheme, ThemeVariant.Dark);
    }

    /// <summary>
    /// Reset to the M3 baseline seed (#6750A4). The change applies immediately.
    /// </summary>
    public static void ResetSeed() => SetSeed(AvaColor.Parse("#6750A4"));

    private static int ToArgb(AvaColor c)
        => ((int)c.A << 24) | ((int)c.R << 16) | ((int)c.G << 8) | (int)c.B;

    private static AvaColor ToAvaColor(int argb)
        => AvaColor.FromArgb(
            (byte)((argb >> 24) & 0xFF),
            (byte)((argb >> 16) & 0xFF),
            (byte)((argb >> 8) & 0xFF),
            (byte)(argb & 0xFF));

    private static void ApplyScheme(IResourceDictionary root, M3Scheme s, ThemeVariant variant)
    {
        // Find or create the variant-scoped dictionary so DynamicResource bindings
        // are notified when we update brushes.
        if (!root.ThemeDictionaries.TryGetValue(variant, out var bag) || bag is not ResourceDictionary dict)
        {
            dict = new ResourceDictionary();
            root.ThemeDictionaries[variant] = dict;
        }

        SetBrush(dict, "Primary", s.Primary);
        SetBrush(dict, "OnPrimary", s.OnPrimary);
        SetBrush(dict, "PrimaryContainer", s.PrimaryContainer);
        SetBrush(dict, "OnPrimaryContainer", s.OnPrimaryContainer);
        SetBrush(dict, "InversePrimary", s.InversePrimary);
        SetBrush(dict, "PrimaryFixed", s.PrimaryFixed);
        SetBrush(dict, "PrimaryFixedDim", s.PrimaryFixedDim);
        SetBrush(dict, "OnPrimaryFixed", s.OnPrimaryFixed);
        SetBrush(dict, "OnPrimaryFixedVariant", s.OnPrimaryFixedVariant);

        SetBrush(dict, "Secondary", s.Secondary);
        SetBrush(dict, "OnSecondary", s.OnSecondary);
        SetBrush(dict, "SecondaryContainer", s.SecondaryContainer);
        SetBrush(dict, "OnSecondaryContainer", s.OnSecondaryContainer);
        SetBrush(dict, "SecondaryFixed", s.SecondaryFixed);
        SetBrush(dict, "SecondaryFixedDim", s.SecondaryFixedDim);
        SetBrush(dict, "OnSecondaryFixed", s.OnSecondaryFixed);
        SetBrush(dict, "OnSecondaryFixedVariant", s.OnSecondaryFixedVariant);

        SetBrush(dict, "Tertiary", s.Tertiary);
        SetBrush(dict, "OnTertiary", s.OnTertiary);
        SetBrush(dict, "TertiaryContainer", s.TertiaryContainer);
        SetBrush(dict, "OnTertiaryContainer", s.OnTertiaryContainer);
        SetBrush(dict, "TertiaryFixed", s.TertiaryFixed);
        SetBrush(dict, "TertiaryFixedDim", s.TertiaryFixedDim);
        SetBrush(dict, "OnTertiaryFixed", s.OnTertiaryFixed);
        SetBrush(dict, "OnTertiaryFixedVariant", s.OnTertiaryFixedVariant);

        SetBrush(dict, "Error", s.Error);
        SetBrush(dict, "OnError", s.OnError);
        SetBrush(dict, "ErrorContainer", s.ErrorContainer);
        SetBrush(dict, "OnErrorContainer", s.OnErrorContainer);

        SetBrush(dict, "Background", s.Background);
        SetBrush(dict, "OnBackground", s.OnBackground);
        SetBrush(dict, "Surface", s.Surface);
        SetBrush(dict, "OnSurface", s.OnSurface);
        SetBrush(dict, "SurfaceVariant", s.SurfaceVariant);
        SetBrush(dict, "OnSurfaceVariant", s.OnSurfaceVariant);
        SetBrush(dict, "SurfaceDim", s.SurfaceDim);
        SetBrush(dict, "SurfaceBright", s.SurfaceBright);
        SetBrush(dict, "SurfaceContainerLowest", s.SurfaceContainerLowest);
        SetBrush(dict, "SurfaceContainerLow", s.SurfaceContainerLow);
        SetBrush(dict, "SurfaceContainer", s.SurfaceContainer);
        SetBrush(dict, "SurfaceContainerHigh", s.SurfaceContainerHigh);
        SetBrush(dict, "SurfaceContainerHighest", s.SurfaceContainerHighest);
        SetBrush(dict, "InverseSurface", s.InverseSurface);
        SetBrush(dict, "InverseOnSurface", s.InverseOnSurface);
        SetBrush(dict, "SurfaceTint", s.SurfaceTint);

        SetBrush(dict, "Outline", s.Outline);
        SetBrush(dict, "OutlineVariant", s.OutlineVariant);
        SetBrush(dict, "Shadow", s.Shadow);
        SetBrush(dict, "Scrim", s.Scrim);
    }

    private static void SetBrush(IResourceDictionary dict, string key, int argb)
    {
        dict[key] = new ImmutableSolidColorBrush(ToAvaColor(argb));
    }
}
